using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ValueConfigUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TMP_InputField input;
    public int num,maxx,minn;

    void Start()
    {
        input=GetComponent<TMP_InputField>();
        input.text="3";
        num=3;
    }
    public void AddNum()
    {
        num+=1;
        CheckValueRange();
    }
    public void OnValueChange()
    {
        if(int.TryParse(input.text,out num))
        {
            CheckValueRange();
        }
        
        
    }
    public void DeclineNum()
    {
        num-=1;
        CheckValueRange();
    }
    void CheckValueRange()
    {
        num=math.min(maxx,num);
        num=math.max(minn,num);
        input.text=num.ToString();
    }
    
}
