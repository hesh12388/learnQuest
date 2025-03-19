using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// Define classes for shop items
[System.Serializable]
public class ShopItem: MonoBehaviour
{
    public string item_name;
    public int cost;

    public ShopItem(string item_name, int cost)
    {
        this.item_name = item_name;
        this.cost = cost;
    }
}

[System.Serializable]
public class ShopItemsResponse
{
    public Dictionary<string, List<ShopItem>> categories;
    
    public ShopItemsResponse()
    {
        categories = new Dictionary<string, List<ShopItem>>();
    }
}