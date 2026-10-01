
using UnityEngine;
using UnityEngine.UI;

public class PaintItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    PaintInfo mine;
    public Image myImg,hightlightImg;
    void Start()
    {
        Reset();
    }
    public void Set(PaintInfo info)
    {
        mine=info;
        myImg.sprite=info.image;
    }
    public void OnClick()
    {
        PaintCtrl.instance.Reset();
        PaintCtrl.instance.nowChoose=mine;
        hightlightImg.enabled=true;
    }
    public void Reset()
    {
        hightlightImg.enabled=false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
