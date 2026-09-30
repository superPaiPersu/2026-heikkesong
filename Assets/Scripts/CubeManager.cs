using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeManager : InstanceBase<CubeManager>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject smallCubePrefab,cubesParent;
    public Vector3 centerPos,cubeCnt;
    List<CubeItem> allCubes=new List<CubeItem>();
    void Start()
    {
        Application.targetFrameRate=60;
    }
    public void InitCubes()//y是竖轴
    {
        BoxCollider collider=smallCubePrefab.GetComponent<BoxCollider>();
        float lengthX=collider.size.x;
        float lengthY=collider.size.y;
        float lengthZ=collider.size.z;
        float nowX=0,nowY=0,nowZ=0;
        cubesParent.transform.position=centerPos;
        for(int i=1;i<=cubeCnt.x;i++)
        {
            nowY=0;
            for(int u=1;u<=cubeCnt.y;u++)
            {
                nowZ=0;
                for(int v=1;v<=cubeCnt.z;v++)
                {
                    CubeItem item=Instantiate(smallCubePrefab,cubesParent.transform).GetComponent<CubeItem>();
                    //小块的轴心位于中心，假设第一个块位于世界原点
                    item.transform.position=new Vector3(
                        centerPos.x-lengthX*cubeCnt.x/2+lengthX*0.5f,
                    centerPos.y-lengthY*cubeCnt.y/2+lengthY*0.5f,
                    centerPos.z-lengthZ*cubeCnt.z/2+lengthZ*0.5f)
                    + new Vector3(nowX,nowY,nowZ);
                    
                    allCubes.Add(item);
                    nowZ+=lengthZ;
                }
                nowY+=lengthY;
            }
            nowX+=lengthX;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            InitCubes();
        }
    }
}
