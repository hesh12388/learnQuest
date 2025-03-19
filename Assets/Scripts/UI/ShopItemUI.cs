using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// Define classes for shop items
[System.Serializable]
public class ShopItemUI: MonoBehaviour
{
    public TextMeshProUGUI item_name_text;
    public TextMeshProUGUI item_cost_text;
    public GameObject imagePanel;
    public Button purchaseButton; 
    public ShopItem item;
    public string itemCategory;
    
    private void Awake()
    {
        // If button isn't assigned in inspector, try to find it
        if (purchaseButton == null)
        {
            purchaseButton = GetComponentInChildren<Button>();
        }
        
        // Set up button click handler
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(HandlePurchaseButtonClick);
        }
    }

    // Handle button click
    private void HandlePurchaseButtonClick()
    {
       ShopManager.Instance.buyItem(item.item_name, itemCategory);
    }

    // Enable or disable the purchase button
    public void SetButtonInteractable(bool interactable)
    {
        if (purchaseButton != null)
        {
            purchaseButton.interactable = interactable;
        }
    }

    public void UpdateUI(Sprite image, string item_name, int cost)
    {
        item_name_text.text = item_name;
        item_cost_text.text = cost.ToString();
        imagePanel.GetComponent<Image>().sprite = image;


        if(ShopManager.Instance.isPurchased(item_name)){
            SetButtonInteractable(false);
            item_cost_text.text = "Owned";
        }

        else if(cost>DatabaseManager.Instance.loggedInUser.score)
        {
            SetButtonInteractable(false);
        }
        else
        {
            SetButtonInteractable(true);
        }
    }

    public void refreshItem(){

        if(ShopManager.Instance.isPurchased(item.item_name)){
            SetButtonInteractable(false);
            item_cost_text.text = "Owned";
        }
       
        else if(item.cost>DatabaseManager.Instance.loggedInUser.score)
        {
            SetButtonInteractable(false);
        }
        else
        {
            SetButtonInteractable(true);
        }
    }
}
