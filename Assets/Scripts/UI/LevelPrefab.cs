using System;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelPrefab : MonoBehaviour
{
    public TextMeshProUGUI levelNumber;
    public GameObject starsPanel;
    public GameObject goldStar;
    public GameObject emptyStar;
    

    private int levelIndex;
    
    public void setLevelData(Level level){
        this.levelNumber.text = level.levelNumber.ToString();
        this.levelIndex = level.levelNumber;
        int score = level.score;
        int num_stars;
        if (score >80)
        {
            num_stars = 3;
        }
        else if (score > 50)
        {
            num_stars = 2;
        }
        else if (score > 0)
        {
            num_stars = 1;
        }
        else
        {
            num_stars = 0;
        }
        for (int i = 0; i < num_stars; i++)
        {
            Instantiate(goldStar, starsPanel.transform);
        }
        for (int i = 0; i < 3 - num_stars; i++)
        {
            Instantiate(emptyStar, starsPanel.transform);
        }

    }

    public void startLevel(){
        AudioController.Instance.PlayButtonClick();
        if(levelIndex ==DatabaseManager.Instance.loggedInUser.currentLevel){
            return;
        }
        UIManager.Instance.startLevel(levelIndex);
    }
}
