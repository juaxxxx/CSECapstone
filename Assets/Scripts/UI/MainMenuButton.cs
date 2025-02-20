using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{
    public UIManager.ButtonTypeEnum buttonType;
    public MainMenuMove menuMove;
    
    public Sprite notOnMouse;
    public Sprite onMouse;
    Image currentImg;

    private Color mainColor = new Color(128f / 255f, 128f / 255f, 128f / 255f);
    private Color selectColor = new Color(45f / 255f, 45f / 255f, 45f / 255f);
    private Vector3 leftPos = new Vector3(-960f, 540f, 0f);
    private Vector3 centerPos = new Vector3(960f, 540f, 0f);
    private Vector3 rightPos = new Vector3(2880f, 540f, 0f);

    private void Awake()
    {
        currentImg = GetComponent<Image>();
    }

    public void onButtonClick()
    {
        if (buttonType == UIManager.ButtonTypeEnum.Start)
        {
            menuMove.backGround.color = selectColor;
            menuMove.mainTarget = leftPos;
            menuMove.selectTarget = centerPos;
        }
        else if (buttonType == UIManager.ButtonTypeEnum.Quit)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if (buttonType == UIManager.ButtonTypeEnum.Play)
        {
            GameManager.Instance.changeSceneToStage();
        }
        else if (buttonType == UIManager.ButtonTypeEnum.Back)
        {
            menuMove.backGround.color = mainColor;
            menuMove.mainTarget = centerPos;
            menuMove.selectTarget = rightPos;
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
