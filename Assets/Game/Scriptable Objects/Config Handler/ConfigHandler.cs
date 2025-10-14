using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConfigHandler", menuName = "Scriptable Objects/Config Handler/ConfigHandler")]
public class ConfigHandler : ScriptableObject
{
    public List<GameObject> objectPrefab;

    public List<ShopItem> cubeShopItems;
    public List<ShopItem> shipShopItems;
    public List<ShopItem> ballShopItems;
    public List<ShopItem> ufoShopItems;
    public List<ShopItem> waveShopItems;

    public List<ShopColorItem> colorShopItems;
}
