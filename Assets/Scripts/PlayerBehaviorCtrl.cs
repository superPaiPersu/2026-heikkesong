using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum GameMode
{
    carve,paint,add
}
public class PlayerBehaviorCtrl : InstanceBase<PlayerBehaviorCtrl>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameMode nowMode;
    bool inCarve;
    List<CubeItem> deleteIntentCubeLt= new List<CubeItem>();
    void Start()
    {
        
    }
    void GetInput()
    {
        if(Input.GetMouseButtonDown(0) ||Input.GetMouseButton(0) && (inCarve||nowMode==GameMode.paint ))
        { 
            Ray r=CameraCtrl.instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(r,out hit,100,1<<LayerMask.NameToLayer("cube")))
            {
                
                CubeItem item=hit.transform.GetComponent<CubeItem>();
                
                if(nowMode==GameMode.carve && !item.isChosen)
                {
                    inCarve=true;
                    item.isChosen=true;
                    deleteIntentCubeLt.Add(item);
                }
                if(nowMode==GameMode.add)
                {
                    CubeManager.instance.AddCube(hit.normal,item);
                }
                if(nowMode==GameMode.paint)
                {
                    
                    item.ChangeMaterial();
                }
            }
            
        }
        
        if(Input.GetMouseButtonUp(0))
        {
            if(inCarve)
            {
                inCarve=false;
                foreach(var i in deleteIntentCubeLt)
                {
                    
                    i.Delete();
                }
                deleteIntentCubeLt.Clear();
            }
            
        }
        if(Input.GetMouseButtonDown(1))
        {
            if(inCarve)
            {
                inCarve=false;
                foreach(var i in deleteIntentCubeLt)
                {
                   
                    i.isChosen=false;
                }
                deleteIntentCubeLt.Clear();
            }
            
        }
    }
    // Update is called once per frame
    void Update()
    {
        GetInput();
    }
}
