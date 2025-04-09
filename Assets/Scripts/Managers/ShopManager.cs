using System;
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
    public TextMeshProUGUI user_gems_text;
    
    // Item image mappings (assigned in inspector)
    public List<ItemImage> itemImages = new List<ItemImage>();
    
    // Dictionary for faster lookups of item images by ID
    private Dictionary<string, Sprite> itemImageDictionary = new Dictionary<string, Sprite>();

    public static ShopManager Instance { get; private set; } // Singleton instance

    public List<UserItem> items_purchased;

    private ShopItemsResponse shopItemsResponse;
    private string shop_category="Move";
    
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

    /// <summary>
    /// Load shop items from the database
    /// </summary>
    public void LoadShopItems()
    {
        DatabaseManager.Instance.GetShopItems((shopItems) =>
        {
            if (shopItems != null)
            {
                shopItemsResponse = shopItems;
                Debug.Log("Shop items loaded successfully.");
            }
            else
            {
                Debug.LogWarning("Failed to load shop items.");
            }
        });

        // Fetch purchased items from the database
        DatabaseManager.Instance.GetUserItems((List<UserItem> purchasedItems) =>
        {
            items_purchased = purchasedItems;
            
            //store in the User object for future use
            if (DatabaseManager.Instance.loggedInUser != null)
            {
                DatabaseManager.Instance.loggedInUser.purchasedItems = purchasedItems;
            }
        });
    }


    public void ShowShop(){
        if(shop_category=="Move"){
            PopulateMoveItems();
        }
        else if(shop_category=="character"){
            PopulateCharacterItems();
        }
        else{
            PopulateMoveItems();
        }
    }
    private void setCoinsText()
    {
        user_coins_text.text = DatabaseManager.Instance.loggedInUser.score.ToString();
        user_gems_text.text = DatabaseManager.Instance.loggedInUser.numGems.ToString();
    }
    

    // Check if an item is equipped
    public bool isEquipped(string item_name, string item_type)
    {
        // If user isn't logged in or doesn't have data initialized
        if (DatabaseManager.Instance.loggedInUser == null)
            return false;
        
        switch (item_type)
        {
            case "Move":
                // Check if move is in playerMoves array
                if (DatabaseManager.Instance.loggedInUser.playerMoves != null)
                {
                    return System.Array.IndexOf(DatabaseManager.Instance.loggedInUser.playerMoves, item_name) >= 0;
                }
                break;
                
            case "character":
                // Check if this is the equipped character
                if (DatabaseManager.Instance.loggedInUser.equippedCharacter != null)
                {
                    return DatabaseManager.Instance.loggedInUser.equippedCharacter == item_name;
                }
                break;
        }
        
        return false;
    }
    
    // Method to populate character items
    public void PopulateCharacterItems()
    {
        shop_category="character";
        setCoinsText();
        // Clear existing items first
        foreach (Transform child in shopContentPanel) {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in shopItemsResponse.categories["character"])
        {
            Debug.Log("Item name: " + item.item_name);
            GameObject shopItemObject = Instantiate(shopItemPrefab, shopContentPanel.transform);
            shopItemObject.GetComponent<ShopItemUI>().itemCategory = "character";
            shopItemObject.GetComponent<ShopItemUI>().item = item;
            shopItemObject.GetComponent<ShopItemUI>().UpdateUI(itemImageDictionary[item.item_name], item.item_name, item.cost);
            
            
        }
    }

    public void refreshShopItems()
    {
        foreach (Transform child in shopContentPanel)
        {
            
            child.GetComponent<ShopItemUI>().refreshItem();
        }
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
    
    public void equipMove(string item_name)
    {
     
        // Create a new array with the new move at the front
        string[] newMoves = new string[4];
        newMoves[0] = item_name;
        
        // Copy the existing moves (except the last one)
        for (int i = 0; i < 3; i++)
        {
            newMoves[i + 1] = DatabaseManager.Instance.loggedInUser.playerMoves[i];
        }
        
        // Update the playerMoves array
        DatabaseManager.Instance.loggedInUser.playerMoves = newMoves;
        
        // Update the UI to reflect the change
        refreshShopItems();
        
    
    }

    // Equip a character
    public void equipCharacter(string item_name)
    {
        if (DatabaseManager.Instance.loggedInUser == null)
            return;
        
        // Set the equipped character
        DatabaseManager.Instance.loggedInUser.equippedCharacter = item_name;

        PlayerManager.Instance.SetActivePlayerAppearance(item_name);
        
        // Update the UI to reflect the change
        refreshShopItems();
        
    }

    // Method to populate instructor items
    public void PopulateMoveItems()
    {
        shop_category="Move";
        setCoinsText();
         // Clear existing items first
        foreach (Transform child in shopContentPanel) {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in shopItemsResponse.categories["Move"])
        {
            GameObject shopItemObject = Instantiate(shopItemPrefab, shopContentPanel.transform);
            shopItemObject.GetComponent<ShopItemUI>().itemCategory = "Move";
            shopItemObject.GetComponent<ShopItemUI>().item = item;
            shopItemObject.GetComponent<ShopItemUI>().UpdateUI(itemImageDictionary[item.item_name], item.item_name, item.cost);
        }
    }
    
    public void buyItem(string item_name, string item_type)
    {
        if(isPurchased(item_name))
        {
            if(item_type == "Move")
            {
                equipMove(item_name);
            }
            else if(item_type == "character")
            {
                equipCharacter(item_name);
            }

            return;
        }
        AudioController.Instance.PlayBuyItem();
        DatabaseManager.Instance.BuyItem(item_name, item_type, (bool success) =>
        {
            if (success)
            {
                Debug.Log("Item bought successfully");
                setCoinsText();
                items_purchased.Add(new UserItem(item_name, item_type));
                refreshShopItems();
                UIManager.Instance.updatePlayerCoins();
                UIManager.Instance.updatePlayerGems();
            }
            else
            {
                Debug.Log("Item purchase failed");
            }
        });
    }
    // Method to populate boost items
    public void PopulateBoostItems()
    {
        shop_category="Boost";
        setCoinsText();
         // Clear existing items first
        foreach (Transform child in shopContentPanel) {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in shopItemsResponse.categories["Boost"])
        {
            GameObject shopItemObject = Instantiate(shopItemPrefab, shopContentPanel.transform);
            shopItemObject.GetComponent<ShopItemUI>().itemCategory = "Boost";
            shopItemObject.GetComponent<ShopItemUI>().item = item;
            shopItemObject.GetComponent<ShopItemUI>().UpdateUI(itemImageDictionary[item.item_name], item.item_name, item.cost);
        }
    }
    
  
     

    
}

