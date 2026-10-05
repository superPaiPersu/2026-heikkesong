using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrderCtrl : InstanceBase<OrderCtrl>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    List<OrderItem> orders=new List<OrderItem>();
    public OrderInfo nowOrder;
    void Start()
    {
        
    }
    public void AddOrder(OrderItem item)
    {
        orders.Add(item);
    }
    public void ClearHighlight()
    {
        foreach(var i in orders)
        {
            i.ChangeHighlight(false);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
