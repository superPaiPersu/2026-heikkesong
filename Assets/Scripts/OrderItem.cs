using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public OrderInfo info;
    public Image bossPratrait;
    public TMP_Text orderName,orderDetail;

    public Color highlightColor,normalColor;
    Image img;
    void Start()
    {
        Init();
    }
    void Init()
    {
        img=GetComponent<Image>();
        img.color=normalColor;
        orderName.text=info.orderName;
        bossPratrait.sprite=info.bossPratrait;
        OrderCtrl.instance.AddOrder(this);
    }
    public void OnChoose()
    {
        orderDetail.text=info.orderDetail;
        var textRect = orderDetail.rectTransform;
       
        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        
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
