using System.Collections.Generic;
using UnityEngine;

public class CubeItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isChosen;
    MeshRenderer mr;
    public Color chosenColor,normalColor;
    void Start()
    {
        mr=GetComponent<MeshRenderer>();
    }
    public void Delete()
    {
        CubeManager.instance.DeleteCube(this);
        DestroyImmediate(gameObject);
        
    }
    public void ChangeMaterial()
    {
        
        List<Material> mtlt= new List<Material>(PaintCtrl.instance.nowChoose.materialLt)
        {
            PaintCtrl.instance.outlineMt
        };
        mr.SetMaterials(mtlt);
        CubeManager.instance.OutlineVisablity(OutlineButton.instance.isActive);
    }
    public void ChangeColor()
    {
        normalColor=CubeColorCtrl.instance.nowChosenColor;

    }
    public void Create()
    {
        ;
    }
    // Update is called once per frame
    void Update()
    {
        if(isChosen)
        {
            mr.material.color=chosenColor;
        }
        else
        {
            mr.material.color=normalColor;
        }
    }
}
