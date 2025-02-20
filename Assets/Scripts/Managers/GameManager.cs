using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /* enum */
    public enum DebufIndexEnum
    {
        None = 0,
        LeftArm,
        Leg,
        Poisoned,
        Slow,
        Restraint,
        Stuck
    }
    public enum EnemyIndexEnum
    {
        None = 0,
        ShortSmallEnemy,
        ShortBigEnemy,
        LongGroundEnemy,
        LongFlyEnemy
    }
    public enum ItemIndexEnum
    {
        None = 0,
        Pistol,
        Rifle,
        Shotgun,
        Machinegun,
        Sniper,
        Crowbar,
        Lightsaber,
        FirstAidKit,
        NanoCure,
        PoisonCure,
        Steampack,
        Picture
    }
    public enum WeaponIndexEnum
    {
        None = 0,
        Pistol,
        Rifle,
        Shotgun,
        Machinegun,
        Sniper,
        Crowbar,
        Lightsaber,
    }
    public enum HealIndexEnum
    {
        None = 0,
        FirstAidKit,
        NanoCure,
        PoisonCure
    }
    public enum BuffIndexEnum
    {
        None = 0,
        Steampack,
        Picture
    }

    /* Variables */
    /* Singleton */
    private static GameManager instance = null;

    /* Size Threshold */
    public Vector3[] sectionSizeThreshold = new Vector3[2];     // [0] 置社, [1] 置企
    public Vector3[] roomSizeThreshold = new Vector3[2];        // [0] 置社, [1] 置企

    /* Functions */
    public void changeSceneToStage()
    {
        SceneManager.LoadScene("StageScene");
    }
    void changeSceneToMain()
    {
        SceneManager.LoadScene("MainScene");
    }

    void Awake()
    {
        /* Singletom Set */
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static GameManager Instance
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
}
