using UnityEngine;
[CreateAssetMenu(fileName = "OrderConfig", menuName = "ScriptableObjects/OrderConfig", order = 1)]
public class OrderInfo : ScriptableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string orderName;
    public string orderDetail;
    public string AIPrompt;

    public Sprite bossPratrait;
}
