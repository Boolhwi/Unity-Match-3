using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum GameType{
    Moves,
    Time
}

[System.Serializable]
public class EndGameRequirements{
    public GameType gameType;
    public int counterValue;
}

public class EndGameManager : MonoBehaviour
{
    public GameObject movesLabel;
    public GameObject timeLabel;
    public TextMeshProUGUI counter;
    public EndGameRequirements requirements;
    public int currentCounterValue;
    private float timerSeconds;
    private Board board;

    public GameObject tryAgainPanel;
    public GameObject winPanel;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        SetGameType();
        SetUpGame();
    }

    void SetGameType(){
        
        if(board.world != null){
            if(board.world.levels[board.level] != null){   
                requirements = board.world.levels[board.level].endGameRequirements;
            }
        }
    }

    void SetUpGame(){
        currentCounterValue = requirements.counterValue;
        if(requirements.gameType == GameType.Moves){
            movesLabel.SetActive(true);
            timeLabel.SetActive(false);
        } else{
            timerSeconds = 1;
            movesLabel.SetActive(false);
            timeLabel.SetActive(true);
        }
        counter.text = "" + currentCounterValue;
    }

    public void DecreaseCounterValue(){
        if(board.currentState != GameState.pause && board.currentState != GameState.win){
            currentCounterValue--;
            counter.text = "" + currentCounterValue;
            if(currentCounterValue <= 0){
                LoseGame();
            }
        }
    }

    public void WinGame(){
        board.currentState = GameState.win;
        StartCoroutine(EndGame(true));
    }

    public void LoseGame(){
        board.currentState = GameState.lose;
        StartCoroutine(EndGame(false));
    }

    private IEnumerator EndGame(bool state){

        // 마지막으로 채워진 것에 대해서는 매칭 안되는 버그 발생
        // 아마 내려오는 시간으로 인해 isMatched = false인 상태로 아래 조건문을 통과하는 것 같음
        yield return new WaitForSeconds(1.0f);

        if(state){
            winPanel.SetActive(true);
        }
        else {
            tryAgainPanel.SetActive(true);
        }

        currentCounterValue = 0;
        counter.text = "" + currentCounterValue;

        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }

    // Update is called once per frame
    void Update()
    {
        if(requirements.gameType == GameType.Time && currentCounterValue > 0){
            timerSeconds -= Time.deltaTime;
            if(timerSeconds <= 0){
                DecreaseCounterValue();
                timerSeconds = 1;
            }
        }
    }
}
