using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderCtrl : InstanceBase<OrderCtrl>
{
    [SerializeField] OrderInfo[] orderTemplates;
    [SerializeField] int visibleOrderCount = 10;

    readonly List<OrderItem> orders = new List<OrderItem>();
    readonly Queue<OrderInfo> orderPool = new Queue<OrderInfo>();
    [Serializable]
    class PresetOrder
    {
        public string orderName;
        public string orderDetail;
        public int template;
    }

    [Serializable]
    class PresetLibrary
    {
        public PresetOrder[] orders;
    }
    public OrderInfo nowOrder;
    public GameObject configPanel;

    void Start()
    {
        StartCoroutine(InitializeOrders());
    }

    IEnumerator InitializeOrders()
    {
        yield return null;

        // The order cards live under the ScrollView Content, not under this controller.
        OrderItem[] sceneItems = FindObjectsByType<OrderItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sceneItems.Length; i++)
            AddOrder(sceneItems[i]);

        BuildOrderPool(sceneItems);
        if (orderPool.Count == 0 || orders.Count == 0)
            yield break;

        OrderItem template = orders[0];
        Transform itemParent = template.transform.parent;
        while (orders.Count < Mathf.Max(1, visibleOrderCount))
        {
            OrderItem item = Instantiate(template, itemParent);
            item.name = "OrderItem_" + orders.Count;
            item.onChooseWhenStart = false;
            AddOrder(item);
        }

        for (int i = 0; i < orders.Count; i++)
            orders[i].SetInfo(TakeNextOrder());

        ResetOrder();
    }

    void BuildOrderPool(OrderItem[] sceneItems)
    {
        orderPool.Clear();
        List<OrderInfo> sources = new List<OrderInfo>();
        if (orderTemplates != null)
        {
            for (int i = 0; i < orderTemplates.Length; i++)
            {
                if (orderTemplates[i] != null && !sources.Contains(orderTemplates[i]))
                    sources.Add(orderTemplates[i]);
            }
        }
        if (sources.Count == 0)
        {
            for (int i = 0; i < sceneItems.Length; i++)
            {
                if (sceneItems[i].info != null && !sources.Contains(sceneItems[i].info))
                    sources.Add(sceneItems[i].info);
            }
        }
        if (sources.Count == 0)
            return;

        TextAsset libraryAsset = Resources.Load<TextAsset>("order_library");
        PresetLibrary library = libraryAsset != null
            ? JsonUtility.FromJson<PresetLibrary>(libraryAsset.text)
            : null;

        if (library != null && library.orders != null && library.orders.Length > 0)
        {
            for (int i = 0; i < library.orders.Length; i++)
            {
                PresetOrder preset = library.orders[i];
                OrderInfo source = sources[Mathf.Abs(preset.template) % sources.Count];
                OrderInfo generated = Instantiate(source);
                generated.name = "order_preset_" + (i + 1).ToString("D3");
                generated.orderName = preset.orderName;
                generated.orderDetail = preset.orderDetail;
                orderPool.Enqueue(generated);
            }
        }
        else
        {
            Debug.LogError("预制订单库 order_library 缺失或为空。", this);
            return;
        }

        List<OrderInfo> shuffled = new List<OrderInfo>(orderPool);
        orderPool.Clear();
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int swap = UnityEngine.Random.Range(0, i + 1);
            OrderInfo temp = shuffled[i];
            shuffled[i] = shuffled[swap];
            shuffled[swap] = temp;
        }
        for (int i = 0; i < shuffled.Count; i++)
            orderPool.Enqueue(shuffled[i]);
    }

    OrderInfo TakeNextOrder()
    {
        return orderPool.Count > 0 ? orderPool.Dequeue() : null;
    }

    public void AddOrder(OrderItem item)
    {
        if (item != null && !orders.Contains(item))
            orders.Add(item);
    }

    public void ClearHighlight()
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i] != null)
                orders[i].ChangeHighlight(false);
        }
    }

    public void ResetOrder()
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i] != null && orders[i].info != null)
            {
                orders[i].OnChoose();
                return;
            }
        }
        nowOrder = null;
    }

    public void RemoveCurrentOrder()
    {
        if (nowOrder == null)
            return;

        for (int i = orders.Count - 1; i >= 0; i--)
        {
            OrderItem item = orders[i];
            if (item == null)
            {
                orders.RemoveAt(i);
                continue;
            }
            if (item.info == nowOrder)
            {
                OrderInfo replacement = TakeNextOrder();
                item.SetInfo(replacement);
                item.ChangeHighlight(false);
                if (replacement == null)
                    item.gameObject.SetActive(false);
                break;
            }
        }

        nowOrder = null;
        if (OrderInfoDisplayer.instance != null)
            OrderInfoDisplayer.instance.SetDetail("");
    }
}
