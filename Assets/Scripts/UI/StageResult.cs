using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageResult : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[2];
    Image currentImg;

    float delayTime = 1f;
    float accumulatedTime = 0f;
    int currentState = 0;

    private void Awake()
    {
        currentImg = GetComponent<Image>();
    }

    private void Update()
    {
        messageTwinkle();
    }

    void messageTwinkle()
    {
        accumulatedTime += Time.deltaTime;
        if (accumulatedTime > delayTime)
        {
            if (currentState == 0)
                currentState = 1;
            else
                currentState = 0;

            currentImg.sprite = sprites[currentState];
            accumulatedTime = 0f;
        }
    }
}
