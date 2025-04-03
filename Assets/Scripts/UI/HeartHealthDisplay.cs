using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartHealthDisplay : MonoBehaviour
{
    [Header("Heart Settings")]
    [SerializeField] private int healthPerHeart = 2; // Each heart represents 2 health points
    
    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    
    [Header("Container")]
    [SerializeField] private Transform heartsContainer; // Parent object for heart images
    
    // Reference to all heart image components
    [SerializeField] private Image[] heartImages; // Drag your existing heart images here
    
    // Store last known health to avoid unnecessary updates
    private int lastHealth = -1;
    
    private void Start()
    {
        // Update hearts initially
        if (Player.Instance != null)
        {
            UpdateHearts(Player.Instance.currentHealth);
        }
    }
    
     private void OnEnable()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnHealthChanged += OnPlayerHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnHealthChanged -= OnPlayerHealthChanged;
        }
    }

    private void Update()
    {
        // Check if health has changed
        if (Player.Instance != null && Player.Instance.currentHealth != lastHealth)
        {
            UpdateHearts(Player.Instance.currentHealth);
            lastHealth = Player.Instance.currentHealth;
        }
    }
    
  
    
    public void UpdateHearts(int currentHealth)
    {
        // Ensure health is not negative
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Calculate how many full hearts and possible half heart
        int fullHeartsCount = currentHealth / healthPerHeart;
        int halfHeart = currentHealth % healthPerHeart;
        
        // Update each heart image
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < fullHeartsCount)
            {
                // Full heart
                heartImages[i].sprite = fullHeartSprite;
            }
            else if (i == fullHeartsCount && halfHeart > 0)
            {
                // Half heart
                heartImages[i].sprite = halfHeartSprite;
            }
            else
            {
                // Empty heart
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }
    
    // Optional: Direct method to subscribe to player health change events
    public void OnPlayerHealthChanged(int newHealth)
    {
        UpdateHearts(newHealth);
        lastHealth = newHealth;
    }
}