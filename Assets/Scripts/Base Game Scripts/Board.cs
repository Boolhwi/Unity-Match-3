using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind{
    Breakable,
    Blank,
    Lock,
    Concrete,
    Slime,
    Normal
}


[System.Serializable]
public class MatchType {
    public int type;
    public string color;
}

[System.Serializable]
public class TileType{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{

    
    [Header ("Scriptable Object Stuff")]
    public World world;
    public int level;

    public GameState currentState = GameState.move;

    [Header ("Board Dimensions")]
    public int width;
    public int height;
    public int offset;

    [Header ("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject slimePiecePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;
    public GameObject comboTextPrefab;

    [Header ("Layout")]
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    private BackgroundTile[,] concreteTiles;
    private BackgroundTile[,] slimeTiles;
    public GameObject[,] allDots;

    [Header ("Match Stuff")]
    public MatchType matchType;
    public Dot currentDot;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private bool streakTextShow = false;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private GoalManager goalManager;
    private EndGameManager endGameManager;
    public float refillDelay = 1f;
    public int [] scoreGoals;
    private bool makeSlime = true;

    private CameraScalar cameraScalar;
    private ItemCollector itemCollector;

    private void Awake(){
        if(PlayerPrefs.HasKey("Current Level")){
            level = PlayerPrefs.GetInt("Current Level");
        }
        if(world != null){
            if(world.levels[level] != null){
                width = world.levels[level].width;
                height = world.levels[level].height;
                dots = world.levels[level].dots;
                scoreGoals = world.levels[level].scoreGoals;
                boardLayout = world.levels[level].boardLayout;

            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        breakableTiles = new BackgroundTile[width,height];
        lockTiles = new BackgroundTile[width,height];
        concreteTiles = new BackgroundTile[width,height];
        slimeTiles = new BackgroundTile[width,height];

        findMatches = FindObjectOfType<FindMatches>();
        scoreManager = FindObjectOfType<ScoreManager>();
        soundManager = FindObjectOfType<SoundManager>();
        goalManager = FindObjectOfType<GoalManager>();
        endGameManager = FindObjectOfType<EndGameManager>();
        cameraScalar = FindObjectOfType<CameraScalar>();
        itemCollector = FindObjectOfType<ItemCollector>();

        blankSpaces = new bool[width,height];
        allDots = new GameObject[width,height];
        SetUp();
        currentState = GameState.pause;
    }

    public void GenerateBlankSpaces(){
        for(int i=0; i<boardLayout.Length; i++){
            if(boardLayout[i].tileKind == TileKind.Blank){
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles(){
        for(int i=0; i<boardLayout.Length; i++){
            if(boardLayout[i].tileKind == TileKind.Breakable){
                Vector2 tempPosition = new Vector2(boardLayout[i].x,boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                tile.transform.parent = this.transform;
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateLockTiles(){
        for(int i=0; i<boardLayout.Length; i++){
            if(boardLayout[i].tileKind == TileKind.Lock){
                Vector2 tempPosition = new Vector2(boardLayout[i].x,boardLayout[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                tile.transform.parent = this.transform;
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateConcreteTiles(){
        for(int i=0; i<boardLayout.Length; i++){
            if(boardLayout[i].tileKind == TileKind.Concrete){
                Vector2 tempPosition = new Vector2(boardLayout[i].x,boardLayout[i].y);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                tile.transform.parent = this.transform;
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateSlimeTiles(){
        for(int i=0; i<boardLayout.Length; i++){
            if(boardLayout[i].tileKind == TileKind.Slime){
                Vector2 tempPosition = new Vector2(boardLayout[i].x,boardLayout[i].y);
                GameObject tile = Instantiate(slimePiecePrefab, tempPosition, Quaternion.identity);
                tile.transform.parent = this.transform;
                slimeTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp() 
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();

        for(int i = 0; i<width; i++) {
            for(int j =0; j<height; j++) {
                if(!blankSpaces[i,j] && !concreteTiles[i,j] && !slimeTiles[i,j]){      
                    Vector2 tempPosition = new Vector2(i,j + offset);

                    int dotToUse = Random.Range(0, dots.Length);

                    while(MatchesAt(i,j,dots[dotToUse])){
                        dotToUse = Random.Range(0, dots.Length);
                    }

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                    dot.GetComponent<Dot>().previousRow = j;
                    dot.GetComponent<Dot>().previousColumn = i;

                    dot.transform.parent = this.transform;
                    dot.name = "( "+i+", "+j+" )";

                    allDots[i, j] = dot;
                }

                if(!blankSpaces[i,j]){
                    Vector2 tempPosition = new Vector2(i,j + offset);
                    // Dot GameObject는 Decrease되는 반면에 BackgroundTile Object는 Decrease되지 않기에 별도로 선언해줘야한다.
                    Vector2 tilePosition = new Vector2(i,j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( "+i+", "+j+" )";
                }
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if(column > 1 && row > 1){
            if(allDots[column-1, row] != null && allDots[column-2, row] != null){
                if(allDots[column-1, row].tag == piece.tag && allDots[column-2, row].tag == piece.tag) {
                    return true;
                }
            }
            
            if(allDots[column, row-1] != null && allDots[column, row-2] != null){
                if(allDots[column, row-1].tag == piece.tag && allDots[column, row-2].tag == piece.tag) {
                    return true;
                }
            }
        } else if(column <= 1 || row <= 1) {
            if(column > 1) {    
                if(allDots[column-1, row] != null && allDots[column-2, row] != null){
                    if(allDots[column-1, row].tag == piece.tag && allDots[column-2, row].tag == piece.tag) {
                        return true;
                    }   
                }
            }
            if(row > 1) {
                if(allDots[column, row-1] != null && allDots[column, row-2] != null){
                    if(allDots[column, row-1].tag == piece.tag && allDots[column, row-2].tag == piece.tag) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public MatchType ColumnOrRow(){
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;

        matchType.type = 0;
        matchType.color = "";

        for(int i = 0; i< matchCopy.Count; i++){
            Dot thisDot = matchCopy[i].GetComponent<Dot>();
            string color = matchCopy[i].tag;
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;

            for(int j = 0; j< matchCopy.Count; j++){
                Dot nextDot = matchCopy[j].GetComponent<Dot>();
                if(thisDot == nextDot){
                    continue;
                }
                if(nextDot.column == thisDot.column && nextDot.tag == color){
                    columnMatch++;
                } 
                if(nextDot.row == thisDot.row && nextDot.tag == color){
                    rowMatch++;
                }
            }

            // color bomb
            if(columnMatch == 4 || rowMatch == 4){
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
            // adjacent bomb
            else if(columnMatch == 2 && rowMatch == 2){
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            // column/row bomb
            else if(columnMatch == 3 || rowMatch == 3){
                matchType.type = 3;
                matchType.color = color;
                return matchType;
            }
        }

        return matchType;
    }

    private void CheckToMakeBomb(){
        if(findMatches.currentMatches.Count > 3){
            MatchType typeOfMatch = ColumnOrRow();
            if(typeOfMatch.type == 1){
                if(currentDot != null){
                    if(currentDot.isMatched && currentDot.tag == typeOfMatch.color){
                        currentDot.isMatched= false;
                        currentDot.MakeColorBomb();
                    } else{
                        if(currentDot.otherDot != null){
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if(otherDot.isMatched && otherDot.tag == typeOfMatch.color){
                                otherDot.isMatched= false;
                                otherDot.MakeColorBomb();
                            }
                        }
                    }
                } else {
                    for(int i = 0; i < findMatches.currentMatches.Count; i++){
                        Dot targetDot = findMatches.currentMatches[i].GetComponent<Dot>();
                        if(targetDot.tag == typeOfMatch.color){
                            targetDot.isMatched= false;
                            targetDot.MakeColorBomb();
                            return;
                        };
                    }
                }
            }
            else if(typeOfMatch.type == 2){
                if(currentDot != null){
                    if(currentDot.isMatched && currentDot.tag == typeOfMatch.color){
                        currentDot.isMatched= false;
                        currentDot.MakeAdjacentBomb();
                    } else{
                        if(currentDot.otherDot != null){
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if(otherDot.isMatched && otherDot.tag == typeOfMatch.color){
                                otherDot.isMatched= false;
                                otherDot.MakeAdjacentBomb();
                            }
                        }
                    }
                } else{
                    for(int i = 0; i < findMatches.currentMatches.Count; i++){
                        Dot targetDot = findMatches.currentMatches[i].GetComponent<Dot>();
                        if(targetDot.tag == typeOfMatch.color){
                            targetDot.isMatched= false;
                            targetDot.MakeAdjacentBomb();
                            return;
                        };
                    }
                }
            }
            else if(typeOfMatch.type == 3){
                findMatches.CheckBombs(typeOfMatch);   
            }
        }
    }

    public void BombRow(int row){
        // for(int i =0; i< width; i++){
        //     if(concreteTiles[i,row]){
        //         concreteTiles[i,row].TakeDamage(1);
        //         if(concreteTiles[i, row].hitPoints <=0){
        //             concreteTiles[i, row] = null;
        //         }
        //     }
        // }
    }

    public void BombColumn(int column){
        // for(int i =0; i< height; i++){
        //     if(concreteTiles[column,i]){
        //         concreteTiles[column, i].TakeDamage(1);
        //         if(concreteTiles[column, i].hitPoints <=0){
        //             concreteTiles[column, i] = null;
        //         }
        //     }
        // }
    }

    private void DestroyMatchesAt(int column, int row) {

        // 이 코드의 문제점은 currentDot이 가장 먼저 matching된 dots 중에 처리 될 때, 인덱스가 빠른 경우!
        // 폭탄인 채로 사라진다는 것이다. 다른 얘들이 먼저 돌아줘야 currentDot이 isMatched가 false로 바뀌면서
        // 다른 얘들 삭제 루틴에 의해 폭탄으로 생성되고 정작 본인은 삭제 루틴을 안타서 살아남음
        if(allDots[column,row].GetComponent<Dot>().isMatched){
            if(breakableTiles[column, row] != null){
                breakableTiles[column, row].TakeDamage(1);
                itemCollector.StartMoveItem(breakableTiles[column, row].gameObject.transform.position,breakableTiles[column, row].gameObject.tag);

                // 굳이 필요한 코드인가 싶다. 어차피 GameObject 삭제하면 스크립트도 날라갈텐데.. 주소를 가리키고 있어서 garbage가 쌓여서 그런가
                // -> 필요한 코드다 스크립트가 날라가는건 BackgroundTile의 스크립트지.. Board.breakableTiles 객체는 남아있기에
                if(breakableTiles[column, row].hitPoints <=0){
                    breakableTiles[column, row] = null;
                }
            }

            if(lockTiles[column, row] != null){
                lockTiles[column, row].TakeDamage(1);
                itemCollector.StartMoveItem(lockTiles[column, row].gameObject.transform.position,lockTiles[column, row].gameObject.tag);

                if(lockTiles[column, row].hitPoints <=0){
                    lockTiles[column, row] = null;
                }
            }

            DamageConcrete(column, row);
            DamageSlime(column, row);

            if(goalManager != null){
                goalManager.CompareGoal(allDots[column,row].tag.ToString());
            }

            if(soundManager != null){
                soundManager.PlayRandomDestroyNoise();
            }

            findMatches.currentMatches.Remove(allDots[column,row]);
            GameObject particle = Instantiate(destroyEffect, allDots[column,row].transform.position,Quaternion.identity);
            Destroy(particle, .5f);

            // 콤보 텍스트 띄우기
            if(!streakTextShow && streakValue > 1){
                Vector3 streakTextPos = allDots[column,row].transform.position;
                streakTextPos.y += 1.0f;
                GameObject comboText = Instantiate(comboTextPrefab, streakTextPos,Quaternion.identity);
                
                TextMesh textMesh = comboText.GetComponentInChildren<TextMesh>();
                textMesh.text = "COMBO x " + streakValue.ToString();

                // 콤보 수에 따른 색깔 변경
                switch(streakValue){
                    case 2:
                        textMesh.color = new Color(0.925f, 0.516f, 0.271f, 1.0f);
                        break;
                    case 3:
                        textMesh.color = new Color(0.851f, 0.357f, 0.263f, 1.0f);
                        break;
                    case 4:
                        textMesh.color = new Color(0.753f, 0.161f, 0.259f, 1.0f);
                        break;
                    default:
                        textMesh.color = new Color(0.329f, 0.141f, 0.216f, 1.0f);
                        break;
                }
                    
                Destroy(comboText, .5f);
                streakTextShow = true;
            }

            itemCollector.StartMoveItem(allDots[column,row].transform.position,allDots[column,row].tag);
  
            // allDots[column, row].GetComponent<Dot>().PopAnimation();
            Destroy(allDots[column,row]);

            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[column,row] = null;
        }
    }

    public void DestroyMatches(){
        if(findMatches.currentMatches.Count >= 4 ){
            CheckToMakeBomb();          
        }
        for(int i=0; i<width;i++){
            for(int j=0; j<height;j++){
                if(allDots[i,j] != null){
                    DestroyMatchesAt(i,j);
                }
            }
        }

        streakTextShow = false;

        // 카메라 쉐이크 효과
        cameraScalar.CameraShake(0.5f);

        
        // 이 위치는 맞는지 모르겠음
        // 이 위치가 맞음. 미리 다 삭제해 버리면 Remove할 것이 없어서 에러가 날 것임
        // 다 끝나고 살아남은 폭탄만 리스트에서 제거해야함
        findMatches.currentMatches.Clear();
        currentDot = null;

        StartCoroutine(DecreaseRowGo2());
    }

    private void DamageConcrete(int column, int row){
        int c = -1;
        int r = -1;

        if(column > 0){
            if(concreteTiles[column -1, row]){
                c = column -1;
                r = row;
            }
        }
        if(column < width -2){
            if(concreteTiles[column +1, row]){
                c = column +1;
                r = row;
            }
        }
        if(row > 0){
            if(concreteTiles[column, row -1]){
                c = column;
                r = row -1;
            }
        }
        if(row < height -2){
            if(concreteTiles[column, row +1]){
                c = column;
                r = row +1;
            }
        }

        if(c != -1 && r != -1){
            itemCollector.StartMoveItem(concreteTiles[c, r].gameObject.transform.position,concreteTiles[c, r].gameObject.tag);
            concreteTiles[c, r].TakeDamage(1);
            if(concreteTiles[c, r].hitPoints <=0){
                concreteTiles[c, r] = null;
            }
        }
    }

    private void DamageSlime(int column, int row){
        int c = -1;
        int r = -1;

        if(column > 0){
            if(slimeTiles[column -1, row]){
                c = column -1;
                r = row;
            }
        }
        if(column < width -2){
            if(slimeTiles[column +1, row]){
                c = column +1;
                r = row;
            }
        }
        if(row > 0){
            if(slimeTiles[column, row -1]){
                c = column;
                r = row -1;
            }
        }
        if(row < height -2){
            if(slimeTiles[column, row +1]){
                c = column;
                r = row +1;
            }
        }

        if(c != -1 && r != -1){
            itemCollector.StartMoveItem(slimeTiles[c, r].gameObject.transform.position,slimeTiles[c, r].gameObject.tag);
            slimeTiles[c, r].TakeDamage(1);
            if(slimeTiles[c, r].hitPoints <=0){
                slimeTiles[c, r] = null;
            }
            makeSlime = false;
        }
    }

    private IEnumerator DecreaseRowGo2(){
        for(int i=0; i<width;i++){
            for(int j=0; j<height;j++){
                if(!blankSpaces[i,j] && allDots[i,j] == null && !concreteTiles[i,j] && !slimeTiles[i,j]){
                    for(int k=j; k<height; k++){
                        if(allDots[i,k] != null){
                            allDots[i,k].GetComponent<Dot>().row = j;
                            allDots[i,k] = null;
                            break;
                        }
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardGo());
    }

    // private IEnumerator DecreaseRowGo(){
    //     int nullCount = 0;
    //     for(int i=0; i<width;i++){
    //         for(int j=0; j<height;j++){
    //             if(allDots[i,j] ==null){
    //                 nullCount++;
    //             } else if(nullCount >0){
    //                 allDots[i,j].GetComponent<Dot>().row -= nullCount;
    //                 allDots[i,j].GetComponent<Dot>().previousRow -= nullCount;
    //                 allDots[i,j].GetComponent<Dot>().previousColumn = allDots[i,j].GetComponent<Dot>().column;
    //                 allDots[i,j] = null;
    //             }
    //         }
    //         nullCount=0;
    //     } 
    //     yield return new WaitForSeconds(refillDelay * 0.5f);
    //     StartCoroutine(FillBoardGo());
    // }

    private void RefillBoard(){
        for(int i=0; i<width;i++){
            for(int j=0; j<height;j++){
                if(allDots[i,j] == null && !blankSpaces[i,j] && !concreteTiles[i,j] && !slimeTiles[i,j]) {
                    Vector2 tempPosition = new Vector2 (i,j+offset);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    piece.transform.parent = this.transform;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                    piece.GetComponent<Dot>().previousRow = j;
                    piece.GetComponent<Dot>().previousColumn = i;
                    allDots[i,j] = piece;
                }
            }
        }
    }

    private bool MatchesOnBoard(){
        findMatches.FindAllMatches();
        for(int i=0; i<width;i++){
            for(int j=0; j<height;j++){
                if(allDots[i,j] != null){
                    if(allDots[i,j].GetComponent<Dot>().isMatched){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void CheckToMakeSlime(){
        for(int i=0; i<width;i++){
            for(int j=0; j<height;j++){
                if(slimeTiles[i,j] != null && makeSlime){
                    MakeNewSlime();
                }
            }
        }
    }

    private Vector2 CheckForAdjacent(int column, int row){
        if(column < width - 2 && allDots[column + 1, row] ){
            return Vector2.right;
        }
        if(column > 0 && allDots[column - 1, row]){
            return Vector2.left;
        }
        if(row < height - 2 && allDots[column, row + 1]){
            return Vector2.up;
        }
        if(row > 0 && allDots[column, row - 1]){
            return Vector2.down;
        }
        return Vector2.zero;
    }

    private void MakeNewSlime(){
        bool slime = false;
        int loops = 0;
        while(!slime && loops < 200){
            int newX = Random.Range(0, width);
            int newY = Random.Range(0, height);
            if(slimeTiles[newX, newY]){
                Vector2 adjacent = CheckForAdjacent(newX, newY);
                if(adjacent != Vector2.zero){
                    Destroy(allDots[newX + (int)adjacent.x, newY + (int)adjacent.y]);
                    Vector2 tempPosition = new Vector2(newX + (int)adjacent.x, newY + (int)adjacent.y);
                    GameObject tile = Instantiate(slimePiecePrefab, tempPosition, Quaternion.identity);
                    tile.transform.parent = this.transform;
                    slimeTiles[newX + (int)adjacent.x, newY + (int)adjacent.y] = tile.GetComponent<BackgroundTile>();
                    makeSlime = false;
                    slime = true;
                }
            }
            loops++;
        }
    }

    private IEnumerator FillBoardGo(){

        // 마지막으로 채워진 것에 대해서는 매칭 안되는 버그 발생
        // 아마 내려오는 시간으로 인해 isMatched = false인 상태로 아래 조건문을 통과하는 것 같음
        yield return new WaitForSeconds(refillDelay);
        RefillBoard(); // 굳이 위치를 아래로 내려야할까
        yield return new WaitForSeconds(refillDelay);
        while(MatchesOnBoard()){
            streakValue++;
            DestroyMatches();
            yield break;
        }
        currentDot = null;

        // 승리 조건 체크
        goalManager.CheckWin();

        // 패배 조건 체크
        if(endGameManager != null){
            // 패배 조건이 이동 횟수인지 체크
            if(endGameManager.requirements.gameType == GameType.Moves){
                endGameManager.DecreaseCounterValue();
            }
        }

        CheckToMakeSlime();

        if(IsDeadlocked()){
            ShuffleBoard();
        }
        if(currentState != GameState.pause){
            currentState = GameState.move;
        }
        streakValue = 1;
        makeSlime = true;
    }

    private void SwitchPieces(int column, int row, Vector2 direction){
        if(allDots[column + (int)direction.x, row + (int)direction.y] != null){
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
            allDots[column, row] = holder;
        }
    }

    private bool CheckForMatches(){
        for(int i =0; i< width; i++){
            for(int j =0; j< height; j++){
                if(allDots[i,j] != null){
                    if(i < width -2){
                        if(allDots[i+1,j] != null && allDots[i+2,j] != null){
                            if(allDots[i+1,j].tag == allDots[i,j].tag && allDots[i+2,j].tag == allDots[i,j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    if(j < height -2){
                        if(allDots[i,j+1] != null && allDots[i,j+2] != null){
                            if(allDots[i,j+1].tag == allDots[i,j].tag && allDots[i,j+2].tag == allDots[i,j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction){
        SwitchPieces(column, row, direction);
        if(CheckForMatches()){
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadlocked(){     
        for(int i =0; i< width; i++){
            for(int j =0; j< height; j++){
                if(allDots[i,j] != null){
                    if(i < width -1){
                        if(SwitchAndCheck(i,j,Vector2.right)){
                            return false;
                        }
                    }
                    if(j < height -1){
                        if(SwitchAndCheck(i,j,Vector2.up)){
                            return false;
                        }
                    }
                }
            }
        }
        return true;   
    }

    private void ShuffleBoard(){

        List<GameObject> newBoard = new List<GameObject>();

        for(int i =0; i< width; i++){
            for(int j =0; j< height; j++){
                if(allDots[i,j] != null){
                    newBoard.Add(allDots[i,j]);
                }
            }
        }

        for(int i =0; i< width; i++){
            for(int j =0; j< height; j++){
                if(!blankSpaces[i,j] && !concreteTiles[i,j] && !slimeTiles[i,j]){
                    int pieceToUse = Random.Range(0, newBoard.Count);

                    while(MatchesAt(i,j,newBoard[pieceToUse])){
                        pieceToUse = Random.Range(0, newBoard.Count);
                    }

                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    piece.column = i;
                    piece.row = j;
                    allDots[i,j] = newBoard[pieceToUse];
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }

        if(IsDeadlocked()){
            ShuffleBoard();
        }
    }
}
