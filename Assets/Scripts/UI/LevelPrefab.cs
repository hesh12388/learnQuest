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
        for (int i = 0; i < score; i++)
        {
            Instantiate(goldStar, starsPanel.transform);
        }
        for (int i = 0; i < 3 - score; i++)
        {
            Instantiate(emptyStar, starsPanel.transform);
        }

    }

    public void startLevel(){
        if(levelIndex ==DatabaseManager.Instance.loggedInUser.currentLevel){
            return;
        }
        UIManager.Instance.startLevel(levelIndex);
    }
}
