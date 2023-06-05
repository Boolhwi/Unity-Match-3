using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScalar : MonoBehaviour
{

    private Board board;
    public float cameraOffset;
    public float aspectRatio = 0.625f;
    public float padding = 2;
    public float xOffset = 1.0f;
    public float yOffset = 1.5f;

    private Vector3 initPosition;
    private float shakeTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        if(board != null){
            RepositionCamera(board.width - 1, board.height - 1);
        }

        initPosition = transform.position;
    }

    void RepositionCamera(float x, float y){
        Vector3 tempPosition = new Vector3(x/2 - xOffset, y/2 + yOffset, cameraOffset);
        transform.position = tempPosition;
        Camera.main.orthographicSize = 5.0f;
        // if(board.width >= board.height){
        //     Camera.main.orthographicSize = (board.width/2 + padding) / aspectRatio;
        // }
        // else {
        //     Camera.main.orthographicSize = board.height/2 + padding;
        // }
    }


    void Update()
    {
        if(shakeTime >= 0){
            transform.position = Random.insideUnitSphere * 0.1f + initPosition;
            shakeTime -= Time.deltaTime;
        } else {
            transform.position = initPosition;
        }
    }

    public void CameraShake(float time){
        shakeTime = time;
    }
}
