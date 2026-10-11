using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class BargainApiConfig
{
    public string endpoint = "https://api.openai.com/v1/chat/completions";
    public string apiKey = "";
    public string model = "gpt-4o-mini";
}

[System.Serializable]
public class ImpressionResult
{
    public int basePrice;
    public string comment;
    public int mood;
}

[System.Serializable]
public class TalkResult
{
    public string reply;
    public int talkScore;
    public bool accepted;
    public int mood;
}

public class BargainApi
{
    public static BargainApiConfig Config { get; private set; }

    public static bool Ready => Config != null && !string.IsNullOrWhiteSpace(Config.apiKey);

    public static void Load()
    {
        string directory = Application.streamingAssetsPath;
        string path = Path.Combine(directory, "bargain_api.json");
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, "{\n  \"endpoint\": \"https://api.openai.com/v1/chat/completions\",\n  \"apiKey\": \"\",\n  \"model\": \"gpt-4o-mini\"\n}\n");
        }
        Config = JsonUtility.FromJson<BargainApiConfig>(File.ReadAllText(path));
        if (Config == null)
            Config = new BargainApiConfig();
        if (string.IsNullOrWhiteSpace(Config.endpoint))
            Config.endpoint = "https://api.openai.com/v1/chat/completions";
        if (string.IsNullOrWhiteSpace(Config.model))
            Config.model = "gpt-4o-mini";
    }

    public static IEnumerator RequestImpression(string orderTitle, string brief, List<byte[]> pngs, Action<ImpressionResult, string> onDone)
    {
        StringBuilder images = new StringBuilder();
        for (int i = 0; i < pngs.Count; i++)
        {
            if (i > 0)
                images.Append(',');
            string name = i < SubmitCapture.ViewNames.Length ? SubmitCapture.ViewNames[i] : "视角" + (i + 1);
            images.Append("{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,");
            images.Append(Convert.ToBase64String(pngs[i]));
            images.Append("\"}},{\"type\":\"text\",\"text\":\"");
            images.Append(Esc(name));
            images.Append("\"}");
        }
        string prompt = "你是委托人，正在看一件刚做好的雕塑。下面按顺序是六个固定机位：" + string.Join("、", SubmitCapture.ViewNames) + "。"
            + "委托是「" + orderTitle + "」：" + brief
            + "。请只根据画面给第一印象基础价，范围 20 到 80 的整数，并给出当前心情 mood。mood 必须是 0 到 6 的整数：0 平静，1 开心，2 怀疑，3 难过，4 震惊，5 思考，6 愤怒。只返回 JSON：{\"basePrice\":40,\"comment\":\"一句中文评价\",\"mood\":0}";
        string body = "{"
            + "\"model\":\"" + Esc(Config.model) + "\","
            + "\"messages\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"" + Esc(prompt) + "\"},"
            + images + "]}]}";
        yield return Post(body, raw =>
        {
            string content = ReadContent(raw, out string error);
            if (content == null)
            {
                onDone?.Invoke(null, error);
                return;
            }
            ImpressionResult result = JsonUtility.FromJson<ImpressionResult>(ExtractJson(content));
            if (result == null)
            {
                onDone?.Invoke(null, "第一印象结果无法解析。");
                return;
            }
            result.basePrice = Mathf.Clamp(result.basePrice, 0, 100);
            result.mood = Mathf.Clamp(result.mood, 0, 6);
            onDone?.Invoke(result, null);
        });
    }

    public static IEnumerator RequestTalk(string orderTitle, string brief, int basePrice, int talkScore, List<string> roles, List<string> lines, string playerLine, List<byte[]> pngs, Action<TalkResult, string> onDone)
    {
        string system = "你是委托人，正在和雕塑匠人议价。委托「" + orderTitle + "」：" + brief
            + "。作品的第一印象基础价是 " + basePrice + "，当前谈话分是 " + talkScore + "。"
            + "总价=基础价+谈话分。谈话分是累计值，不是增量，范围 -20 到 30。"
            + "根据玩家这句话调整谈话分：诚恳、讲清作品构思、工时和难度可以加分，胡言乱语、虚假描述或态度差就减分。"
            + "accepted 表示你是否还愿意继续谈话：如果玩家的解释清楚且值得继续沟通，返回 true；如果玩家胡言乱语、态度恶劣或你决定不再继续谈判，返回 false。"
            + "返回 false 时，当前总价就是最终报价，不代表已经成交。用中文说话，不要说自己是人工智能。"
            + "Always return mood as an integer from 0 to 6: 0 default, 1 happy, 2 unsure, 3 sad, 4 shocked, 5 thinking, 6 angry. Return JSON: {\"reply\":\"reply\",\"talkScore\":0,\"accepted\":true,\"mood\":0}";
        StringBuilder messages = new StringBuilder();
        messages.Append("{\"role\":\"system\",\"content\":\"").Append(Esc(system)).Append("\"}");
        for (int i = 0; i < lines.Count; i++)
        {
            messages.Append(",{\"role\":\"").Append(roles[i]).Append("\",\"content\":\"").Append(Esc(lines[i])).Append("\"}");
        }
        messages.Append(",{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"")
            .Append(Esc(playerLine)).Append("\"}");
        for (int i = 0; i < pngs.Count; i++)
        {
            string name = i < SubmitCapture.ViewNames.Length ? SubmitCapture.ViewNames[i] : "视角" + (i + 1);
            messages.Append(",{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,")
                .Append(Convert.ToBase64String(pngs[i]))
                .Append("\"}},{\"type\":\"text\",\"text\":\"")
                .Append(Esc(name)).Append("\"}");
        }
        messages.Append("]}");
        string body = "{\"model\":\"" + Esc(Config.model) + "\",\"messages\":[" + messages + "]}";
        yield return Post(body, raw =>
        {
            string content = ReadContent(raw, out string error);
            if (content == null)
            {
                onDone?.Invoke(null, error);
                return;
            }
            TalkResult result = JsonUtility.FromJson<TalkResult>(ExtractJson(content));
            if (result == null || string.IsNullOrEmpty(result.reply))
            {
                onDone?.Invoke(null, "谈话结果无法解析。");
                return;
            }
            result.talkScore = Mathf.Clamp(result.talkScore, -20, 30);
            result.mood = Mathf.Clamp(result.mood, 0, 6);
            onDone?.Invoke(result, null);
        });
    }

    static IEnumerator Post(string body, Action<string> onDone)
    {
        byte[] payload = Encoding.UTF8.GetBytes(body);
        using UnityWebRequest request = new UnityWebRequest(Config.endpoint, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(payload);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + Config.apiKey);
        request.timeout = 90;
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            string detail = request.downloadHandler != null ? request.downloadHandler.text : "";
            onDone?.Invoke("{\"error\":\"" + Esc(request.error + " " + detail) + "\"}");
            yield break;
        }
        onDone?.Invoke(request.downloadHandler.text);
    }

    static string ReadContent(string raw, out string error)
    {
        error = null;
        if (string.IsNullOrEmpty(raw))
        {
            error = "接口没有返回内容。";
            return null;
        }
        ChatResponse response = JsonUtility.FromJson<ChatResponse>(raw);
        if (response != null && response.choices != null && response.choices.Length > 0 && response.choices[0].message != null)
            return response.choices[0].message.content;
        error = raw.Length > 180 ? raw.Substring(0, 180) : raw;
        return null;
    }

    static string ExtractJson(string content)
    {
        int start = content.IndexOf('{');
        int end = content.LastIndexOf('}');
        if (start < 0 || end <= start)
            return content;
        return content.Substring(start, end - start + 1);
    }

    static string Esc(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
    }

    [Serializable]
    class ChatResponse
    {
        public ChatChoice[] choices;
    }

    [Serializable]
    class ChatChoice
    {
        public ChatMessage message;
    }

    [Serializable]
    class ChatMessage
    {
        public string content;
    }
}
