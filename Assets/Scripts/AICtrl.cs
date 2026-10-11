using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AICtrl : InstanceBase<AICtrl>
{
    // 与 Assets/Resources/UI/委托人 中的文件后缀保持一致，文件夹是表情的唯一来源。
    static readonly string[] MoodNames =
    {
        "default", "happy", "unsure", "sad", "shocked", "thinking", "angry"
    };

    public GameObject cameraParent;
    [SerializeField] Image moodImage;
    [SerializeField] Sprite[] moodSprites;
    Texture2D[] imgs;
   
    public void Start()
    {
        imgs=Resources.LoadAll<Texture2D>("/photos");
    }
    
    // public IEnumerator AskGPT(
    //     string apiKey,
    //     string prompt,
    //     string[] imageUrls,
    //     Action<string> onResult)
    // {
    //     StringBuilder content = new StringBuilder();

    //     content.Append("{\"type\":\"input_text\",\"text\":\"");
    //     content.Append(prompt.Replace("\"", "\\\""));
    //     content.Append("\"}");

    //     foreach (string url in imageUrls)
    //     {
    //         content.Append(",{\"type\":\"input_image\",\"image_url\":\"");
    //         content.Append(url);
    //         content.Append("\"}");
    //     }

    //     string json =
    //         "{\"model\":\"gpt-6.1-sol\",\"input\":[{\"role\":\"user\",\"content\":[" +
    //         content +
    //         "]}]}";

    //     using UnityWebRequest request =
    //         new UnityWebRequest("https://api.openai.com/v1/responses", "POST");

    //     request.uploadHandler =
    //         new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

    //     request.downloadHandler = new DownloadHandlerBuffer();

    //     request.SetRequestHeader("Content-Type", "application/json");
    //     request.SetRequestHeader("Authorization", "Bearer " + apiKey);

    //     yield return request.SendWebRequest();

    //     if (request.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogError(request.error);
    //         yield break;
    //     }

    //     Response response =
    //         JsonUtility.FromJson<Response>(request.downloadHandler.text);

    //     onResult?.Invoke(response.output[0].content[0].text);
    // }

    // [Serializable]
    // private class Response
    // {
    //     public Output[] output;
    // }

    // [Serializable]
    // private class Output
    // {
    //     public Content[] content;
    // }

    // [Serializable]
    // private class Content
    // {
    //     public string text;
    // }
    public void TakePhoto()
    {
        int cnt=0;
        foreach(var i in imgs)
        {
            string path =
            Path.Combine(Application.persistentDataPath, "object.png",cnt.ToString());

            File.WriteAllBytes(path, i.EncodeToPNG());
            Debug.Log("图片已保存：" + path);
        }
        
    }
    public string GetAIPrompt()
    {
        return OrderCtrl.instance.nowOrder.AIPrompt;
    }

    public void SetMood(int mood)
    {
        if (moodImage == null)
            return;

        int index = Mathf.Clamp(mood, 0, MoodNames.Length - 1);
        string prefix = "";
        if (OrderCtrl.instance != null && OrderCtrl.instance.nowOrder != null &&
            OrderCtrl.instance.nowOrder.bossImg != null)
        {
            string spriteName = OrderCtrl.instance.nowOrder.bossImg.name;
            int separator = spriteName.IndexOf('_');
            prefix = separator > 0 ? spriteName.Substring(0, separator) : spriteName;
        }

        if (!string.IsNullOrEmpty(prefix))
        {
            Sprite folderSprite = Resources.Load<Sprite>("UI/\u59d4\u6258\u4eba/" + prefix + "_" + MoodNames[index]);
            if (folderSprite != null)
            {
                moodImage.sprite = folderSprite;
                return;
            }
        }

        // 保留 Inspector 旧引用作为资源缺失时的后备方案。
        if (moodSprites != null && index < moodSprites.Length && moodSprites[index] != null)
            moodImage.sprite = moodSprites[index];
    }

    public void BeginBargain()
    {
        BargainFlow.Ensure().SubmitWork();
    }
}
