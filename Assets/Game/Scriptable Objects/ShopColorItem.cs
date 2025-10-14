using UnityEngine;

[CreateAssetMenu(fileName = "ShopColorItem", menuName = "Scriptable Objects/ShopColorItem")]
public class ShopColorItem : ScriptableObject
{
    public string itemName;
    public int cost;
    public Color color;
    public Currency currency;
    public string id;
    public int index;
}
