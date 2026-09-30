
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public enum Direction
{
    up=0,left=1,right=2,front=3,back=4
}

public class CameraCtrl : InstanceBase<CameraCtrl>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform center,target;
    
    bool isLerping;
    public List<GameObject> flagPointLt;
    void Start()
    {
        
    }
    IEnumerator CarmeraLerp()
    {
        while(true)
        {
            
            transform.position=center.position+Vector3.Slerp(transform.position-center.position,target.position-center.position,8*Time.deltaTime);
            if(Vector3.Distance(transform.position,target.position)<0.1f)
                break;
            transform.LookAt(center);
            yield return null;
        }
        transform.position=target.position;
        transform.rotation=target.rotation;
        isLerping=false;
    }
    public void Transform(GameObject flagPoint)
    {
        
        target=flagPoint.transform;
        if(!isLerping)
        {
            isLerping=true;
            StartCoroutine(CarmeraLerp());
        }

    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            Transform(flagPointLt[(int)Direction.up]);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Transform(flagPointLt[(int)Direction.left]);
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Transform(flagPointLt[(int)Direction.right]);
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            Transform(flagPointLt[(int)Direction.back]);
        }
    }
}
