using System.Collections.Generic;
using UnityEngine;

public class PanelCtrl : InstanceBase<PanelCtrl>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    List <PanelItem> allPanels= new List<PanelItem>();
    void Start()
    {
        
    }
    public void RegisterPanel(PanelItem item)
    {
        allPanels.Add(item);
    }
    public void CloseAllPanel()
    {
        foreach(var i in allPanels)
        {
            i.Close();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
