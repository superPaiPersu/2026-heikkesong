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
        
        DestroyImmediate(gameObject);
        ;
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
