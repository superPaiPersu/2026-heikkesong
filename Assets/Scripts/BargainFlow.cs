using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BargainFlow : MonoBehaviour
{
    [SerializeField] TMP_Text bossText;
    [SerializeField] TMP_Text dealText;
    [SerializeField] TMP_Text orderInfoText;
    [SerializeField] TMP_InputField input;
    
    [SerializeField] RollingNum priceTensNum;
    [SerializeField] RollingNum priceOnesNum;
    bool busy;
    bool talkClosed;
    int basePrice;
    int talkScore;
    string comment = "";
    const string RefusalReply = "我已经给出最终报价，不再继续谈了。";
    readonly List<string> roles = new List<string>();
    readonly List<string> lines = new List<string>();

    public static BargainFlow Ensure()
    {
        BargainFlow existing = FindAnyObjectByType<BargainFlow>(FindObjectsInactive.Include);
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
        UpdateOrderInfo();
        StartCoroutine(Bind());
    }

    IEnumerator Bind()
    {
        yield return null;
        yield return null;
        // 第一次 AI 初印象返回前，始终显示当前委托人的默认表情。
        AICtrl.instance?.SetMood(0);
    }

    public void SubmitWork()
    {
        if (busy)
            return;
        // 开始新的初印象请求时清除上一次议价留下的心情。
        AICtrl.instance?.SetMood(0);
        UpdateOrderInfo();
        if (BargainApi.Config == null)
            BargainApi.Load();
        CubeManager cubes = CubeManager.instance;
        if (cubes == null || cubes.cubesParent == null || cubes.cubesParent.transform.childCount == 0)
        {
            SetBoss("还没有雕出东西，先完成作品再提交。");
            return;
        }
        if (!BargainApi.Ready)
        {
            SetBoss("还没有填写接口密钥。请在 StreamingAssets/bargain_api.json 里写入 apiKey。");
            return;
        }
        StartCoroutine(Upload());
    }

    IEnumerator Upload()
    {
        busy = true;
        talkClosed = false;
        talkScore = 0;
        roles.Clear();
        lines.Clear();
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
        SetBoss("让我仔细看看......");
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
        AICtrl.instance?.SetMood(result.mood);
        SetBoss(comment);
        RefreshDeal();
    }

    public void Send()
    {
        if (busy || input == null)
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
        if (talkClosed)
        {
            SetBoss(RefusalReply);
            UpdatePrice();
            SetDeal("最终报价  基础价 " + basePrice + " + 谈话分 " + talkScore + " = 总价 " + Total());
            return;
        }
        StartCoroutine(Talk(line));
    }

    public void CompleteDeal()
    {
        if (busy)
            return;

        if (OrderCtrl.instance != null)
        {
            OrderCtrl.instance.RemoveCurrentOrder();
            OrderCtrl.instance.ResetOrder();
        }
            
        if (CubeManager.instance != null)
            CubeManager.instance.ClearCubes();
        
    }

    IEnumerator Talk(string line)
    {
        busy = true;
        ReadOrder(out string title, out string brief);
        TalkResult result = null;
        string error = null;
        SetBoss("让我仔细看看......");
        List<byte[]> pngs = null;
        yield return SubmitCapture.Ensure().Capture(value => pngs = value);
        if (pngs == null || pngs.Count == 0)
        {
            busy = false;
            SetBoss("没有找到结算相机。");
            yield break;
        }
        yield return BargainApi.RequestTalk(title, brief, basePrice, talkScore, roles, lines, line, pngs, (value, err) =>
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
        AICtrl.instance?.SetMood(result.mood);
        if (!result.accepted)
        {
            talkClosed = true;
            SetBoss(RefusalReply);
            UpdatePrice();
            SetDeal("最终报价  基础价 " + basePrice + " + 谈话分 " + talkScore + " = 总价 " + Total());
            yield break;
        }
        SetBoss(result.reply);
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
        if (AICtrl.instance != null)
        {
            string aiPrompt = AICtrl.instance.GetAIPrompt();
            if (!string.IsNullOrEmpty(aiPrompt))
                brief += " " + aiPrompt;
        }
    }

    int Total()
    {
        return Mathf.Max(0, basePrice + talkScore);
    }

    void RefreshDeal()
    {
        UpdatePrice();
        SetDeal("基础价 " + basePrice + " + 谈话分 " + talkScore + " = 总价 " + Total());
    }

    void UpdatePrice()
    {
        int price = Mathf.Max(0, Total());
        priceTensNum?.SetNum((price / 10) % 10);
        priceOnesNum?.SetNum(price % 10);
    }

    void SetBoss(string value)
    {
        if (bossText == null)
            return;
        bossText.text = value;
        LayoutRebuilder.ForceRebuildLayoutImmediate(bossText.rectTransform);
    }

    void SetDeal(string value)
    {
        if (dealText != null)
            dealText.text = value;
    }

    void UpdateOrderInfo()
    {
        if (orderInfoText == null)
            return;
        if (OrderCtrl.instance == null || OrderCtrl.instance.nowOrder == null)
        {
            orderInfoText.text = "";
            return;
        }
        orderInfoText.text = OrderCtrl.instance.nowOrder.orderDetail ?? "";
    }

}
