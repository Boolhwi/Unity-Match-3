using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject levelPanel;
    private GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        GameData gameData = FindObjectOfType<GameData>();

        if(gameData.gameStateData.init == true){
            Home();
        } else {
            PlayGame();
        }
    }

    public void PlayGame(){
        startPanel.SetActive(false);
        levelPanel.SetActive(true);
    }

    public void Home(){
        startPanel.SetActive(true);
        levelPanel.SetActive(false);
    }
}
