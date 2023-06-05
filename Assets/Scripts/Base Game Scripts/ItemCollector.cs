using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabTag{
    public Transform transform{ set; get; }
    public string tag{ set; get; }

    public PrefabTag(Transform initTransform, string initTag){
        this.transform = initTransform;
        this.tag = initTag;
    }
}

public class ItemCollector : MonoBehaviour
{
    public float speed;
    public Camera cam;
    public GameObject[] piecePrefabs;
    private List<PrefabTag> prefabTagArray = new List<PrefabTag>();

    private Board board;
    private GoalManager goalManager;
    private EffectManager effectManager;

    // Start is called before the first frame update
    void Start()
    {
        if(cam == null){
            cam = Camera.main;
        }

        board = FindObjectOfType<Board>();
        goalManager = FindObjectOfType<GoalManager>();
        effectManager = FindObjectOfType<EffectManager>();
    }

    public void AddCollectItem(Transform transform, string tag){
        prefabTagArray.Add(new PrefabTag(transform,tag));
    }

    public void StartMoveItem(Vector3 initial, string tag){
        GameObject targetPrefab = null;
        Transform targetTransform = null;
        int index = new int();

        for(int i = 0; i < piecePrefabs.Length; i++){
            if(tag == piecePrefabs[i].tag){
                targetPrefab = piecePrefabs[i];
            }
        }

        for(int i = 0; i< prefabTagArray.Count; i++){
            if(tag == prefabTagArray[i].tag && !goalManager.goalCompleteState[i]){
                targetTransform = prefabTagArray[i].transform;
                index = i;
            }
        }

        if(targetPrefab != null && targetTransform != null){

            Vector3 targetPos = new Vector3(targetTransform.position.x, targetTransform.position.y, cam.transform.position.z);
            GameObject item = Instantiate(targetPrefab, transform);

            Destroy(item, 5.0f);

            StartCoroutine(MoveItem(item.transform, initial, targetPos, index));
        }
    }

    IEnumerator MoveItem(Transform obj, Vector3 startPos, Vector3 endPos, int index){
        float time = 0;

        while(time < 1){
            time += speed * Time.deltaTime;
            
            if(obj != null) obj.position = Vector3.Lerp(startPos, endPos, time);

            yield return new WaitForEndOfFrame();
        }

        goalManager.UpdateGoals();

        effectManager.StartFlashImage(index);

        yield return null;
    }
}
