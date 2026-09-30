using UnityEngine;

public class InstanceBase<T> : MonoBehaviour where T:class
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    static public T instance;
    void Awake()
    {
        if(instance==null)
        {
            instance=this as T;
        }
    }

}
