using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
# if PLATFORM_ANDROID
using UnityEngine.Android;
# endif

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
}

[Serializable]
public class GameStateData
{
    public bool init;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;
    public GameStateData gameStateData;

    void Awake()
    {
        #if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        #endif

        if(gameData == null){
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        } 
        // 처음 GameData GameObject를 제외한 나머지는 제거 중복 예외 처리
        else if(gameData != this) {
            Destroy(this.gameObject);
        }
        
        Load();
    }

    private void Start(){
    }

    public void Save(){
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Create);
        SaveData data = new SaveData();
        data = saveData;
        formatter.Serialize(file, data);
        file.Close();
    }

    public void Load(){
        if(File.Exists(Application.persistentDataPath + "/player.dat")){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
        }
        else{
            saveData = new SaveData();
            saveData.isActive = new bool[100];
            saveData.stars = new int[100];
            saveData.highScores = new int[100];
            saveData.isActive[0] = true;
        }
    }

    private void OnApplicationPause(){
        Save();
    }

    private void OnApplicationQuit(){
        Save();
    }

    private void OnDisable(){
        Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
