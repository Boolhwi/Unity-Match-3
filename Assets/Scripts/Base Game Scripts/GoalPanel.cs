using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GoalPanel : MonoBehaviour
{
    public Image thisImage;
    public Sprite thisSprite;
    public TextMeshProUGUI thisText;
    public string thisString;

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        thisImage.sprite = thisSprite;
        thisText.text = thisString;
    }
}
