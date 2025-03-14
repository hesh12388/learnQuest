using UnityEngine;
using TMPro;  // Import TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject landingPage;
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Fields")]
    public TMP_InputField loginEmailField;
    public TMP_InputField loginPasswordField;

    [Header("Register Fields")]
    public TMP_InputField registerEmailField;
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;

    public void ShowLogin()
    {
        landingPage.SetActive(false);
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void ShowRegister()
    {
        landingPage.SetActive(false);
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void BackToLanding()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        landingPage.SetActive(true);
    }

    public void loginUser()
    {
        string email = loginEmailField.text;
        string password = loginPasswordField.text;

            DatabaseManager.Instance.Login(email, password, (bool success) =>
            {
                if (success)
                {
                    Debug.Log("Login successful! 🎉");
                }
                else
                {
                    Debug.LogError("Login failed! ❌");
                }
            });
    }

    public void registerUser()
    {
        string email = registerEmailField.text;
        string username = registerUsernameField.text;
        string password = registerPasswordField.text;

        DatabaseManager.Instance.Register(email, username, password, (bool success) =>
        {
            if (success)
            {
                Debug.Log("Registration successful! 🎉");
            }
            else
            {
                Debug.LogError("Registration failed! ❌");
            }
        });
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
