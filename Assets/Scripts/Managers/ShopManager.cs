using System;
using System.Collections.Generic;
using UnityEngine;

// Simple data class to pass item data to UIManager
public class ShopItemData
{
    public ShopItem item;
    public string category;
    public Sprite sprite;
    public string itemDescription;
    public string item_use;
    
    public ShopItemData(ShopItem item, string category, Sprite sprite, string itemDescription = null, string item_use = null)
    {
        this.item = item;
        this.category = category;
        this.sprite = sprite;
        this.itemDescription = itemDescription;
        this.item_use = item_use;
    }
}
   
public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemImage
    {
        public string itemId;
        public Sprite itemImage;
        public string itemDescription;
        public string item_use;
    }
    
    // Item image mappings (assigned in inspector)
    public List<ItemImage> itemImages = new List<ItemImage>();
    
    // Dictionary for faster lookups of item images by ID
    private Dictionary<string, Sprite> itemImageDictionary = new Dictionary<string, Sprite>();

    public static ShopManager Instance { get; private set; } // Singleton instance

    public List<UserItem> items_purchased;
    private ShopItemsResponse shopItemsResponse;


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
                DatabaseManager.Instance.loggedInUser.equippedCharacter = "Jessica";
                // Set up player moves based on purchased items
                SetupPlayerMoves(purchasedItems);
            }
        });
    }


    /// <summary>
    /// Set up player moves based on purchased items
    /// </summary>
    private void SetupPlayerMoves(List<UserItem> purchasedItems)
    {
        // Create a new array for player moves (default to 4 moves)
        string[] playerMoves = new string[4];
        
        // Find all purchased move items
        List<UserItem> purchasedMoves = purchasedItems.FindAll(item => item.item_type == "Move");
        
        // If player has purchased moves, use them starting from index 0
        for (int i = 0; i < purchasedMoves.Count && i < 4; i++)
        {
            playerMoves[i] = purchasedMoves[i].item_name;
        }
        
        // Set the player moves
        DatabaseManager.Instance.loggedInUser.playerMoves = playerMoves;
        
        Debug.Log("Player moves initialized: " + string.Join(", ", playerMoves));
    }

    public ShopItemData GetItemInfo(string itemName)
    {
        
        // Look for the item in our item images list to get descriptions
        ItemImage itemImageData = itemImages.Find(i => i.itemId == itemName);
        
        if (itemImageData == null)
        {
            Debug.LogWarning($"No item data found for: {itemName}");
            return null;
        }
         
        foreach (KeyValuePair<string, List<ShopItem>> entry in shopItemsResponse.categories)
        {
            // Check each category's items
            foreach (ShopItem item in entry.Value)
            {
                // If we find the item, store it and break out of the loop
                if (item.item_name.Trim() == itemName.Trim())
                {
                    Debug.Log(item.cost);
                    return new ShopItemData(
                        item,
                        entry.Key,
                        itemImageData.itemImage,
                        itemImageData.itemDescription,
                        itemImageData.item_use
                    );
            
                }
            }
            
        }
        
        return null; // Item not found
    }

    // Get items for a specific category to display in the UI
    public List<ShopItemData> GetItemsForCategory(string category)
    {
        List<ShopItemData> result = new List<ShopItemData>();
        
        if (shopItemsResponse == null || shopItemsResponse.categories == null)
        {
            Debug.LogWarning("Shop items not loaded yet");
            return result;
        }
        
        if (!shopItemsResponse.categories.ContainsKey(category))
        {
            Debug.LogWarning($"Category {category} not found in shop items");
            return result;
        }
        
        foreach (ShopItem item in shopItemsResponse.categories[category])
        {
            // Get the sprite for this item
            Sprite itemSprite = null;
            if (itemImageDictionary.ContainsKey(item.item_name))
            {
                itemSprite = itemImageDictionary[item.item_name];
            }
            else
            {
                Debug.LogWarning($"No image found for item: {item.item_name}");
            }
            
            result.Add(new ShopItemData(item, category, itemSprite));
        }
        
        return result;
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

    // Check if an item is already purchased
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
    
    // Equip a move
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
        UIManager.Instance.RefreshShopItems();
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
        UIManager.Instance.RefreshShopItems();
    }
    
    // Buy an item
    public void buyItem(string item_name, string item_type)
    {

        // Play purchase sound
        AudioController.Instance.PlayBuyItem();

        // If already purchased, just equip it
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
    
        // Process the purchase through the database
        DatabaseManager.Instance.BuyItem(item_name, item_type, (bool success) =>
        {
            if (success)
            {
                Debug.Log("Item bought successfully");
                
                // Add to purchased items
                items_purchased.Add(new UserItem(item_name, item_type));
                
                // Update UI
                UIManager.Instance.UpdateShopCurrencyDisplay();
                UIManager.Instance.RefreshShopItems();
                UIManager.Instance.updatePlayerCoins();
                UIManager.Instance.updatePlayerGems();
            }
            else
            {
                Debug.Log("Item purchase failed");
            }
        });
    }
}