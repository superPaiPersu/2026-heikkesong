
using UnityEngine;

public class PanelItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Vector3 originPos;
    void Start()
    {
        originPos=transform.position;
        PanelCtrl.instance.RegisterPanel(this);
    }
    public void Close()
    {
        transform.position=new Vector3(5000,0,0);
    }
    public void Open()
    {
        transform.position=originPos;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
