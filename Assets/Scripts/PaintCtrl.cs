using System.Collections.Generic;
using UnityEngine;

public class PaintCtrl : InstanceBase<PaintCtrl>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<PaintInfo> paintLt=new List<PaintInfo>();
    public PaintInfo nowChoose;

    
    public GameObject itemPrefab,itemParent;

    public Material outlineMt; 
    List<PaintItem> paintItemLt=new List<PaintItem>();
    void Start()
    {
        foreach(var i in paintLt)
        {
            PaintItem item=Instantiate(itemPrefab,itemParent.transform).GetComponent<PaintItem>();
            item.Set(i);
            paintItemLt.Add(item);
        }
    }
    public void Reset()
    {
        foreach(var i in paintItemLt)
        {
            i.Reset();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
