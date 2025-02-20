using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum buttonTypeInGame
{
    Resume,
    Restart,
    Exit
}

public class InStageUIManager : MonoBehaviour
{
    [SerializeField]
    Slider hpBar;
    public Text hpText;
    public Text bulletText;
    public Text targetItem;
    public GameObject armDestroyed;
    public GameObject legDestroyed;
    public GameObject buffPicture;
    public GameObject buffSteam;
    public GameObject debuffPoison;
    public GameObject debuffStuck;
    public GameObject debuffSlow;
    public GameObject debuffRestraint;
    public GameObject Canvas_ESC;

    public GameObject Clear;
    public GameObject Fail;

    float maxHP;
    public float currentHP;
    public string currentBullet;
    public string currentTargetItem;

    public bool escapeDown;



    private static InStageUIManager instance = null;

    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        Canvas_ESC.SetActive(false);
        escapeDown = false;
    }
    
    void Start()
    {
        maxHP = 100;
        
    }


    public static InStageUIManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    void Update()
    {
        statusUpdate();
        menuUpDown();
    }

    void statusUpdate()
    {
        hpBar.value = currentHP / maxHP;
        hpText.text = currentHP.ToString();
    }

    void menuUpDown()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !escapeDown)
        {
            Canvas_ESC.SetActive(true);
            escapeDown = true;
            Time.timeScale = 0f;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && escapeDown)
        {
            Canvas_ESC.SetActive(false);
            escapeDown = false;
            Time.timeScale = 1f;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    public void stageClear()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Clear.SetActive(true);
    }

    public void stageFail()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Fail.SetActive(true);
    }

}
