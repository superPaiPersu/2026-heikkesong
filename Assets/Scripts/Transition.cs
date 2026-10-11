using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Transition : InstanceBase<Transition>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Image  img;
    public float speed,dir=1;
    bool inTran;
    
    public void Start()
    {
        img=GetComponent<Image>();
    }
    public void Active(Action callback)
    {
        
        print("enter");
        if(!inTran)
        {
            inTran=true;
            StartCoroutine(TransitionGo(callback));
        }
        
    }
    IEnumerator TransitionGo(Action callback)
    {
        while(true)
        {
            print(img.fillAmount);
            img.fillAmount+=speed*Time.deltaTime*dir;
            if(dir==1 && img.fillAmount>=1)
            {
                img.fillClockwise=true;
                dir=-1;
                callback();
            }
            if(dir==-1 && img.fillAmount<=0)
            {
                dir=1;
                img.fillClockwise=false;
                
                break;
            }
            yield return null;
        }
        inTran=false;
    }
    // Update is called once per frame
    void Update()
    {
        

    }
}
