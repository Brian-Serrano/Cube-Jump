using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Scriptable Objects/ShopItem")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public int cost;
    public Sprite itemSprite;
    public Currency currency;
    public string id;
    public int index;
}
