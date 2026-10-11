
using UnityEngine;

public class PanelItem : MonoBehaviour
{
    Vector3 originPos;

    void Awake()
    {
        originPos = transform.localPosition;
    }

    void Start()
    {
        PanelCtrl.instance.RegisterPanel(this);
    }
    public void Close()
    {
        transform.localPosition=new Vector3(5000,0,0);
    }
    public void Open()
    {
        
        Transition.instance.Active(()=>{PanelCtrl.instance.CloseAllPanel();transform.localPosition=originPos;});
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
