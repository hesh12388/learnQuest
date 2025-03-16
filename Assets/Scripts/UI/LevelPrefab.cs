using System;
using System.Globalization;
using UnityEngine;
using TMPro;

public class LevelPrefab : MonoBehaviour
{
    public TextMeshProUGUI levelNumber;
    public GameObject starsPanel;
    public GameObject goldStar;
    public GameObject emptyStar;

    
    public void setLevelData(Level level){
        this.levelNumber.text = level.levelNumber.ToString();
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
}
