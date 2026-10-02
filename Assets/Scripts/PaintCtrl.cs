using System.Collections.Generic;
using UnityEngine;

public class PaintCtrl : InstanceBase<PaintCtrl>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<PaintInfo> paintLt=new List<PaintInfo>();
    public PaintInfo nowChoose;

    public Dictionary<string,Color> colorDt= new Dictionary<string, Color>();
    public GameObject itemPrefab,itemParent;

    public Material outlineMt; 
    List<PaintItem> paintItemLt=new List<PaintItem>();
    void Start()
    {
        Init();
    }
    public void Init()
    {
        foreach(var i in paintLt)
        {
            colorDt[i.materialLt[0].name]=i.materialLt[0].color;
            PaintItem item=Instantiate(itemPrefab,itemParent.transform).GetComponent<PaintItem>();
            item.Set(i);
            paintItemLt.Add(item);
        }
        
        paintItemLt[0].OnClick();
    }
    public Color? GetOriginColor(string name)
    {
        foreach(var i in colorDt.Keys)
        {
            if(name.Contains(i))
            {
                return colorDt[i];
                
            }
        }
        return null;
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
