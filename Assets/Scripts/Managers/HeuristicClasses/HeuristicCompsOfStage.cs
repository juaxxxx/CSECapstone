using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

public class HeuristicCompsOfStage
{
    /* Variables */
    /* Thresholds */
    public Vector3 maxRoomSize = new Vector3(100, 0, 100);
    
    /* Elements */
    public float[] fweaponTimes = Enumerable.Repeat<float>(0f, 7).ToArray<float>();
    public GameManager.WeaponIndexEnum oldWeapon = GameManager.WeaponIndexEnum.None;
    public int[] ihitEnemyNums = Enumerable.Repeat<int>(0, 4).ToArray<int>();
    public int[] igenEnemyNums = Enumerable.Repeat<int>(0, 4).ToArray<int>();
    public int[] idieEnemyNums = Enumerable.Repeat<int>(0, 4).ToArray<int>();
    public float fbattleTime = 0f;
    public float ftotalDeals = 0f;
    public float fDPS = 0f;
    public int isectionIndexInPlayer = 0;
    public int iroomIndexPlayer = 0;
    public int imoveCnt = 0;
    public int igoodChoiceCnt = 0;
    public bool isGenTargetArea = false;

    /* Functions */
    public void UpdateWherePlayer(Vector3 pos, Section section)
    {
        if (section == null) return;
        for (int i = 0; i < section.rooms.Count; i++)
        {
            if (section.rooms[i].CheckInRoom(pos))
            {
                iroomIndexPlayer = i;
                break;
            }
        }
    }
    public void UpdateWeaponTime(GameManager.WeaponIndexEnum weaponIdx)
    {
        fweaponTimes[(int)weaponIdx - 1] += 1;
    }
    public void UpdateHitEnemyNum(GameManager.EnemyIndexEnum enemyIdx)
    {
        ihitEnemyNums[(int)enemyIdx - 1] += 1;
    }
    public float GetHitEnemyRate(GameManager.EnemyIndexEnum enemyIndex)
    {
        float total = 0f;
        for (int i = 0; i < ihitEnemyNums.Length; i++)
            total += (float)ihitEnemyNums[i];
        return total / (float)ihitEnemyNums[(int)enemyIndex];
    }
    public void UpdateBattleTime()
    {
        fbattleTime += 1f;
    }
    public void UpdateDPS(float damage)
    {
        ftotalDeals += damage;
        fDPS = ftotalDeals / fbattleTime;
    }
    public float GetGoodChoiceRate()
    {
        return (float)(igoodChoiceCnt / imoveCnt);
    }

}
