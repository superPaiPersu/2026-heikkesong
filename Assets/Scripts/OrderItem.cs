using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OrderItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public OrderInfo info;
    public Image bossPratrait,bossImgUI;
    public TMP_Text orderName,orderDetail;

    public Color highlightColor,normalColor;

    public bool onChooseWhenStart;
    Image img;
    void Start()
    {
        Init();
        if(onChooseWhenStart)
        OnChoose();
    }
    void Init()
    {
        img=GetComponent<Image>();
        ApplyInfo();
        OrderCtrl.instance.AddOrder(this);
    }

    public void SetInfo(OrderInfo value)
    {
        info = value;
        ApplyInfo();
    }

    void ApplyInfo()
    {
        if (img == null)
            img = GetComponent<Image>();
        if (img != null)
            img.color = normalColor;
        if (info == null)
            return;
        if (orderName != null)
            orderName.text = info.orderName;
        if (bossPratrait != null)
            bossPratrait.sprite = info.bossPratrait;
    }
    public void OnChoose()
    {
        if (info == null)
            return;
        orderDetail.text=info.orderDetail;
        var textRect = orderDetail.rectTransform;
        bossImgUI.sprite=info.bossImg;
        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        OrderCtrl.instance.nowOrder=info;
        OrderInfoDisplayer.instance.SetDetail(info.orderDetail);
        OrderCtrl.instance.ClearHighlight();
        ChangeHighlight(true);
    }
    
    public void ChangeHighlight(bool flag)
    {
        if(flag)
        {
            img.color=highlightColor;
        }
        else
        {
            img.color=normalColor;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
