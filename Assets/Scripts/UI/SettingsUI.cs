using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.Utilities;

public class SettingsUI : MonoBehaviour
{
    [Header("Keybinding Buttons")]
    public TextMeshProUGUI upKeyText;
    public TextMeshProUGUI downKeyText;
    public TextMeshProUGUI leftKeyText;
    public TextMeshProUGUI rightKeyText;
    public TextMeshProUGUI interactKeyText;

    [Header("Shortcut Keybinding Buttons")]
    public TextMeshProUGUI openShopModifierText;
    public TextMeshProUGUI openShopKeyText;

    public TextMeshProUGUI openLevelsModifierText;
    public TextMeshProUGUI openLevelsKeyText;

    public TextMeshProUGUI openCharactersModifierText;
    public TextMeshProUGUI openCharactersKeyText;

    public TextMeshProUGUI openSettingsModifierText;
    public TextMeshProUGUI openSettingsKeyText;

    public TextMeshProUGUI openObjectivesModifierText;
    public TextMeshProUGUI openObjectivesKeyText;

    public TextMeshProUGUI openAchievementsModifierText;
    public TextMeshProUGUI openAchievementsKeyText;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private PlayerInputActions playerInputActions;

    private void Start()
    {
        // Get direct reference to the same PlayerInputActions instance used by Player
        playerInputActions = Player.Instance.playerControls;

        // Initialize the UI with the current keybindings
        UpdateKeybindingUI();
    }

    private void UpdateKeybindingUI()
    {
        var moveBindings = playerInputActions.Player.Move.bindings;

        Debug.Log(moveBindings);
        // Update movement keybinds
        upKeyText.text = GetBindingDisplayName(moveBindings, "Up");
        downKeyText.text = GetBindingDisplayName(moveBindings, "Down");
        leftKeyText.text = GetBindingDisplayName(moveBindings, "Left");
        rightKeyText.text = GetBindingDisplayName(moveBindings, "Right");

        // Update interaction keybind
        interactKeyText.text = playerInputActions.Player.Interact.bindings[0].ToDisplayString();

        // Update shortcut keybinds
        UpdateShortcutKeybindingUI(playerInputActions.UIShortCuts.OpenShop, openShopModifierText, openShopKeyText);
        UpdateShortcutKeybindingUI(playerInputActions.UIShortCuts.OpenLevels, openLevelsModifierText, openLevelsKeyText);
        UpdateShortcutKeybindingUI(playerInputActions.UIShortCuts.OpenCharacters, openCharactersModifierText, openCharactersKeyText);
        UpdateShortcutKeybindingUI(playerInputActions.UIShortCuts.OpenSettings, openSettingsModifierText, openSettingsKeyText);
        UpdateShortcutKeybindingUI(playerInputActions.UIShortCuts.OpenObjectives, openObjectivesModifierText, openObjectivesKeyText);
        UpdateShortcutKeybindingUI(playerInputActions.UIShortCuts.OpenAchievements, openAchievementsModifierText, openAchievementsKeyText);
    }

    private string GetBindingDisplayName(ReadOnlyArray<InputBinding> bindings, string name)
    {
        foreach (var binding in bindings)
        {
            if (binding.name?.ToLower() == name.ToLower())
            {
                return binding.ToDisplayString();
            }
        }
        return "Not Bound";
    }

    public void RebindKey(InputAction action, int bindingIndex, TextMeshProUGUI keyText)
    {
        // Stop any ongoing rebinding
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
        }

        // Disable relevant actions during rebinding
        Player.Instance.DisableInputTemporarily();
        
