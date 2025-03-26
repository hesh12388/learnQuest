using UnityEngine;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
    [SerializeField] private Button newGameButton;

    private void Start()
    {
        // Add click listener to the button
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        }
        else
        {
            Debug.LogError("New Game button not assigned in the inspector");
        }
    }

    private void OnNewGameButtonClicked()
    {
        AudioController.Instance.PlayButtonClick();
        // Call the showCourseSelection method on the UIManager instance
        UIManager.Instance.showCourseSelection();
    }

    private void OnDestroy()
    {
        // Remove listener when object is destroyed to prevent memory leaks
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(OnNewGameButtonClicked);
        }
    }
}