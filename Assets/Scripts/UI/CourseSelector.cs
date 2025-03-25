using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CourseSelector : MonoBehaviour
{
    public Sprite[] courseImages; // Array of course images
    public string[] courseNames; // Array of course names

    public Image courseImageUI; // Reference to the UI Image component
    public TextMeshProUGUI courseNameText; // Reference to the Course Name text

    public Button leftButton, rightButton, selectButton; // UI Buttons

    private int currentIndex = 0; // Track the selected course

    private string allowedCourse="WebDev";

    void Start()
    {
        UpdateCourseDisplay();

        // Add button listeners
        leftButton.onClick.AddListener(PreviousCourse);
        rightButton.onClick.AddListener(NextCourse);
    }

    void UpdateCourseDisplay()
    {
        // Update UI with new course image & name
        courseImageUI.sprite = courseImages[currentIndex];
        courseNameText.text = courseNames[currentIndex];

        if(courseNames[currentIndex] == allowedCourse){
            selectButton.interactable = true;
            selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "SELECT";
        }else{
            selectButton.interactable = false;
            selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Soon!";
        }
    }

    public void NextCourse()
    {
        currentIndex++;
        if (currentIndex >= courseImages.Length)
            currentIndex = 0; // Loop back to first course

        UpdateCourseDisplay();
    }

    public void PreviousCourse()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = courseImages.Length - 1; // Loop back to last course

        UpdateCourseDisplay();
    }
}
