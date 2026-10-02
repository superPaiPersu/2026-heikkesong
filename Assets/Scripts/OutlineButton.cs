using UnityEngine;

public class OutlineButton : InstanceBase<OutlineButton>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isActive;
    public GameObject activeFlag;//对勾
    
    public void Switch()
    {
        isActive=!isActive;
        CubeManager.instance.OutlineVisablity(isActive);
    }
    // Update is called once per frame
    public void Update()
    {
        activeFlag.SetActive(isActive);
        
    }
}
