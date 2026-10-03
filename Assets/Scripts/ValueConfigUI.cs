using System;
using UnityEngine;
using UnityEngine.UI;

public class ValueConfigUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    InputField input;
    public int num;
    void Start()
    {
        input=GetComponent<InputField>();
    }
    public void AddNum()
    {
        num+=1;
        input.text=num.ToString();
    }
    public void OnValueChange()
    {
        num=int.Parse(input.text);
    }
    public void DeclineNum()
    {
        num-=1;
        input.text=num.ToString();
    }
    
}
