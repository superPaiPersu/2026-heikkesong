using UnityEngine;
using UnityEngine.UI;

public class CubeColorCtrl : InstanceBase<CubeColorCtrl>
{
   
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public Image bigColorDisplay;
    public Color nowChosenColor;
    void Start()
    {
        
    }
    public void SetColor(Color color)
    {
        nowChosenColor=color;
        bigColorDisplay.color=color;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
