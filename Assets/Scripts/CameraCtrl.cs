
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
    public float moveSpeed,scaleSpeed,radius,maxAngle;
    bool isLerping;
    public List<GameObject> flagPointLt;
    void Start()
    {
        ;
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
    public void InitCameraRadius(float distance)
    {
        radius=distance;
        transform.position=(transform.position-center.position).normalized*radius+center.position;
    }
    void GetMouseInput()
    {
        if(Input.GetMouseButton(2))
        {
            float angle=Vector3.Angle(transform.position-center.position,Vector3.up),x=Input.GetAxisRaw("Mouse X"),y=Input.GetAxisRaw("Mouse Y");


            Vector3 temp=transform.position-center.position,temp1=temp,axis=Vector3.Cross(temp,Vector3.up).normalized;
            
            temp.y=0;
            temp1.x=0;
            temp1.z=0;

            transform.position=Quaternion.Euler(0,x*moveSpeed*Time.deltaTime,0)*temp+temp1+center.position;
            if(!(90-angle>maxAngle && y<0 || 90-angle<-maxAngle && y>0))
                transform.position=Quaternion.AngleAxis(-y*moveSpeed*Time.deltaTime,axis)*(transform.position-center.position)+center.position;

            transform.LookAt(center);
        }
        // print(Input.GetAxis("Mouse ScrollWheel"));
        float wheelNum=Input.GetAxis("Mouse ScrollWheel");
        if(wheelNum!=0)
        {
            transform.position+=-(transform.position-center.position).normalized*wheelNum*scaleSpeed*Time.deltaTime;
            radius=(transform.position-center.position).magnitude;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        foreach(GameObject flagPoint in flagPointLt)
        {
            flagPoint.transform.position=(flagPoint.transform.position-center.position).normalized*radius+center.position;
        }
        
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
        GetMouseInput();
    }
}
