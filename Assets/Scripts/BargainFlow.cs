using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BargainFlow : MonoBehaviour
{
    PanelItem argue;
    TMP_Text bossText;
    TMP_Text playerText;
    TMP_Text dealText;
    TMP_InputField input;
    TMP_FontAsset font;
    bool busy;
    bool dealt;
    int basePrice;
    int talkScore;
    string comment = "";
    readonly List<string> roles = new List<string>();
    readonly List<string> lines = new List<string>();

    public static BargainFlow Ensure()
    {
        BargainFlow existing = FindAnyObjectByType<BargainFlow>();
        if (existing != null)
            return existing;
        return new GameObject("BargainFlow").AddComponent<BargainFlow>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        Ensure();
    }

    void Start()
    {
        BargainApi.Load();
        SubmitCapture.Ensure();
        StartCoroutine(Bind());
    }

    IEnumerator Bind()
    {
        yield return null;
        yield return null;
        PlaceEntryOnGear();
        Transform panel = Find("arguepanel");
        if (panel == null)
            yield break;
        argue = panel.GetComponent<PanelItem>();
        CacheFont();
        bossText = FindText(panel, "bosstext");
        playerText = FindText(panel, "playertext");
        dealText = FindText(panel, "deal");
        BuildTalkBar(panel);
        Transform work = Find("workpanel");
        if (work != null && work.Find("提交作品") == null)
            BuildButton(work, "提交作品", "提交作品", new Vector2(592f, -210f), SubmitWork);
        argue?.Close();
    }

    public void SubmitWork()
    {
        if (busy)
            return;
        CubeManager cubes = CubeManager.instance;
        if (cubes == null || cubes.cubesParent == null || cubes.cubesParent.transform.childCount == 0)
        {
            OpenArgue();
            SetBoss("还没有雕出东西，先完成作品再提交。");
            return;
        }
        if (!BargainApi.Ready)
        {
            OpenArgue();
            SetBoss("还没有填写接口密钥。请在 StreamingAssets/bargain_api.json 里写入 apiKey。");
            return;
        }
        StartCoroutine(Upload());
    }

    IEnumerator Upload()
    {
        busy = true;
        dealt = false;
        talkScore = 0;
        roles.Clear();
        lines.Clear();
        OpenArgue();
        SetBoss("正在看六个机位的照片……");
        SetDeal("定价中");
        List<byte[]> pngs = null;
        yield return SubmitCapture.Ensure().Capture(value => pngs = value);
        if (pngs == null || pngs.Count == 0)
        {
            busy = false;
            SetBoss("没有找到结算相机。");
            yield break;
        }
        ReadOrder(out string title, out string brief);
        ImpressionResult result = null;
        string error = null;
        yield return BargainApi.RequestImpression(title, brief, pngs, (value, err) =>
        {
            result = value;
            error = err;
        });
        busy = false;
        if (result == null)
        {
            SetBoss(string.IsNullOrEmpty(error) ? "第一印象没有评出来。" : error);
            yield break;
        }
        basePrice = result.basePrice;
        comment = result.comment;
        talkScore = 0;
        SetBoss(comment);
        SetPlayer("");
        RefreshDeal();
    }

    public void Send()
    {
        if (busy || dealt || input == null)
            return;
        string line = input.text.Trim();
        if (line.Length == 0)
            return;
        if (basePrice <= 0 && string.IsNullOrEmpty(comment))
        {
            SetBoss("先提交作品，等委托人看过照片。");
            return;
        }
        input.text = "";
        SetPlayer(line);
        StartCoroutine(Talk(line));
    }

    IEnumerator Talk(string line)
    {
        busy = true;
        ReadOrder(out string title, out string brief);
        TalkResult result = null;
        string error = null;
        yield return BargainApi.RequestTalk(title, brief, basePrice, talkScore, roles, lines, line, (value, err) =>
        {
            result = value;
            error = err;
        });
        busy = false;
        if (result == null)
        {
            SetBoss(string.IsNullOrEmpty(error) ? "这句话没有送到。" : error);
            yield break;
        }
        roles.Add("user");
        lines.Add(line);
        roles.Add("assistant");
        lines.Add(result.reply);
        talkScore = result.talkScore;
        dealt = result.accepted;
        SetBoss(result.reply);
        RefreshDeal();
        if (dealt)
            SetDeal("成交  基础价 " + basePrice + " + 谈话分 " + talkScore + " = 总价 " + Total());
    }

    public void AcceptOffer()
    {
        if (busy || string.IsNullOrEmpty(comment))
            return;
        dealt = true;
        SetDeal("成交  基础价 " + basePrice + " + 谈话分 " + talkScore + " = 总价 " + Total());
        SetBoss("就按这个价，成交。");
    }

    public void ShiftOrder()
    {
        if (busy || string.IsNullOrEmpty(comment))
            return;
        dealt = false;
        talkScore = 0;
        roles.Clear();
        lines.Clear();
        SetPlayer("");
        SetBoss(comment);
        RefreshDeal();
    }

    void ReadOrder(out string title, out string brief)
    {
        title = "这件雕塑";
        brief = "按画面和谈话议价";
        if (OrderCtrl.instance == null || OrderCtrl.instance.nowOrder == null)
            return;
        OrderInfo order = OrderCtrl.instance.nowOrder;
        if (!string.IsNullOrEmpty(order.orderName))
            title = order.orderName;
        if (!string.IsNullOrEmpty(order.orderDetail))
            brief = order.orderDetail;
        if (!string.IsNullOrEmpty(order.AIPrompt))
            brief += " " + order.AIPrompt;
    }

    int Total()
    {
        return Mathf.Max(0, basePrice + talkScore);
    }

    void RefreshDeal()
    {
        SetDeal("基础价 " + basePrice + " + 谈话分 " + talkScore + " = 总价 " + Total());
    }

    void PlaceEntryOnGear()
    {
        GameObject entry = GameObject.Find("entry");
        if (entry == null)
            return;
        Transform task = Find("taskpanel");
        Transform background = task != null ? FindDeep(task, "bk") : null;
        RectTransform rect = entry.GetComponent<RectTransform>();
        if (background != null)
            rect.SetParent(background, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(857f, 320f);
        rect.sizeDelta = new Vector2(176f, 128f);
        rect.SetAsLastSibling();
        Image image = entry.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = 0f;
            image.color = color;
            image.raycastTarget = true;
        }
    }

    void OpenArgue()
    {
        if (PanelCtrl.instance != null)
            PanelCtrl.instance.CloseAllPanel();
        argue?.Open();
    }

    void SetBoss(string value)
    {
        if (bossText != null)
            bossText.text = value;
    }

    void SetPlayer(string value)
    {
        if (playerText != null)
            playerText.text = value;
    }

    void SetDeal(string value)
    {
        if (dealText != null)
            dealText.text = value;
    }

    void BuildTalkBar(Transform panel)
    {
        if (panel.Find("BargainInput") != null)
            return;
        Sprite board = Resources.Load<Sprite>("UI/议价UI/button_empty");
        input = CreateInput(panel, "BargainInput", new Vector2(-40f, -430f), new Vector2(760f, 64f), board);
        BuildButton(panel, "BargainSend", "发送", new Vector2(470f, -430f), Send);
        BuildButton(panel, "BargainAccept", "成交", new Vector2(250f, -500f), AcceptOffer);
        BuildButton(panel, "BargainShift", "重谈", new Vector2(40f, -500f), ShiftOrder);
        BuildButton(panel, "BargainBack", "返回", new Vector2(-180f, -500f), () => argue?.Close());
    }

    void BuildButton(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        Sprite sprite = Resources.Load<Sprite>("UI/议价UI/button_empty");
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(160f, 56f);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        go.GetComponent<Button>().onClick.AddListener(action);
        GameObject textGo = new GameObject("label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TMP_Text text = textGo.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = 28;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.2f, 0.12f, 0.08f);
        text.text = label;
    }

    TMP_InputField CreateInput(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = new Color(1f, 0.97f, 0.9f);
        GameObject textGo = new GameObject("text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        Stretch(textGo.GetComponent<RectTransform>());
        TMP_Text text = textGo.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = 26;
        text.color = new Color(0.15f, 0.1f, 0.08f);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        TMP_InputField field = go.GetComponent<TMP_InputField>();
        field.textComponent = text;
        field.characterLimit = 80;
        field.lineType = TMP_InputField.LineType.SingleLine;
        return field;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(16f, 6f);
        rect.offsetMax = new Vector2(-16f, -6f);
    }

    void CacheFont()
    {
        TMP_Text any = FindAnyObjectByType<TMP_Text>();
        if (any != null)
            font = any.font;
    }

    static TMP_Text FindText(Transform root, string name)
    {
        Transform child = FindDeep(root, name);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    static Transform Find(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.transform : null;
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null)
                return found;
        }
        return null;
    }
}
