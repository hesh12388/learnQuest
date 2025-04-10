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
    public Image item_cost_image;
    public Sprite gem_sprite;
    public Sprite coin_sprite;
    
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

        
        if(itemCategory=="Boost"){
            item_cost_image.sprite = gem_sprite;
        }
        else{
            item_cost_image.sprite = coin_sprite;
        }


        if(ShopManager.Instance.isEquipped(item_name, itemCategory)){
            SetButtonInteractable(false);
            item_cost_text.text = "Equipped";
        }

        else if(ShopManager.Instance.isPurchased(item_name)){

            if(itemCategory=="character" || itemCategory=="Move"){
                SetButtonInteractable(true);
                item_cost_text.text = "Equip";
            }
            else{
                SetButtonInteractable(false);
                item_cost_text.text = "Owned";
            }
        }

        else if(itemCategory!="Boost" && item.cost>DatabaseManager.Instance.loggedInUser.score)
        {
            SetButtonInteractable(false);
        }
        else if(itemCategory=="Boost" && item.cost>DatabaseManager.Instance.loggedInUser.numGems)
        {
            SetButtonInteractable(false);
        }
    }

    public void refreshItem(){

        if(ShopManager.Instance.isPurchased(item.item_name)){
            
            if(itemCategory=="character" || itemCategory=="Move"){
                
                if(ShopManager.Instance.isEquipped(item.item_name, itemCategory)){
                    SetButtonInteractable(false);
                    item_cost_text.text = "Equipped";
                }
                else{
                    SetButtonInteractable(true);
                    item_cost_text.text = "Equip";
                }
            }
            else{
                SetButtonInteractable(false);
                item_cost_text.text = "Owned";
            }
        }
       
        else if(itemCategory!="Boost" && item.cost>DatabaseManager.Instance.loggedInUser.score)
        {
            SetButtonInteractable(false);
        }
        else if(itemCategory=="Boost" && item.cost>DatabaseManager.Instance.loggedInUser.numGems)
        {
            SetButtonInteractable(false);
        }
    }
}
