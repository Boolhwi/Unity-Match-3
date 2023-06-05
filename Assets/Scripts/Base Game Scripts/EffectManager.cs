using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectManager : MonoBehaviour
{
    public Material flashMaterial;
    
    private List<GameObject> imageList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddImageObject(GameObject imageObject){
        imageList.Add(imageObject);
    }

    public void StartFlashImage(int index){
        StartCoroutine(FlashImage(index));
    }

    IEnumerator FlashImage(int index){

        Image image = imageList[index].GetComponent<Image>();

        image.material = flashMaterial;

        yield return new WaitForSeconds(0.1f);

        image.material = null;
    }
}
