using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    [Header ("Level Information")]
    public string levelToLoad;
    public int level;
    private GameData gameData;
    private int starsActive;
    private int highScore;

    [Header ("UI Stuff")]
    public Image[] stars;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI starText;

    
    public GameObject confirmPanelBg;

    // Start is called before the first frame update
    void OnEnable()
    {
        gameData = FindObjectOfType<GameData>();
        LoadData();
        ActivateStars();
        SetText();
    }

    void LoadData(){
        if(gameData != null){
            starsActive = gameData.saveData.stars[level -1];
            highScore = gameData.saveData.highScores[level -1];
        }
    }

    void SetText(){
        highScoreText.text = ""+highScore;
        starText.text = ""+starsActive+"/3";
    }

    
    void ActivateStars(){
        for(int i = 0; i< stars.Length; i++){
            stars[i].enabled = false;
        }
        for(int i = 0; i< starsActive; i++){
            stars[i].enabled = true;
        }
    }

    public void Cancel(){
        confirmPanelBg.SetActive(false);
    }

    public void Play(){
        PlayerPrefs.SetInt("Current Level",level -1);
        SceneManager.LoadScene(levelToLoad);
    }
}
