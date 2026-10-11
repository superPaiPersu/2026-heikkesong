using TMPro;
using UnityEngine;

public class OrderInfoDisplayer : InstanceBase<OrderInfoDisplayer>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text orderdetail;
    public void SetDetail(string text)
    {
        orderdetail.text=text;
    }
}
