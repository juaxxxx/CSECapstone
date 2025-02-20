using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeuristicCompsOfSection
{
    /* Class */
    public class Battle
    {
        public int iroomIndex;
        public int ispawnerNum;
        public int[] enemyNum = new int[4];
        public float favgGenPeriodic;
    }

    public List<Battle> battles = new List<Battle>();
    public float favgDistBetweenSpawners;
    public float favgBattleSize;
    public int ispawnerNum;
    public int iavgGenEnemyNum;
    public float[] favgEnemyRateInBattle = new float[4];
    public float favgSpawnerPeriod;

    public float favgDistBetweenWeapons;
    public int iweaponNum;
    public float[] fpropOfWeapons = new float[7];
    public float favgDistBetweenBuffs;
    public int ibuffNum;
    public float favgDistBetweenHeals;
    public int iHealNum;
    public float[] fpropOfHeals = new float[3];

    public List<MapGenerator.Coord> weapons_coord = new List<MapGenerator.Coord>();
    public List<MapGenerator.Coord> buffs_coord = new List<MapGenerator.Coord>();
    public List<MapGenerator.Coord> heals_coord = new List<MapGenerator.Coord>();

    public Vector3 avgRoomSize = new Vector3();
    public int inumOfExit;

    /* Functions */
    public void SetValues(List<Room> rooms)
    {
        avgRoomSize = Vector3.zero;
        int room_idx = 0;
        int totalSpwanerCnt = 0;
        float fdistSpawner = 0f;
        iavgGenEnemyNum = 0;
        foreach (Room room in rooms)
        {
            if (room == null) continue;
            if (room.roomSizeMax.x < 0)
            {
                continue;
            }
            avgRoomSize.x += (room.roomSizeMax.x  -  room.roomSizeMin.x) + 1;
            avgRoomSize.z += (room.roomSizeMax.z - room.roomSizeMin.z) + 1;
            /*
             * Battle
             * favgBattleSize
             */
            if (room.spawners.Count > 0)
            {
                bool[] flag = new bool[room.spawners.Count];
                for (int i = 0; i < room.spawners.Count; i++) flag[i] = false;
                int[] degs = new int[room.spawners.Count];
                for (int i = 0; i < room.spawners.Count; i++) degs[i] = 0;
                for(int i = 0; i < room.spawners.Count; i++)
                {
                    for(int j = i + 1; j < room.spawners.Count; j++)
                    {
                        if (Vector3.Distance(room.spawners[i].transform.position,
                                room.spawners[j].transform.position) < room.spawners[i].fgenRadius)
                        {
                            degs[i] += 1;
                            degs[j] += 1;
                        }
                    }
                }

                List<int> degSortedIdx = new List<int>();
                for (int i = 0; i < room.spawners.Count; i++)
                    degSortedIdx.Add(i);
                degSortedIdx.Sort();
                for(int i = 0; i < degSortedIdx.Count; i++)
                {
                    int pivotIndex = degSortedIdx[i];
                    if (flag[pivotIndex])
                        continue;
                    flag[pivotIndex] = true;

                    Battle battle = new Battle();
                    battle.iroomIndex = room_idx;
                    battle.ispawnerNum = 1;
                    for (int e = 0; e < 4; e++) battle.enemyNum[e] = 0;
                    battle.enemyNum[(int)room.spawners[i].genEnemyIdx - 1] += room.spawners[i].igenEnemyNum;
                    float totalPeriod = room.spawners[i].fperiod;
                    for (int j = i + 1; j < room.spawners.Count; j++)
                    {
                        if (flag[j]) continue;
                        if (Vector3.Distance(room.spawners[pivotIndex].transform.position,
                                room.spawners[j].transform.position) < room.spawners[pivotIndex].fgenRadius)
                        {
                            flag[j] = true;
                            battle.ispawnerNum++;
                            battle.enemyNum[(int)room.spawners[j].genEnemyIdx - 1] += room.spawners[j].igenEnemyNum;
                            totalPeriod += room.spawners[j].fperiod;
                        }
                    }
                    battle.favgGenPeriodic = totalPeriod / battle.ispawnerNum;
                    favgBattleSize += battle.ispawnerNum;
                    battles.Add(battle);
                }
            }
            
            /*
             * favgDistBetweenSpawners
             * avgGenEnemyNum
             */
            for (int i = 0; i < room.spawners.Count; i++)
            {
                iavgGenEnemyNum += room.spawners[i].igenEnemyNum;
                for(int j = 0; j < room.spawners.Count; j++)
                {
                    fdistSpawner += Vector3.Distance(room.spawners[i].transform.position,
                        room.spawners[j].transform.position);
                }
            }
            totalSpwanerCnt += room.spawners.Count;
            
            room_idx++;
        }
        ispawnerNum = (int)favgBattleSize;
        if (battles.Count > 0) favgBattleSize /= battles.Count;
        else favgBattleSize = 4;

        /*
         * favgEnemyRateInBattle
         * favgSpawnerPeriod
         */
        for (int e = 0; e < 4; e++) favgEnemyRateInBattle[e] = 0f;
        favgSpawnerPeriod = 0;
        for (int i = 0; i < battles.Count; i++)
        {
            float total = 0;
            for (int e = 0; e < 4; e++) total += battles[i].enemyNum[e];
            for (int e = 0; e < 4; e++)
            {
                if (total > 0) favgEnemyRateInBattle[e] += (float)battles[i].enemyNum[e] / total;
            }
            favgSpawnerPeriod += battles[i].favgGenPeriodic;
        }
        if (battles.Count > 0)
        {
            for (int e = 0; e < 4; e++)
                favgEnemyRateInBattle[e] /= battles.Count;
            favgSpawnerPeriod /= battles.Count;
        }
        else
        {
            for (int e = 0; e < 4; e++)
                favgEnemyRateInBattle[e] = 0.25f;
            favgSpawnerPeriod = 7;
        }
        if (totalSpwanerCnt > 0)
        {
            favgDistBetweenSpawners = fdistSpawner / (totalSpwanerCnt * totalSpwanerCnt / 2);
            iavgGenEnemyNum = iavgGenEnemyNum / totalSpwanerCnt;
        }
        else
        {
            favgDistBetweenSpawners = 4f;
            iavgGenEnemyNum = 3;
        }

        for (int i = 0; i < 7; i++)
        {
            if (iweaponNum > 0)
                fpropOfWeapons[i] /= iweaponNum;
            else fpropOfWeapons[i] = (float)(1f / 6f);
        }
        for (int i = 0; i < 3; i++)
        {
            if (iHealNum > 0)
                fpropOfHeals[i] /= iHealNum;
            else fpropOfHeals[i] = (float)(1f / 3);

        }

        favgDistBetweenWeapons = 0;
        for (int i = 0; i < weapons_coord.Count; i++)
        {
            MapGenerator.Coord a = weapons_coord[i];
            for(int j = 0; j < weapons_coord.Count; j++)
            {
                if (i == j) continue;
                MapGenerator.Coord b = weapons_coord[i];
                favgDistBetweenWeapons += Vector3.Distance(new Vector3(a.tileX, 0, a.tileY), new Vector3(b.tileX, 0, b.tileY));
            }
        }
        if (iweaponNum > 0)
            favgDistBetweenWeapons /= (iweaponNum * iweaponNum / 2);
        else favgDistBetweenWeapons = 10f;

        favgDistBetweenBuffs = 0;
        for (int i = 0; i < buffs_coord.Count; i++)
        {
            MapGenerator.Coord a = buffs_coord[i];
            for (int j = 0; j < buffs_coord.Count; j++)
            {
                if (i == j) continue;
                MapGenerator.Coord b = buffs_coord[i];
                favgDistBetweenBuffs += Vector3.Distance(new Vector3(a.tileX, 0, a.tileY), new Vector3(b.tileX, 0, b.tileY));
            }
        }
        if (ibuffNum > 0)
            favgDistBetweenBuffs /= (ibuffNum * ibuffNum / 2);
        else favgDistBetweenBuffs = 10f;


        favgDistBetweenHeals = 0;
        for (int i = 0; i < heals_coord.Count; i++)
        {
            MapGenerator.Coord a = heals_coord[i];
            for (int j = 0; j < heals_coord.Count; j++)
            {
                if (i == j) continue;
                MapGenerator.Coord b = heals_coord[i];
                favgDistBetweenHeals += Vector3.Distance(new Vector3(a.tileX, 0, a.tileY), new Vector3(b.tileX, 0, b.tileY));
            }
        }
        if (iHealNum > 0)
            favgDistBetweenHeals /= (iHealNum * iHealNum / 2);
        else favgDistBetweenHeals = 10f;

        if(rooms.Count > 0)
        {
            avgRoomSize.x /= rooms.Count;
            avgRoomSize.z /= rooms.Count;
        }
        else
        {
            avgRoomSize = new Vector3(100, 0, 100);
        }
    }
}
