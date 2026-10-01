using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeManager : InstanceBase<CubeManager>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject smallCubePrefab,cubesParent;
    public Vector3 centerPos,cubeCnt;
    List<CubeItem> allCubes=new List<CubeItem>();
    float lengthX,lengthY,lengthZ;
    void Start()
    {
        Application.targetFrameRate=60;
        BoxCollider collider=smallCubePrefab.GetComponent<BoxCollider>();
        lengthX=collider.size.x;
        lengthY=collider.size.y;
        lengthZ=collider.size.z;
    }
    public void InitCubes()//y是竖轴
    {
        
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
    public void AddCube(Vector3 normal,CubeItem item)
    {
        CubeItem newItem=Instantiate(smallCubePrefab,cubesParent.transform).GetComponent<CubeItem>();
        allCubes.Add(newItem);
        if(Vector3.Cross(item.transform.forward,normal).sqrMagnitude<1e-6f)
        {
            newItem.transform.position=item.transform.position+normal.normalized*lengthZ;
        }
        if(Vector3.Cross(item.transform.up,normal).sqrMagnitude<1e-6f)
        {
            newItem.transform.position=item.transform.position+normal.normalized*lengthY;
        }
        if(Vector3.Cross(item.transform.right,normal).sqrMagnitude<1e-6f)
        {
            newItem.transform.position=item.transform.position+normal.normalized*lengthX;
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
