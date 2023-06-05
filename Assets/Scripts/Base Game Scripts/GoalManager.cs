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
    public List<bool> goalCompleteState = new List<bool>();
    private EndGameManager endGameManager;
    private Board board;
    private ItemCollector itemCollector;
    private EffectManager effectManager;


    // Start is called before the first frame update
    void Start()
    {
        endGameManager = FindObjectOfType<EndGameManager>();
        board = FindObjectOfType<Board>();
        itemCollector = FindObjectOfType<ItemCollector>();
        effectManager = FindObjectOfType<EffectManager>();
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

            // 개발 중 저장이 되는 것으로 인한 차선책..
            levelGoals[i].numberCollected = 0;

            GameObject goal = Instantiate(goalIntroPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform, false);

            GoalPanel panel = goal.GetComponent<GoalPanel>();
            panel.thisSprite = levelGoals[i].goalSpirte;
            panel.thisString = levelGoals[i].numberNeeded.ToString();

            GameObject gameGoal = Instantiate(goalGamePrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform, false);

            panel = gameGoal.GetComponent<GoalPanel>();
            currentGoals.Add(panel);
            panel.thisSprite = levelGoals[i].goalSpirte;
            panel.thisString = "0/"+ levelGoals[i].numberNeeded;

            // for collecting effect
            GameObject child = gameGoal.transform.GetChild(0).gameObject;
            itemCollector.AddCollectItem(child.transform, levelGoals[i].matchValue);
            effectManager.AddImageObject(child);
            goalCompleteState.Add(false);
        }
    }

    public void UpdateGoals()
    {
        for(int i = 0; i< levelGoals.Length; i++){
            currentGoals[i].thisText.text = levelGoals[i].numberCollected + "/" + levelGoals[i].numberNeeded;

            if(levelGoals[i].numberCollected >= levelGoals[i].numberNeeded){
                currentGoals[i].thisText.text = levelGoals[i].numberNeeded + "/" + levelGoals[i].numberNeeded;
                currentGoals[i].thisText.color = new Color(0.098f, 1.0f, 0.098f, 1.0f); 
                goalCompleteState[i] = true;
            }
        }
    }

    public void CheckWin(){

        int goalsCompleted = 0;
        for(int i = 0; i< levelGoals.Length; i++){
            if(levelGoals[i].numberCollected >= levelGoals[i].numberNeeded){
                goalsCompleted++;
            }
        }

        if(goalsCompleted >= levelGoals.Length){
            if(endGameManager != null){
                endGameManager.WinGame();
            }
        }
    }

    public void CompareGoal(string goalToCompare){
        for(int i = 0; i< levelGoals.Length; i++){
            if(goalToCompare == levelGoals[i].matchValue){
                levelGoals[i].numberCollected++;

                
                // 글로우 임팩트
                // currentGoals[i].thisSprite;
            }
        }
    }
}
