using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoldierUI_ID : MonoBehaviour
{

    public string ID;

    public Image image;
    public Sprite ActiveSprite;
    public Sprite NonActiveSprite;

    public void Active(bool value)
    {
        if (!value)
        {
            image.sprite = NonActiveSprite;
        }

        else
        {
            image.sprite = ActiveSprite;
        }
    }
}
