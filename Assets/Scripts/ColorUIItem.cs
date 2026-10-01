using UnityEngine;
using UnityEngine.UI;

public class ColorUIItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Image img;
    
    void Start()
    {
        img=GetComponent<Image>();
    }
    public void OnClick()
    {
        CubeColorCtrl.instance.SetColor(img.color);
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
