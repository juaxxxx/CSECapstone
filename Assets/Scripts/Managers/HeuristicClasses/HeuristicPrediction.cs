using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeuristicPrediction
{
    public int inumOfTargetItemOccurence;
    public float fpropTargetAreaOccurence;

    public int iminBattleSize, imaxBattleSize;
    public int iminSpawnerNum, imaxSpawnerNum;
    public int iminGenEnemyNum, imaxGenEnemyNum;
    public float[] enemyGenProb = new float[4];
    public int iminSpawnPeriod, imaxSpawnPeriod;

    public float favgDistBetweenWeapons;
    public int iminWeaponNum, imaxWeaponNum;
    public float[] propOfWeapons = new float[7];
    public float favgDistBetweenBuff;
    public int iminBuffNum, imaxBuffNum;
    public float favgDistBetweenHeals;
    public int iminHealNum, imaxHealNum;
    public float[] propOfHeal = new float[3];

    public Vector3 minRoomSize = new Vector3(), maxRoomSize = new Vector3();
    public Vector3 minSectionSize = new Vector3(), maxSectionSize = new Vector3();
    public int inumOfEntrance;

    public HeuristicPrediction()
    {

    }
    public HeuristicPrediction(bool isDummy, Section prevSection = null)
    {
        if (isDummy)
        {
            inumOfTargetItemOccurence = StageManager.Instance.itargetItemNum / 5;
            fpropTargetAreaOccurence = 0f;
            iminBattleSize = 3; imaxBattleSize = 5;
            iminSpawnerNum = 9; imaxSpawnerNum = 15;
            iminGenEnemyNum = 1; imaxGenEnemyNum = 5;
            for (int i = 0; i < 4; i++)
                enemyGenProb[i] = 0.25f;
            iminSpawnPeriod = 5; imaxSpawnPeriod = 10;

            favgDistBetweenWeapons = 10f;
            iminWeaponNum = 3; imaxWeaponNum = 5;
            for (int i = 0; i < 7; i++)
                propOfWeapons[i] = (float)(1f / 6f);

            favgDistBetweenBuff = 10f;
            iminBuffNum = 3; imaxBuffNum = 5;

            favgDistBetweenHeals = 10f;
            iminHealNum = 3; imaxHealNum = 5;
            for (int i = 0; i < 3; i++)
                propOfHeal[i] = (float)(1f / 3);

            minRoomSize.x = 30;
            minRoomSize.z = 30;
            maxRoomSize.x = 200;
            maxRoomSize.z = 200;

            minSectionSize = new Vector3(100, 0, 100);
            maxSectionSize = new Vector3(100, 0, 100);

            inumOfEntrance = 4;
        }
        else
        {
            CalculateHeuristic(prevSection);
        }
    }

    public void CalculateHeuristic(Section prevSection)
    {
        // 비생성 섹션 비율
        float probOfTargetArea = 0f;
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
                if (!StageManager.Instance.sectionGeneratedFlag[i, j]) probOfTargetArea += 1f;
        probOfTargetArea /= 25;

        /* 초기화: 이전 섹션 + 현재 스테이지 상황 */
        // 발생할 타겟 아이템의 수 (최초 남은 개수 전량 * 남은 비생성 섹션의 비율)
        // 남은 영역이 없을 경우, 전부 생성
        if (probOfTargetArea == 0)
            inumOfTargetItemOccurence = StageManager.Instance.iremainTargetItemNum;  // StageManager.Instance.iremainTargetItemNum 로 변경
        else inumOfTargetItemOccurence = (int)((StageManager.Instance.iremainTargetItemNum) * probOfTargetArea);

        // 발생할 타겟 지역의 수
        fpropTargetAreaOccurence = (1 - probOfTargetArea);
        if (StageManager.Instance.heuristics.isGenTargetArea)
            fpropTargetAreaOccurence = 0;
        else
        {
            if (StageManager.Instance.itargetItemNum == StageManager.Instance.icurTargetItemNum + StageManager.Instance.iremainTargetItemNum)
                fpropTargetAreaOccurence *= 1.25f;
        }
        if (fpropTargetAreaOccurence > 1f)
            fpropTargetAreaOccurence = 1f;

        // iminBattleSize, imaxBattleSize
        iminBattleSize = 1000000;
        imaxBattleSize = -1;
        if(prevSection.heuristics.battles.Count > 0)
        {
            foreach (HeuristicCompsOfSection.Battle battle in prevSection.heuristics.battles)
            {
                if (battle.ispawnerNum < iminBattleSize)
                    iminBattleSize = battle.ispawnerNum;
                if (battle.ispawnerNum > imaxBattleSize)
                    imaxBattleSize = battle.ispawnerNum;
            }
        }
        

        // iminSpawnerNum, imaxSpawnerNum
        iminSpawnerNum = imaxSpawnerNum = prevSection.heuristics.ispawnerNum;

        // iminGenEnemyNum, imaxGenEnemyNum
        iminGenEnemyNum = imaxGenEnemyNum = prevSection.heuristics.iavgGenEnemyNum;

        // propOfEnmey
        enemyGenProb[0] = prevSection.heuristics.favgEnemyRateInBattle[0];
        enemyGenProb[1] = prevSection.heuristics.favgEnemyRateInBattle[1];
        enemyGenProb[2] = prevSection.heuristics.favgEnemyRateInBattle[2];
        enemyGenProb[3] = prevSection.heuristics.favgEnemyRateInBattle[3];

        // iminSpawnPeriod, imaxSpawnPeriod
        iminSpawnPeriod = imaxSpawnPeriod = (int)prevSection.heuristics.favgSpawnerPeriod;

        // favgDistBetweenWeapons
        favgDistBetweenWeapons = prevSection.heuristics.favgDistBetweenWeapons;

        // iminWeaponNum, imaxWeaponNum
        iminWeaponNum = imaxWeaponNum = prevSection.heuristics.iweaponNum;

        // propOfWeapons
        propOfWeapons[0] = prevSection.heuristics.fpropOfWeapons[0];
        propOfWeapons[1] = prevSection.heuristics.fpropOfWeapons[1];
        propOfWeapons[2] = prevSection.heuristics.fpropOfWeapons[2];
        propOfWeapons[3] = prevSection.heuristics.fpropOfWeapons[3];
        propOfWeapons[4] = prevSection.heuristics.fpropOfWeapons[4];
        propOfWeapons[5] = prevSection.heuristics.fpropOfWeapons[5];
        propOfWeapons[6] = prevSection.heuristics.fpropOfWeapons[6];

        // favgDistBetweenBuff
        favgDistBetweenBuff = prevSection.heuristics.favgDistBetweenBuffs;

        // iminBuffNum, imaxBuffNum
        iminBuffNum = imaxBuffNum = prevSection.heuristics.ibuffNum;

        // favgDistBetweenHeals
        favgDistBetweenHeals = prevSection.heuristics.favgDistBetweenHeals;

        // iminHealNum, imaxHealNum
        iminHealNum = imaxHealNum = prevSection.heuristics.iHealNum;

        // propOfHeal
        propOfHeal[0] = prevSection.heuristics.fpropOfHeals[0];
        propOfHeal[1] = prevSection.heuristics.fpropOfHeals[1];
        propOfHeal[2] = prevSection.heuristics.fpropOfHeals[2];

        // minRoomSize, maxRoomSize
        minRoomSize = prevSection.heuristics.avgRoomSize - prevSection.heuristics.avgRoomSize * 2;
        maxRoomSize = prevSection.heuristics.avgRoomSize + prevSection.heuristics.avgRoomSize * 2;
        if (minRoomSize.x < 30) minRoomSize.x = 30;
        if (minRoomSize.z < 30) minRoomSize.z = 30;
        if (maxRoomSize.x < 30) maxRoomSize.x = 30;
        if (maxRoomSize.z > 200) maxRoomSize.z = 200;

        // minSectionSize, maxSectionSize
        minSectionSize = maxSectionSize = prevSection.sectionSize;

        // inumOfEntrance
        inumOfEntrance = prevSection.heuristics.inumOfExit;
    }
}