        keyText.text = "Press a key...";

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(operation =>
            {
                operation.Dispose();
                rebindingOperation = null;

                // Save the binding overrides to PlayerPrefs
                string overridesJson = playerInputActions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString("InputBindings", overridesJson);
                PlayerPrefs.Save();

                // Re-enable the player controls
                Player.Instance.EnableInput();
                
                // Update the UI to show the new binding
                UpdateKeybindingUI();
            })
            .WithCancelingThrough("<Keyboard>/escape")
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnCancel(operation =>
            {
                operation.Dispose();
                rebindingOperation = null;
                
                // Re-enable after cancel
                Player.Instance.EnableInput();
                
                // Reset the text
                UpdateKeybindingUI();
            })
            .Start();
    }

    private void UpdateShortcutKeybindingUI(InputAction action, TextMeshProUGUI modifierText, TextMeshProUGUI keyText)
    {
        // Your shortcuts are using composite bindings (OneModifier)
        // Need to extract the modifier and binding parts from the composite binding
        
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            
            if (binding.isPartOfComposite)
            {
                if (binding.name == "modifier")
                {
                    modifierText.text = binding.ToDisplayString();
                }
                else if (binding.name == "binding")
                {
                    keyText.text = binding.ToDisplayString();
                }
            }
        }
        
        // Set default values if not found
        if (string.IsNullOrEmpty(modifierText.text))
            modifierText.text = "Not Bound";
        
        if (string.IsNullOrEmpty(keyText.text))
            keyText.text = "Not Bound";
    }

    // Modify the RebindShortcutKey method
    public void RebindShortcutKey(InputAction action, int partIndex, TextMeshProUGUI keyText)
    {
        // Stop any ongoing rebinding
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
        }

        // Disable relevant actions during rebinding - now from Player
        Player.Instance.DisableInputTemporarily();

        keyText.text = "Press a key...";

        // Find the actual binding index based on the part we want to rebind
        string partName = partIndex == 0 ? "modifier" : "binding";
        int actualBindingIndex = -1;
        
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].isPartOfComposite && action.bindings[i].name == partName)
            {
                actualBindingIndex = i;
                break;
            }
        }
        
        if (actualBindingIndex == -1)
        {
            Debug.LogError($"Could not find binding part '{partName}' for action '{action.name}'");
            Player.Instance.EnableInput();
            keyText.text = "Error";
            return;
        }

        rebindingOperation = action.PerformInteractiveRebinding(actualBindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                operation.Dispose();
                rebindingOperation = null;

                // IMPORTANT: Disable and re-enable the action to make sure the composite binding works
                action.Disable();
                
                // Save the binding overrides to PlayerPrefs
                string overridesJson = playerInputActions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString("InputBindings", overridesJson);
                PlayerPrefs.Save();
                
                // Re-enable the action to apply the changes
                action.Enable();

                // Re-enable all input (via Player now)
                Player.Instance.EnableInput();

                // Update the UI to show the new binding
                UpdateKeybindingUI();
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                rebindingOperation = null;

                // Re-enable after cancel (via Player now)
                Player.Instance.EnableInput();

                // Reset the text
                UpdateKeybindingUI();
            })
            .Start();
    }

    private int GetBindingIndex(InputAction action, string bindingName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].name?.ToLower() == bindingName.ToLower())
                return i;
        }
        return -1;
    }

    public void RebindDirectionKey(string direction, TextMeshProUGUI keyText)
    {
        var action = playerInputActions.Player.Move;
        int index = GetBindingIndex(action, direction);
        if (index >= 0)
        {
            RebindKey(action, index, keyText);
        }
        else
        {
            Debug.LogError($"Binding '{direction}' not found in 'Move' action!");
        }
    }

    // Rebind movement keys
    public void RebindUpKey() => RebindDirectionKey("Up", upKeyText);
    public void RebindDownKey() => RebindDirectionKey("Down", downKeyText);
    public void RebindLeftKey() => RebindDirectionKey("Left", leftKeyText);
    public void RebindRightKey() => RebindDirectionKey("Right", rightKeyText);

    // Rebind interaction key
    public void RebindInteractKey()
    {
        RebindKey(playerInputActions.Player.Interact, 0, interactKeyText);
    }

    // Rebind shortcut keys (modifier and main key)
    public void RebindOpenShopModifier()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenShop, 0, openShopModifierText);
    }

    public void RebindOpenShopKey()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenShop, 1, openShopKeyText);
    }

    public void RebindOpenLevelsModifier()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenLevels, 0, openLevelsModifierText);
    }

    public void RebindOpenLevelsKey()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenLevels, 1, openLevelsKeyText);
    }

    public void RebindOpenCharactersModifier()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenCharacters, 0, openCharactersModifierText);
    }

    public void RebindOpenCharactersKey()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenCharacters, 1, openCharactersKeyText);
    }

    public void RebindOpenSettingsModifier()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenSettings, 0, openSettingsModifierText);
    }

    public void RebindOpenSettingsKey()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenSettings, 1, openSettingsKeyText);
    }

    public void RebindOpenObjectivesModifier()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenObjectives, 0, openObjectivesModifierText);
    }

    public void RebindOpenObjectivesKey()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenObjectives, 1, openObjectivesKeyText);
    }

    public void RebindOpenAchievementsModifier()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenAchievements, 0, openAchievementsModifierText);
    }

    public void RebindOpenAchievementsKey()
    {
        RebindShortcutKey(playerInputActions.UIShortCuts.OpenAchievements, 1, openAchievementsKeyText);
    }

    // Reset all bindings to defaults
    public void ResetToDefaults()
    {
        playerInputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey("InputBindings");
        UpdateKeybindingUI();
    }
}