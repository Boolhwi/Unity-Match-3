using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlankGoal
{
    public int numberNeeded;
    public int numberCollected;
    public Sprite goalSpirte;
    public string matchValue;
}

public class GoalManager : MonoBehaviour
{
    public BlankGoal[] levelGoals;
    public List<GoalPanel> currentGoals = new List<GoalPanel>(); 
    public GameObject goalIntroPrefab;
    public GameObject goalGamePrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;
    private EndGameManager endGameManager;
    private Board board;


    // Start is called before the first frame update
    void Start()
    {
        endGameManager = FindObjectOfType<EndGameManager>();
        board = FindObjectOfType<Board>();
        GetGoals();
        SetupGoals();
    }

    void GetGoals(){
        
        if(board != null){
            if(board.world != null){
                if(board.world.levels[board.level] != null){   
                    levelGoals = board.world.levels[board.level].levelGoals;
                }
            }
        }
    }

    void SetupGoals()
    {
        for(int i =0; i< levelGoals.Length; i++){

            GameObject goal = Instantiate(goalIntroPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform, false);

            GoalPanel panel = goal.GetComponent<GoalPanel>();
            panel.thisSprite = levelGoals[i].goalSpirte;
            panel.thisString = "0/"+ levelGoals[i].numberNeeded;

            GameObject gameGoal = Instantiate(goalGamePrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform, false);

            panel = gameGoal.GetComponent<GoalPanel>();
            currentGoals.Add(panel);
            panel.thisSprite = levelGoals[i].goalSpirte;
            panel.thisString = "0/"+ levelGoals[i].numberNeeded;
        }
    }

    public void UpdateGoals()
    {
        int goalsCompleted = 0;
        for(int i = 0; i< levelGoals.Length; i++){
            currentGoals[i].thisText.text = levelGoals[i].numberCollected + "/" + levelGoals[i].numberNeeded;

            if(levelGoals[i].numberCollected >= levelGoals[i].numberNeeded){
                goalsCompleted++;
                currentGoals[i].thisText.text = levelGoals[i].numberNeeded + "/" + levelGoals[i].numberNeeded;
            }
        }
        if(goalsCompleted >= levelGoals.Length){
            if(endGameManager != null){
                endGameManager.WinGame();
            }
            Debug.Log("WIN");
        }
    }

    public void CompareGoal(string goalToCompare){
        for(int i = 0; i< levelGoals.Length; i++){
            if(goalToCompare == levelGoals[i].matchValue){
                levelGoals[i].numberCollected++;
            }
        }
    }
}
