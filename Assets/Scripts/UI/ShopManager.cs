using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemImage
    {
        public string itemId;
        public Sprite itemImage;
    }
    
    // References to UI components
    public Transform shopContentPanel;
    public GameObject shopItemPrefab;
    public TextMeshProUGUI user_coins_text;
    
    // Item image mappings (assigned in inspector)
    public List<ItemImage> itemImages = new List<ItemImage>();
    
    // Dictionary for faster lookups of item images by ID
    private Dictionary<string, Sprite> itemImageDictionary = new Dictionary<string, Sprite>();

    public static ShopManager Instance { get; private set; } // Singleton instance

    private string current_category;

    public List<UserItem> items_purchased;
    
    private void Awake()
    {

         if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep this object between scenes
            }
            else
            {
                Destroy(gameObject);
            }

        // Convert the list to a dictionary for faster lookups
        foreach (ItemImage item in itemImages)
        {
            if (!string.IsNullOrEmpty(item.itemId) && item.itemImage != null)
            {
                itemImageDictionary[item.itemId] = item.itemImage;
            }
        }
    }

    private void setCoinsText()
    {
        user_coins_text.text = DatabaseManager.Instance.loggedInUser.score.ToString();
    }
    

    
    // Method to populate character items
    public void PopulateCharacterItems(ShopItemsResponse shopItemsResponse)
    {
        current_category = "character";
        setCoinsText();
        // Clear existing items first
        foreach (Transform child in shopContentPanel) {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in shopItemsResponse.categories["character"])
        {
            Debug.Log("Item name: " + item.item_name);
            GameObject shopItemObject = Instantiate(shopItemPrefab, shopContentPanel.transform);
            shopItemObject.GetComponent<ShopItemUI>().UpdateUI(itemImageDictionary[item.item_name], item.item_name, item.cost);
            shopItemObject.GetComponent<ShopItemUI>().item = item;
            shopItemObject.GetComponent<ShopItemUI>().itemCategory = "character";
        }
    }

    public void refreshShopItems()
    {
        foreach (Transform child in shopContentPanel)
        {
            
            child.GetComponent<ShopItemUI>().refreshItem();
        }
    }

    public void getBoughtItems(ShopItemsResponse shop){
        DatabaseManager.Instance.GetUserItems((List<UserItem> purchasedItems) =>
        {
            items_purchased = purchasedItems;
            PopulateInstructorItems(shop);
        });
    }

    public bool isPurchased(string item_name)
    {
        foreach(UserItem item in items_purchased)
        {
            if(item.item_name == item_name)
            {
                return true;
            }
        }
        return false;

    }
    
    // Method to populate instructor items
    public void PopulateInstructorItems(ShopItemsResponse shopItemsResponse)
    {
        current_category = "instructor";
        setCoinsText();
         // Clear existing items first
        foreach (Transform child in shopContentPanel) {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in shopItemsResponse.categories["instructor"])
        {
            GameObject shopItemObject = Instantiate(shopItemPrefab, shopContentPanel.transform);
            shopItemObject.GetComponent<ShopItemUI>().UpdateUI(itemImageDictionary[item.item_name], item.item_name, item.cost);
            shopItemObject.GetComponent<ShopItemUI>().item = item;
            shopItemObject.GetComponent<ShopItemUI>().itemCategory = "instructor";
        }
    }
    
    public void buyItem(string item_name, string item_type)
    {
        DatabaseManager.Instance.BuyItem(item_name, item_type, (bool success) =>
        {
            if (success)
            {
                Debug.Log("Item bought successfully");
                setCoinsText();
                items_purchased.Add(new UserItem(item_name, item_type));
                refreshShopItems();
            }
            else
            {
                Debug.Log("Item purchase failed");
            }
        });
    }
    // Method to populate boost items
    public void PopulateBoostItems(ShopItemsResponse shopItemsResponse)
    {
        current_category = "Boost";
        setCoinsText();
         // Clear existing items first
        foreach (Transform child in shopContentPanel) {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in shopItemsResponse.categories["Boost"])
        {
            GameObject shopItemObject = Instantiate(shopItemPrefab, shopContentPanel.transform);
            shopItemObject.GetComponent<ShopItemUI>().UpdateUI(itemImageDictionary[item.item_name], item.item_name, item.cost);
            shopItemObject.GetComponent<ShopItemUI>().item = item;
            shopItemObject.GetComponent<ShopItemUI>().itemCategory = "Boost";
        }
    }
    
  
     

    
}

