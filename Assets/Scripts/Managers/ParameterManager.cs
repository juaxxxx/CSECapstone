using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class ParameterManager : MonoBehaviour
{
    /* Enums */
    public enum InputEnum
    {
        None = 0,
        characterHP,
        avgDistBetweenSpawners,
        avgBattleSize,
        spawnerNum,
        avgGenEnemyNum,
        avgEnemyRateInBattle_SS,
        avgEnemyRateInBattle_SB,
        avgEnemyRateInBattle_LG,
        avgEnemyRateInBattle_LF,
        avgSpawnerPeriod,
        avgDistBetweenWeapons,
        weaponNum,
        propOfWeapons_Pistol,
        propOfWeapons_Rifle,
        propOfWeapons_Shotgun,
        propOfWeapons_Machinegun,
        propOfWeapons_Sniper,
        propOfWeapons_Crowbar,
        propOfWeapons_Lightsaber,
        avgDistBetweenBuffs,
        buffNum,
        avgDistBetweenHeals,
        healNum,
        propOfHeals_FirstAidKit,
        propOfHeals_NanoCure,
        propOfHeals_PoisionCure,
        avgRoomSize_x,
        avgRoomSize_y,
        avgRoomSize_z,
        numOfExit
    }
    public enum OutputEnum
    {
        None = 0,
        numOfTargetItemOccurence, propTargetAreaOccurrence,
        minBattleSize, maxBattleSize,
        minSpawnerNum, maxSpawnerNum,
        minGenEnemyNum, maxGenEnemyNum,
        EnemyGenProb_SS,
        EnemyGenProb_SB,
        EnemyGenProb_LG,
        EnemyGenProb_LF,
        minSpawnPeriod, maxSpawnPeriod,
        avgDistBetweenWeapons,
        minWeaponNum, maxWeaponNum,
        propOfWeapons_Pistol,
        propOfWeapons_Rifle,
        propOfWeapons_Shotgun,
        propOfWeapons_Machinegun,
        propOfWeapons_Sniper,
        propOfWeapons_Crowbar,
        propOfWeapons_Lightsaber,
        avgDistBetweenBuff,
        minBuffNum, maxBuffNum,
        avgDistBetweenHeals,
        minHealNum, maxHealNum,
        propOfHeals_FirstAidKit,
        propOfHeals_NanoCure,
        propOfHeals_PoisionCure,
        minRoomSize_x, minRoomSize_y, minRoomSize_z,
        maxRoomSize_x, maxRoomSize_y, maxRoomSize_z,
        numOfExit
    }
    public enum CompareEnum
    {
        None = 0,
        Less, LessEqual, Equal, NotEqual, Greater, GreaterEqual
    }

    /* Classes */
    [System.Serializable]
    public class ApplyFormat
    {
        /// <summary>
        /// if(conditionalInput {comp} fcondition)
        ///     output =  output + ((factorInput * ffactor) / output);
        /// </summary>
        public OutputEnum output;
        public InputEnum conditionalInput;
        public float fcondition;
        public CompareEnum comp;
        public InputEnum factorInput;
        public float ffactor;
    }

    /* Variables */
    public int outputNum = 20;
    [SerializeField]
    public List<ApplyFormat> rules = new List<ApplyFormat>();

    /* Singleton */
    private static ParameterManager instance = null;
    public static ParameterManager Instance
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

    /* Functions */
    float normalize_MM(float output, float input)
    {
        return output + (input / output);
    }
    public void runRules(MapGenerator mapGen, HeuristicPrediction pred)
    {
        HeuristicCompsOfSection prevHeuristicComps = mapGen.prevSection.heuristics;
        for (int i = 0; i < rules.Count; i++)
        {
            float conditionalFloat = -1000000f;
            switch (rules[i].conditionalInput)
            {
                case InputEnum.characterHP:
                    conditionalFloat = StageManager.Instance.player.fhp;
                    break;
                case InputEnum.avgDistBetweenSpawners:
                    conditionalFloat = prevHeuristicComps.favgDistBetweenSpawners;
                    break;
                case InputEnum.avgBattleSize:
                    conditionalFloat = prevHeuristicComps.favgBattleSize;
                    break;
                case InputEnum.spawnerNum:
                    conditionalFloat = (int)prevHeuristicComps.ispawnerNum;
                    break;
                case InputEnum.avgGenEnemyNum:
                    conditionalFloat = (float)(prevHeuristicComps.iavgGenEnemyNum);
                    break;
                case InputEnum.avgEnemyRateInBattle_SS:
                    conditionalFloat = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.ShortSmallEnemy];
                    break;
                case InputEnum.avgEnemyRateInBattle_SB:
                    conditionalFloat = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.ShortBigEnemy];
                    break;
                case InputEnum.avgEnemyRateInBattle_LG:
                    conditionalFloat = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.LongGroundEnemy];
                    break;
                case InputEnum.avgEnemyRateInBattle_LF:
                    conditionalFloat = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.LongFlyEnemy];
                    break;
                case InputEnum.avgSpawnerPeriod:
                    conditionalFloat = prevHeuristicComps.favgSpawnerPeriod;
                    break;
                case InputEnum.avgDistBetweenWeapons:
                    conditionalFloat = prevHeuristicComps.favgDistBetweenWeapons;
                    break;
                case InputEnum.weaponNum:
                    conditionalFloat = (float)prevHeuristicComps.iweaponNum;
                    break;
                case InputEnum.propOfWeapons_Pistol:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Pistol];
                    break;
                case InputEnum.propOfWeapons_Rifle:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Rifle];
                    break;
                case InputEnum.propOfWeapons_Shotgun:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Shotgun];
                    break;
                case InputEnum.propOfWeapons_Machinegun:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Machinegun];
                    break;
                case InputEnum.propOfWeapons_Sniper:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Sniper];
                    break;
                case InputEnum.propOfWeapons_Crowbar:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Crowbar];
                    break;
                case InputEnum.propOfWeapons_Lightsaber:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Lightsaber];
                    break;
                case InputEnum.avgDistBetweenBuffs:
                    conditionalFloat = prevHeuristicComps.favgDistBetweenBuffs;
                    break;
                case InputEnum.buffNum:
                    conditionalFloat = (float)prevHeuristicComps.ibuffNum;
                    break;
                case InputEnum.avgDistBetweenHeals:
                    conditionalFloat = prevHeuristicComps.favgDistBetweenHeals;
                    break;
                case InputEnum.healNum:
                    conditionalFloat = (float)prevHeuristicComps.iHealNum;
                    break;
                case InputEnum.propOfHeals_FirstAidKit:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfHeals[(int)GameManager.HealIndexEnum.FirstAidKit];
                    break;
                case InputEnum.propOfHeals_NanoCure:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfHeals[(int)GameManager.HealIndexEnum.NanoCure];
                    break;
                case InputEnum.propOfHeals_PoisionCure:
                    conditionalFloat = (float)prevHeuristicComps.fpropOfHeals[(int)GameManager.HealIndexEnum.PoisonCure];
                    break;
                case InputEnum.avgRoomSize_x:
                    conditionalFloat = (float)prevHeuristicComps.avgRoomSize.x;
                    break;
                case InputEnum.avgRoomSize_y:
                    conditionalFloat = (float)prevHeuristicComps.avgRoomSize.y;
                    break;
                case InputEnum.avgRoomSize_z:
                    conditionalFloat = (float)prevHeuristicComps.avgRoomSize.z;
                    break;
                case InputEnum.numOfExit:
                    conditionalFloat = (float)prevHeuristicComps.inumOfExit;
                    break;
                default: // None
                    break;
            }

            // 조건 입력이 None인 경우
            if (conditionalFloat <= -1000000f)
                continue;

            bool isTrue = false;
            switch (rules[i].comp)
            {
                case CompareEnum.Less:
                    isTrue = (conditionalFloat < rules[i].fcondition);
                    break;
                case CompareEnum.LessEqual:
                    isTrue = (conditionalFloat <= rules[i].fcondition);
                    break;
                case CompareEnum.Equal:
                    isTrue = (conditionalFloat == rules[i].fcondition);
                    break;
                case CompareEnum.NotEqual:
                    isTrue = (conditionalFloat != rules[i].fcondition);
                    break;
                case CompareEnum.GreaterEqual:
                    isTrue = (conditionalFloat >= rules[i].fcondition);
                    break;
                case CompareEnum.Greater:
                    isTrue = (conditionalFloat > rules[i].fcondition);
                    break;
                default:
                    break;
            }
            
            // 비교기가 None이거나, 비교 걸과 false
            if (!isTrue) continue;

            float factorInput = -1000000f;
            switch(rules[i].factorInput)
            {
                case InputEnum.characterHP:
                    factorInput = StageManager.Instance.player.fhp;
                    break;
                case InputEnum.avgDistBetweenSpawners:
                    factorInput = prevHeuristicComps.favgDistBetweenSpawners;
                    break;
                case InputEnum.avgBattleSize:
                    factorInput = prevHeuristicComps.favgBattleSize;
                    break;
                case InputEnum.spawnerNum:
                    factorInput = (int)prevHeuristicComps.ispawnerNum;
                    break;
                case InputEnum.avgGenEnemyNum:
                    factorInput = (float)(prevHeuristicComps.iavgGenEnemyNum);
                    break;
                case InputEnum.avgEnemyRateInBattle_SS:
                    factorInput = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.ShortSmallEnemy];
                    break;
                case InputEnum.avgEnemyRateInBattle_SB:
                    factorInput = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.ShortBigEnemy];
                    break;
                case InputEnum.avgEnemyRateInBattle_LG:
                    factorInput = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.LongGroundEnemy];
                    break;
                case InputEnum.avgEnemyRateInBattle_LF:
                    factorInput = prevHeuristicComps.favgEnemyRateInBattle[(int)GameManager.EnemyIndexEnum.LongFlyEnemy];
                    break;
                case InputEnum.avgSpawnerPeriod:
                    factorInput = prevHeuristicComps.favgSpawnerPeriod;
                    break;
                case InputEnum.avgDistBetweenWeapons:
                    factorInput = prevHeuristicComps.favgDistBetweenWeapons;
                    break;
                case InputEnum.weaponNum:
                    factorInput = (float)prevHeuristicComps.iweaponNum;
                    break;
                case InputEnum.propOfWeapons_Pistol:
                    factorInput = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Pistol];
                    break;
                case InputEnum.propOfWeapons_Rifle:
                    factorInput = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Rifle];
                    break;
                case InputEnum.propOfWeapons_Shotgun:
                    factorInput = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Shotgun];
                    break;
                case InputEnum.propOfWeapons_Machinegun:
                    factorInput = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Machinegun];
                    break;
                case InputEnum.propOfWeapons_Sniper:
                    factorInput = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Sniper];
                    break;
                case InputEnum.propOfWeapons_Crowbar:
                    factorInput = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Crowbar];
                    break;
                case InputEnum.propOfWeapons_Lightsaber:
                    factorInput = (float)prevHeuristicComps.fpropOfWeapons[(int)GameManager.WeaponIndexEnum.Lightsaber];
                    break;
                case InputEnum.avgDistBetweenBuffs:
                    factorInput = prevHeuristicComps.favgDistBetweenBuffs;
                    break;
                case InputEnum.buffNum:
                    factorInput = (float)prevHeuristicComps.ibuffNum;
                    break;
                case InputEnum.avgDistBetweenHeals:
                    factorInput = prevHeuristicComps.favgDistBetweenHeals;
                    break;
                case InputEnum.healNum:
                    factorInput = (float)prevHeuristicComps.iHealNum;
                    break;
                case InputEnum.propOfHeals_FirstAidKit:
                    factorInput = (float)prevHeuristicComps.fpropOfHeals[(int)GameManager.HealIndexEnum.FirstAidKit];
                    break;
                case InputEnum.propOfHeals_NanoCure:
                    factorInput = (float)prevHeuristicComps.fpropOfHeals[(int)GameManager.HealIndexEnum.NanoCure];
                    break;
                case InputEnum.propOfHeals_PoisionCure:
                    factorInput = (float)prevHeuristicComps.fpropOfHeals[(int)GameManager.HealIndexEnum.PoisonCure];
                    break;
                case InputEnum.avgRoomSize_x:
                    factorInput = (float)prevHeuristicComps.avgRoomSize.x;
                    break;
                case InputEnum.avgRoomSize_y:
                    factorInput = (float)prevHeuristicComps.avgRoomSize.y;
                    break;
                case InputEnum.avgRoomSize_z:
                    factorInput = (float)prevHeuristicComps.avgRoomSize.z;
                    break;
                case InputEnum.numOfExit:
                    factorInput = (float)prevHeuristicComps.inumOfExit;
                    break;
                default: // None
                    break;
            }
            // 계수 입력이 None
            if (factorInput <= -1000000f) continue;

            float outputRef = -1000000f;
            switch (rules[i].output)
            {
                case OutputEnum.numOfTargetItemOccurence:
                    outputRef = (float)pred.inumOfTargetItemOccurence;
                    break;
                case OutputEnum.propTargetAreaOccurrence:
                    outputRef = pred.fpropTargetAreaOccurence;
                    break;
                case OutputEnum.minBattleSize:
                    outputRef = (float)pred.iminBattleSize;
                    break;
                case OutputEnum.maxBattleSize:
                    outputRef = (float)pred.imaxBattleSize;
                    break;
                case OutputEnum.minSpawnerNum:
                    outputRef = (float)pred.iminSpawnerNum;
                    break;
                case OutputEnum.maxSpawnerNum:
                    outputRef = (float)pred.imaxSpawnerNum;
                    break;
                case OutputEnum.minGenEnemyNum:
                    outputRef = (float)pred.iminGenEnemyNum;
                    break;
                case OutputEnum.maxGenEnemyNum:
                    outputRef = (float)pred.imaxGenEnemyNum;
                    break;
                case OutputEnum.EnemyGenProb_SS:
                    outputRef = pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.ShortSmallEnemy];
                    break;
                case OutputEnum.EnemyGenProb_SB:
                    outputRef = pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.ShortBigEnemy];
                    break;
                case OutputEnum.EnemyGenProb_LG:
                    outputRef = pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.LongGroundEnemy];
                    break;
                case OutputEnum.EnemyGenProb_LF:
                    outputRef = pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.LongFlyEnemy];
                    break;
                case OutputEnum.minSpawnPeriod:
                    outputRef = (float)pred.iminSpawnPeriod;
                    break;
                case OutputEnum.maxSpawnPeriod:
                    outputRef = (float)pred.imaxSpawnPeriod;
                    break;
                case OutputEnum.avgDistBetweenWeapons:
                    outputRef = pred.favgDistBetweenWeapons;
                    break;
                case OutputEnum.minWeaponNum:
                    outputRef = (float)pred.iminWeaponNum;
                    break;
                case OutputEnum.maxWeaponNum:
                    outputRef = (float)pred.imaxWeaponNum;
                    break;
                case OutputEnum.propOfWeapons_Pistol:
                    outputRef = pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Pistol];
                    break;
                case OutputEnum.propOfWeapons_Rifle:
                    outputRef = pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Rifle];
                    break;
                case OutputEnum.propOfWeapons_Shotgun:
                    outputRef = pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Shotgun];
                    break;
                case OutputEnum.propOfWeapons_Machinegun:
                    outputRef = pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Machinegun];
                    break;
                case OutputEnum.propOfWeapons_Sniper:
                    outputRef = pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Sniper];
                    break;
                case OutputEnum.propOfWeapons_Crowbar:
                    outputRef = pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Crowbar];
                    break;
                case OutputEnum.propOfWeapons_Lightsaber:
                    outputRef = pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Lightsaber];
                    break;
                case OutputEnum.avgDistBetweenBuff:
                    outputRef = pred.favgDistBetweenBuff;
                    break;
                case OutputEnum.minBuffNum:
                    outputRef = (float)pred.iminBuffNum;
                    break;
                case OutputEnum.maxBuffNum:
                    outputRef = (float)pred.imaxBuffNum;
                    break;
                case OutputEnum.avgDistBetweenHeals:
                    outputRef = pred.favgDistBetweenHeals;
                    break;
                case OutputEnum.minHealNum:
                    outputRef = (float)pred.iminHealNum;
                    break;
                case OutputEnum.maxHealNum:
                    outputRef = (float)pred.imaxHealNum;
                    break;
                case OutputEnum.propOfHeals_FirstAidKit:
                    outputRef= pred.propOfHeal[(int)GameManager.HealIndexEnum.FirstAidKit];
                    break;
                case OutputEnum.propOfHeals_NanoCure:
                    outputRef = pred.propOfHeal[(int)GameManager.HealIndexEnum.NanoCure];
                    break;
                case OutputEnum.propOfHeals_PoisionCure:
                    outputRef = pred.propOfHeal[(int)GameManager.HealIndexEnum.PoisonCure];
                    break;
                case OutputEnum.minRoomSize_x:
                    outputRef = pred.minRoomSize.x;
                    break;
                case OutputEnum.minRoomSize_y:
                    outputRef = pred.minRoomSize.y;
                    break;
                case OutputEnum.minRoomSize_z:
                    outputRef = pred.minRoomSize.z;
                    break;
                case OutputEnum.maxRoomSize_x:
                    outputRef = pred.maxRoomSize.x;
                    break;
                case OutputEnum.maxRoomSize_y:
                    outputRef = pred.maxRoomSize.y;
                    break;
                case OutputEnum.maxRoomSize_z:
                    outputRef = pred.maxRoomSize.z;
                    break;
                case OutputEnum.numOfExit:
                    outputRef = (float)pred.inumOfEntrance;
                    break;
                default:
                    break;
            }

            // None
            if ((float)outputRef <= -1000000f)
                continue;

            factorInput *= rules[i].ffactor;
            outputRef = normalize_MM(outputRef, factorInput);

            switch (rules[i].output)
            {
                case OutputEnum.numOfTargetItemOccurence:
                    pred.inumOfTargetItemOccurence = (int)outputRef;
                    break;
                case OutputEnum.propTargetAreaOccurrence:
                    pred.fpropTargetAreaOccurence = outputRef;
                    break;
                case OutputEnum.minBattleSize:
                    pred.iminBattleSize = (int)outputRef;
                    break;
                case OutputEnum.maxBattleSize:
                    pred.imaxBattleSize = (int)outputRef;
                    break;
                case OutputEnum.minSpawnerNum:
                    pred.iminSpawnerNum = (int)outputRef;
                    break;
                case OutputEnum.maxSpawnerNum:
                    outputRef = (float)pred.imaxSpawnerNum;
                    break;
                case OutputEnum.minGenEnemyNum:
                    pred.iminGenEnemyNum = (int)outputRef;
                    break;
                case OutputEnum.maxGenEnemyNum:
                    pred.imaxGenEnemyNum = (int)outputRef;
                    break;
                case OutputEnum.EnemyGenProb_SS:
                    pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.ShortSmallEnemy] = outputRef;
                    break;
                case OutputEnum.EnemyGenProb_SB:
                    pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.ShortBigEnemy] = outputRef;
                    break;
                case OutputEnum.EnemyGenProb_LG:
                    pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.LongGroundEnemy] = outputRef;
                    break;
                case OutputEnum.EnemyGenProb_LF:
                    pred.enemyGenProb[(int)GameManager.EnemyIndexEnum.LongFlyEnemy] = outputRef;
                    break;
                case OutputEnum.minSpawnPeriod:
                    pred.iminSpawnPeriod = (int)outputRef;
                    break;
                case OutputEnum.maxSpawnPeriod:
                    pred.imaxSpawnPeriod = (int)outputRef;
                    break;
                case OutputEnum.avgDistBetweenWeapons:
                    pred.favgDistBetweenWeapons = outputRef;
                    break;
                case OutputEnum.minWeaponNum:
                    pred.iminWeaponNum = (int)outputRef;
                    break;
                case OutputEnum.maxWeaponNum:
                    pred.imaxWeaponNum = (int)outputRef;
                    break;
                case OutputEnum.propOfWeapons_Pistol:
                    pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Pistol] = outputRef;
                    break;
                case OutputEnum.propOfWeapons_Rifle:
                    pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Rifle] = outputRef;
                    break;
                case OutputEnum.propOfWeapons_Shotgun:
                    pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Shotgun] = outputRef;
                    break;
                case OutputEnum.propOfWeapons_Machinegun:
                    pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Machinegun] = outputRef;
                    break;
                case OutputEnum.propOfWeapons_Sniper:
                    pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Sniper] = outputRef;
                    break;
                case OutputEnum.propOfWeapons_Crowbar:
                    pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Crowbar] = outputRef;
                    break;
                case OutputEnum.propOfWeapons_Lightsaber:
                    pred.propOfWeapons[(int)GameManager.WeaponIndexEnum.Lightsaber] = outputRef;
                    break;
                case OutputEnum.avgDistBetweenBuff:
                    pred.favgDistBetweenBuff = outputRef;
                    break;
                case OutputEnum.minBuffNum:
                    pred.iminBuffNum = (int)outputRef;
                    break;
                case OutputEnum.maxBuffNum:
                    pred.imaxBuffNum = (int)outputRef;
                    break;
                case OutputEnum.avgDistBetweenHeals:
                    pred.favgDistBetweenHeals = outputRef;
                    break;
                case OutputEnum.minHealNum:
                    pred.iminHealNum = (int)outputRef;
                    break;
                case OutputEnum.maxHealNum:
                    pred.imaxHealNum = (int)outputRef;
                    break;
                case OutputEnum.propOfHeals_FirstAidKit:
                    pred.propOfHeal[(int)GameManager.HealIndexEnum.FirstAidKit] = outputRef;
                    break;
                case OutputEnum.propOfHeals_NanoCure:
                    pred.propOfHeal[(int)GameManager.HealIndexEnum.NanoCure] = outputRef;
                    break;
                case OutputEnum.propOfHeals_PoisionCure:
                    pred.propOfHeal[(int)GameManager.HealIndexEnum.PoisonCure] = outputRef;
                    break;
                case OutputEnum.minRoomSize_x:
                    pred.minRoomSize.x = outputRef;
                    break;
                case OutputEnum.minRoomSize_y:
                    pred.minRoomSize.y = outputRef;
                    break;
                case OutputEnum.minRoomSize_z:
                    pred.minRoomSize.z = outputRef;
                    break;
                case OutputEnum.maxRoomSize_x:
                    pred.maxRoomSize.x = outputRef;
                    break;
                case OutputEnum.maxRoomSize_y:
                    pred.maxRoomSize.y = outputRef;
                    break;
                case OutputEnum.maxRoomSize_z:
                    pred.maxRoomSize.z = outputRef;
                    break;
                case OutputEnum.numOfExit:
                    pred.inumOfEntrance = (int)outputRef;
                    break;
                default:
                    break;
            }
        }
    }

    /* Lifecycles */
    private void Awake()
    {
        /* Singletom Set */
        /* DontDestory 옵션은 설정하지 않음 */
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

}
