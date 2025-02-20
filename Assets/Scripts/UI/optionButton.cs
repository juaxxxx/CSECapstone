using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class optionButton : MonoBehaviour
{
    public buttonTypeInGame buttonType;
    public GameObject ESC;

    public Sprite notOnMouse;
    public Sprite onMouse;
    Image currentImg;

    GameObject optionUI;

    void Awake()
    {
        optionUI = GameObject.Find("InStageUIManager");
        ESC = GameObject.Find("Canvas_ESC");
        currentImg = GetComponent<Image>();
    }

    public void OnBtnClick()
    {
        switch (buttonType)
        {
            case buttonTypeInGame.Resume:
                optionUI.GetComponent<InStageUIManager>().Canvas_ESC.SetActive(false);
                optionUI.GetComponent<InStageUIManager>().escapeDown = false;
                Time.timeScale = 1f;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                break;
            case buttonTypeInGame.Restart:
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                Time.timeScale = 1f;
                SceneManager.LoadSceneAsync("StageScene");
                break;
            case buttonTypeInGame.Exit:
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainScene");
                break;
        }
    }

    public void onPointerEnter()
    {
        currentImg.sprite = onMouse;
    }

    public void onPointerExit()
    {
        currentImg.sprite = notOnMouse;
    }
}
