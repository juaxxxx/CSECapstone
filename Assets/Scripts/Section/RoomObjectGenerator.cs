using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Xml;
using UnityEditor.Timeline;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class RoomObjectGenerator : MonoBehaviour
{
    /* Prefabs */
    public GameObject entrance_prefab, spawner_prefab, targetItem_prefab, targetArea_prefab;
    public List<GameObject> items = new List<GameObject>();

    /* Variables */
    public MapGenerator mapGenerator;
    public List<Room> rooms = new List<Room>();

    public float squareSize = 2.5f;

    /* Functions */
    // entrance 배치가 가능한 위치
    List<MapGenerator.Coord> upSide_edge_positions = new List<MapGenerator.Coord>();
    List<MapGenerator.Coord> downSide_edge_positions = new List<MapGenerator.Coord>();
    List<MapGenerator.Coord> leftSide_edge_positions = new List<MapGenerator.Coord>();
    List<MapGenerator.Coord> rightSide_edge_positions = new List<MapGenerator.Coord>();
    // targetItem 생성 수 누적
    int targetItemNum = 0;
    // targetArea 생성 확률 누적
    float unitProb, stackProb;
    List<MapGenerator.Coord> spawners = new List<MapGenerator.Coord>();
    List<MapGenerator.Coord> weapons = new List<MapGenerator.Coord>();
    List<MapGenerator.Coord> buffs = new List<MapGenerator.Coord>();
    List<MapGenerator.Coord> heals = new List<MapGenerator.Coord>();
    List<MapGenerator.Coord> targetItems = new List<MapGenerator.Coord>();
    MapGenerator.Coord targetArea;
    public void GenerateRoom(int[,] map, List<MapGenerator.MapRoom> survivingRooms, List<int[]> eachRoomSize, int[] edgeRoomIdx)
    {
        mapGenerator = GetComponent<MapGenerator>();
        squareSize = mapGenerator.squareSize / 2;
        // targetArea 생성 확률 누적
        unitProb = mapGenerator.heuristicPred.fpropTargetAreaOccurence / survivingRooms.Count;
        stackProb = unitProb;
        // entrance
        bool[] enter_flag = new bool[4];
        for (int f = 0; f < 4; f++) enter_flag[f] = false;

        for (int i = 0; i < survivingRooms.Count; i++)
        {
            Room room = new Room();
            // 상하좌우 edge인지
            bool[] edgeIdx = new bool[4];   // j = {0, 1, 2, 3 : 상, 하, 좌, 우}
            for (int j = 0; j < 4; j++)
            {
                if (edgeRoomIdx[j] == i) edgeIdx[j] = true;
                else edgeIdx[j] = false;
            }

            // local map 사이즈
            int width = (eachRoomSize[i][1] - eachRoomSize[i][0] + 1) * 2;
            int height = (eachRoomSize[i][3] - eachRoomSize[i][2] + 1) * 2;

            // local map 생성
            int[,] localMap = new int[width, height];
            List<MapGenerator.Coord> normalTile = new List<MapGenerator.Coord>();
            for(int row = eachRoomSize[i][0]; row < eachRoomSize[i][1] + 1; row++)
            {
                for(int col = eachRoomSize[i][2]; col < eachRoomSize[i][3] + 1; col++)
                {
                    int localRow = row - eachRoomSize[i][0];
                    int localCol = col - eachRoomSize[i][2];
                    if (map[row, col] == MapGenerator.StateIndexEnum.Terrain)
                    {
                        localMap[localRow * 2, localCol * 2] = MapGenerator.StateIndexEnum.Terrain;
                        localMap[localRow * 2 + 1, localCol * 2] = MapGenerator.StateIndexEnum.Terrain;
                        localMap[localRow * 2, localCol * 2 + 1] = MapGenerator.StateIndexEnum.Terrain;
                        localMap[localRow * 2 + 1, localCol * 2 + 1] = MapGenerator.StateIndexEnum.Terrain;
                    }
                    else
                    {
                        // map[row, col] == MapGenerator.StateIndexEnum.Normal
                        localMap[localRow * 2, localCol * 2] = MapGenerator.StateIndexEnum.Normal;
                        localMap[localRow * 2 + 1, localCol * 2] = MapGenerator.StateIndexEnum.Normal;
                        localMap[localRow * 2, localCol * 2 + 1] = MapGenerator.StateIndexEnum.Normal;
                        localMap[localRow * 2 + 1, localCol * 2 + 1] = MapGenerator.StateIndexEnum.Normal;
                        normalTile.Add(new MapGenerator.Coord(localRow * 2, localCol * 2));
                        normalTile.Add(new MapGenerator.Coord(localRow * 2 + 1, localCol * 2));
                        normalTile.Add(new MapGenerator.Coord(localRow * 2, localCol * 2 + 1));
                        normalTile.Add(new MapGenerator.Coord(localRow * 2 + 1, localCol * 2 + 1));
                    }
                }
            }
            // 오브젝트 배치 금지 바운더리
            foreach(MapGenerator.Coord edgeTile in survivingRooms[i].edgeTiles)
            {
                int row = edgeTile.tileX, col = edgeTile.tileY;
                int localRow = row - eachRoomSize[i][0];
                int localCol = col - eachRoomSize[i][2];
                localMap[localRow * 2, localCol * 2] = MapGenerator.StateIndexEnum.Die;
                localMap[localRow * 2 + 1, localCol * 2] = MapGenerator.StateIndexEnum.Die;
                localMap[localRow * 2, localCol * 2 + 1] = MapGenerator.StateIndexEnum.Die;
                localMap[localRow * 2 + 1, localCol * 2 + 1] = MapGenerator.StateIndexEnum.Die;
                
                // Entrance 배치 가능한 위치
                if(edgeIdx[0])    // 상
                {
                    if (map[row - 1, col] == MapGenerator.StateIndexEnum.Terrain)
                    {
                        upSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2, localCol * 2));
                        upSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2, localCol * 2 + 1));
                    }
                }
                if (edgeIdx[1])   // 하
                {
                    if (map[row + 1, col] == MapGenerator.StateIndexEnum.Terrain)
                    {
                        downSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2 + 1, localCol));
                        downSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2 + 1, localCol * 2 + 1));
                    }
                }
                if (edgeIdx[2])   // 좌
                {
                    if (map[row, col - 1] == MapGenerator.StateIndexEnum.Terrain)
                    {
                        leftSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2, localCol * 2));
                        leftSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2 + 1, localCol * 2));
                    }
                }
                if (edgeIdx[3])   // 우
                {
                    if (map[row, col + 1] == MapGenerator.StateIndexEnum.Terrain)
                    {
                        rightSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2, localCol * 2 + 1));
                        rightSide_edge_positions.Add(new MapGenerator.Coord(localRow * 2 + 1, localCol * 2 + 1));
                    }
                }
            }

            /* (구) RandomFillRoom */
            // Spawner 배치
            int spawnerNum = Random.Range(mapGenerator.heuristicPred.iminSpawnerNum, mapGenerator.heuristicPred.imaxSpawnerNum + 1);
            spawnerNum = (int)(spawnerNum / survivingRooms.Count);
            for(int n = 0; n < spawnerNum; n++)
            {
                int randIndex = Random.Range(0, normalTile.Count);
                if (localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] == MapGenerator.StateIndexEnum.Normal)
                {
                    localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] = MapGenerator.StateIndexEnum.Spawner;
                    normalTile.Remove(normalTile[randIndex]);
                    spawners.Add(normalTile[randIndex]);
                }
                else
                {
                    n--;
                    normalTile.Remove(normalTile[randIndex]);
                }
                if (normalTile.Count < 1) break;
            }

            // Weapon 배치
            int weaponNum = Random.Range(mapGenerator.heuristicPred.iminWeaponNum, mapGenerator.heuristicPred.imaxWeaponNum + 1);
            weaponNum = (int)(weaponNum / survivingRooms.Count);
            for (int n = 0; n < weaponNum; n++)
            {
                int randIndex = Random.Range(0, normalTile.Count);
                if (localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] == MapGenerator.StateIndexEnum.Normal)
                {
                    localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] = MapGenerator.StateIndexEnum.Weapon;
                    normalTile.Remove(normalTile[randIndex]);
                    weapons.Add(normalTile[randIndex]);
                }
                else
                {
                    n--;
                    normalTile.Remove(normalTile[randIndex]);
                }
                if (normalTile.Count < 1) break;
            }

            // Buff 배치
            int buffNum = Random.Range(mapGenerator.heuristicPred.iminBuffNum, mapGenerator.heuristicPred.imaxBuffNum + 1);
            buffNum = (int)(buffNum / survivingRooms.Count);
            for (int n = 0; n < buffNum; n++)
            {
                int randIndex = Random.Range(0, normalTile.Count);
                if (localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] == MapGenerator.StateIndexEnum.Normal)
                {
                    localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] = MapGenerator.StateIndexEnum.Buff;
                    normalTile.Remove(normalTile[randIndex]);
                    buffs.Add(normalTile[randIndex]);
                }
                else
                {
                    n--;
                    normalTile.Remove(normalTile[randIndex]);
                }
                if (normalTile.Count < 1) break;
            }

            // Heal 배치
            int healNum = Random.Range(mapGenerator.heuristicPred.iminHealNum, mapGenerator.heuristicPred.imaxHealNum + 1);
            healNum = (int)(healNum / survivingRooms.Count);
            for (int n = 0; n < healNum; n++)
            {
                int randIndex = Random.Range(0, normalTile.Count);
                if (localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] == MapGenerator.StateIndexEnum.Normal)
                {
                    localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] = MapGenerator.StateIndexEnum.Heal;
                    normalTile.Remove(normalTile[randIndex]);
                    heals.Add(normalTile[randIndex]);
                }
                else
                {
                    n--;
                    normalTile.Remove(normalTile[randIndex]);
                }
                if (normalTile.Count < 1) break;
            }

            // TargetItem 배치
            int numOfTargetItem = Random.Range(mapGenerator.heuristicPred.inumOfTargetItemOccurence / survivingRooms.Count,
                mapGenerator.heuristicPred.inumOfTargetItemOccurence);
            if (numOfTargetItem + targetItemNum > mapGenerator.heuristicPred.inumOfTargetItemOccurence)
                numOfTargetItem = mapGenerator.heuristicPred.inumOfTargetItemOccurence - targetItemNum;
            targetItemNum += numOfTargetItem;
            for (int n = 0; n < numOfTargetItem; n++)
            {
                int randIndex = Random.Range(0, normalTile.Count);
                if (localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] == MapGenerator.StateIndexEnum.Normal)
                {
                    localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] = MapGenerator.StateIndexEnum.TargetItem;
                    normalTile.Remove(normalTile[randIndex]);
                    targetItems.Add(normalTile[randIndex]);
                }
                else
                {
                    n--;
                    normalTile.Remove(normalTile[randIndex]);
                }
                if (normalTile.Count < 1) break;
            }

            // TargetArea 배치
            if(stackProb > 0f)
            {
                int raw = Random.Range(0, 100);
                if (raw < (int)stackProb * 100)
                {
                    while (true)
                    {
                        int randIndex = Random.Range(0, normalTile.Count);
                        if (localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] == MapGenerator.StateIndexEnum.Normal)
                        {
                            localMap[normalTile[randIndex].tileX, normalTile[randIndex].tileY] = MapGenerator.StateIndexEnum.TargetItem;
                            normalTile.Remove(normalTile[randIndex]);
                            targetArea = normalTile[randIndex];
                            break;
                        }
                        else
                        {
                            normalTile.Remove(normalTile[randIndex]);
                        }
                        if (normalTile.Count < 1) break;
                    }
                    stackProb = 0f;
                }
                else stackProb += unitProb;
            }
            
            /* Smooth Map */
            for (int smoothIter = 0; smoothIter < 5; smoothIter++)
                SmoothRoom(localMap, width, height);

            /* Instantiate */
            /* Enterance 배치 */
            int sect_pos_x = mapGenerator.thisSection.sect_position[0];
            int sect_pos_y = mapGenerator.thisSection.sect_position[1];
            // 이미 이전 섹션이 있는 경우 배치하고, 임의 배치
            // 상
            if(sect_pos_x > 0 && edgeIdx[0] && upSide_edge_positions.Count > 0)
            {
                int check_sect_x = sect_pos_x - 1;
                Section check_sect = StageManager.Instance.sections[(int)StageManager.Instance.ParseXyNIndex(check_sect_x, sect_pos_y)];
                if(check_sect != null)
                {
                    if (check_sect.entracnes[1] != null)
                    {
                        int randIdx = Random.Range(0, upSide_edge_positions.Count);
                        int enter_x = upSide_edge_positions[randIdx].tileX, enter_y = upSide_edge_positions[randIdx].tileY;
                        localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                        enter_flag[0] = true;

                        // Instantiate Entrance
                        GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                        genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        Vector3 newPos = genEnter.transform.position;
                        newPos.z += 5f;
                        genEnter.transform.position = newPos;
                        Entrance enter = genEnter.GetComponent<Entrance>();
                        check_sect.entracnes[1].linkEntrance(enter);
                        check_sect.entracnes[1].linked_section_pos[0] = mapGenerator.thisSection.sect_position[0];
                        check_sect.entracnes[1].linked_section_pos[1] = mapGenerator.thisSection.sect_position[1];
                        enter.linked_section_pos[0] = check_sect.sect_position[0];
                        enter.linked_section_pos[1] = check_sect.sect_position[1];
                        enter.setWallDir(new Vector3(0, 0, -1));

                        genEnter.SetActive(true);

                        // Section Info Update
                        mapGenerator.thisSection.entracnes[0] = enter;

                        // Room Info Update
                        room.linkedSectionIndexOfEntrance = 0;
                    }
                }
                else if(mapGenerator.heuristicPred.inumOfEntrance > 3)
                {
                    int randIdx = Random.Range(0, upSide_edge_positions.Count);
                    int enter_x = upSide_edge_positions[randIdx].tileX, enter_y = upSide_edge_positions[randIdx].tileY;
                    localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                    enter_flag[0] = true;
                    // Instantiate Entrance
                    GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                    genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                        mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                    Vector3 newPos = genEnter.transform.position;
                    newPos.z += 5f;
                    genEnter.transform.position = newPos;
                    Entrance enter = genEnter.GetComponent<Entrance>();
                    enter.linked_section_pos[0] = check_sect_x;
                    enter.linked_section_pos[1] = sect_pos_y;
                    enter.setWallDir(new Vector3(0, 0, -1));
                    genEnter.SetActive(true);

                    // Section Info Update
                    mapGenerator.thisSection.entracnes[0] = enter;

                    // Room Info Update
                    room.linkedSectionIndexOfEntrance = 1;
                }
            }
            // 하
            if (sect_pos_x < 4 && edgeIdx[1] && downSide_edge_positions.Count > 0)
            {
                int check_sect_x = sect_pos_x + 1;
                Section check_sect = StageManager.Instance.sections[(int)StageManager.Instance.ParseXyNIndex(check_sect_x, sect_pos_y)];
                if (check_sect != null)
                {
                    if (check_sect.entracnes[0] != null)
                    {
                        int randIdx = Random.Range(0, downSide_edge_positions.Count);
                        int enter_x = downSide_edge_positions[randIdx].tileX, enter_y = downSide_edge_positions[randIdx].tileY;
                        localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                        enter_flag[1] = true;

                        // Instantiate Entrance
                        GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                        genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        Vector3 newPos = genEnter.transform.position;
                        newPos.z -= 5f;
                        genEnter.transform.position = newPos;

                        Entrance enter = genEnter.GetComponent<Entrance>();
                        check_sect.entracnes[0].linkEntrance(enter);
                        check_sect.entracnes[0].linked_section_pos[0] = mapGenerator.thisSection.sect_position[0];
                        check_sect.entracnes[0].linked_section_pos[1] = mapGenerator.thisSection.sect_position[1];
                        enter.linked_section_pos[0] = check_sect.sect_position[0];
                        enter.linked_section_pos[1] = check_sect.sect_position[1];
                        enter.setWallDir(new Vector3(0, 0, 1));

                        genEnter.SetActive(true);

                        // Section Info Update
                        mapGenerator.thisSection.entracnes[1] = enter;

                        // Room Info Update
                        room.linkedSectionIndexOfEntrance = 0;
                    }
                }
                else if (mapGenerator.heuristicPred.inumOfEntrance > 2)
                {
                    int randIdx = Random.Range(0, downSide_edge_positions.Count);
                    int enter_x = downSide_edge_positions[randIdx].tileX, enter_y = downSide_edge_positions[randIdx].tileY;
                    localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                    enter_flag[1] = true;

                    // Instantiate Entrance
                    GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                    genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                        mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                    Vector3 newPos = genEnter.transform.position;
                    newPos.z -= 5f;
                    genEnter.transform.position = newPos;

                    Entrance enter = genEnter.GetComponent<Entrance>();
                    enter.linked_section_pos[0] = check_sect_x;
                    enter.linked_section_pos[1] = sect_pos_y;
                    enter.setWallDir(new Vector3(0, 0, 1));

                    genEnter.SetActive(true);

                    // Section Info Update
                    mapGenerator.thisSection.entracnes[1] = enter;

                    // Room Info Update
                    room.linkedSectionIndexOfEntrance = 1;
                }
            }
            // 좌
            if (sect_pos_y > 0 && edgeIdx[2] && leftSide_edge_positions.Count > 0)
            {
                int check_sect_y = sect_pos_y - 1;
                Section check_sect = StageManager.Instance.sections[(int)StageManager.Instance.ParseXyNIndex(sect_pos_x, check_sect_y)];
                if (check_sect != null)
                {
                    if (check_sect.entracnes[3] != null)
                    {
                        int randIdx = Random.Range(0, leftSide_edge_positions.Count);
                        int enter_x = leftSide_edge_positions[randIdx].tileX, enter_y = leftSide_edge_positions[randIdx].tileY;
                        localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                        enter_flag[2] = true;

                        // Instantiate Entrance
                        GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                        genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        Vector3 newPos = genEnter.transform.position;
                        newPos.x -= 5f;
                        genEnter.transform.position = newPos;

                        Entrance enter = genEnter.GetComponent<Entrance>();
                        check_sect.entracnes[3].linkEntrance(enter);
                        check_sect.entracnes[3].linked_section_pos[0] = mapGenerator.thisSection.sect_position[0];
                        check_sect.entracnes[3].linked_section_pos[1] = mapGenerator.thisSection.sect_position[1];
                        enter.linked_section_pos[0] = check_sect.sect_position[0];
                        enter.linked_section_pos[1] = check_sect.sect_position[1];
                        enter.setWallDir(new Vector3(1, 0, 0));

                        genEnter.SetActive(true);

                        // Section Info Update
                        mapGenerator.thisSection.entracnes[2] = enter;

                        // Room Info Update
                        room.linkedSectionIndexOfEntrance = 0;
                    }
                }
                else if (mapGenerator.heuristicPred.inumOfEntrance > 1)
                {
                    int randIdx = Random.Range(0, leftSide_edge_positions.Count);
                    int enter_x = leftSide_edge_positions[randIdx].tileX, enter_y = leftSide_edge_positions[randIdx].tileY;
                    localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                    enter_flag[2] = true;

                    // Instantiate Entrance
                    GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                    genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                        mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                    Vector3 newPos = genEnter.transform.position;
                    newPos.x -= 5f;
                    genEnter.transform.position = newPos;

                    Entrance enter = genEnter.GetComponent<Entrance>();

                    enter.linked_section_pos[0] = sect_pos_x;
                    enter.linked_section_pos[1] = check_sect_y;
                    enter.setWallDir(new Vector3(1, 0, 0));

                    genEnter.SetActive(true);

                    // Section Info Update
                    mapGenerator.thisSection.entracnes[2] = enter;

                    // Room Info Update
                    room.linkedSectionIndexOfEntrance = 1;
                }
            }
            // 우
            if (sect_pos_y < 4 && edgeIdx[3] && rightSide_edge_positions.Count > 0)
            {
                int check_sect_y = sect_pos_y + 1;
                Section check_sect = StageManager.Instance.sections[(int)StageManager.Instance.ParseXyNIndex(sect_pos_x, check_sect_y)];
                if (check_sect != null)
                {
                    if (check_sect.entracnes[2] != null)
                    {
                        int randIdx = Random.Range(0, rightSide_edge_positions.Count);
                        int enter_x = rightSide_edge_positions[randIdx].tileX, enter_y = rightSide_edge_positions[randIdx].tileY;
                        localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                        enter_flag[3] = true;

                        // Instantiate Entrance
                        GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                        genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        Vector3 newPos = genEnter.transform.position;
                        newPos.x += 5f;
                        genEnter.transform.position = newPos;

                        Entrance enter = genEnter.GetComponent<Entrance>();
                        check_sect.entracnes[2].linkEntrance(enter);
                        check_sect.entracnes[2].linked_section_pos[0] = mapGenerator.thisSection.sect_position[0];
                        check_sect.entracnes[2].linked_section_pos[1] = mapGenerator.thisSection.sect_position[1];
                        enter.linked_section_pos[0] = check_sect.sect_position[0];
                        enter.linked_section_pos[1] = check_sect.sect_position[1];
                        enter.setWallDir(new Vector3(-1, 0, 0));

                        genEnter.SetActive(true);

                        // Section Info Update
                        mapGenerator.thisSection.entracnes[3] = enter;

                        // Room Info Update
                        room.linkedSectionIndexOfEntrance = 0;
                    }
                }
                else if (mapGenerator.heuristicPred.inumOfEntrance > 0)
                {
                    int randIdx = Random.Range(0, rightSide_edge_positions.Count);
                    int enter_x = rightSide_edge_positions[randIdx].tileX, enter_y = rightSide_edge_positions[randIdx].tileY;
                    localMap[enter_x, enter_y] = MapGenerator.StateIndexEnum.Entrance;
                    enter_flag[3] = true;

                    // Instantiate Entrance
                    GameObject genEnter = Instantiate(entrance_prefab, mapGenerator.thisSection.transform);
                    genEnter.transform.position = GetRealPos(enter_x, enter_y, eachRoomSize[i][0], eachRoomSize[i][2],
                        mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                    Vector3 newPos = genEnter.transform.position;
                    newPos.x += 5f;
                    genEnter.transform.position = newPos;

                    Entrance enter = genEnter.GetComponent<Entrance>();

                    enter.linked_section_pos[0] = sect_pos_x;
                    enter.linked_section_pos[1] = check_sect_y;
                    enter.setWallDir(new Vector3(-1, 0, 0));

                    genEnter.SetActive(true);

                    // Section Info Update
                    mapGenerator.thisSection.entracnes[3] = enter;

                    // Room Info Update
                    room.linkedSectionIndexOfEntrance = 1;
                }
            }

            /* Set Objects*/
            SetObjects(localMap, width, height, eachRoomSize[i][0], eachRoomSize[i][2], room);
            // Arrange Room Info
            if (room.roomSizeMin.x > 99) room.roomSizeMin.x = 0;
            if (room.roomSizeMin.z > 99) room.roomSizeMin.z = 0;
            if (room.roomSizeMax.x < 0) room.roomSizeMax.x = 100;
            if (room.roomSizeMax.z < 0) room.roomSizeMax.z = 100;
            room.roomMinCoord = GetRealPos(0, 0, eachRoomSize[i][0], eachRoomSize[i][2],
                        mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
            room.roomMaxCoord = GetRealPos(width - 1, height - 1, eachRoomSize[i][0], eachRoomSize[i][2], 
                mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);

            room.adjacencyRooms = new List<int>(mapGenerator.thisSection.rooms[i].adjacencyRooms);
            mapGenerator.thisSection.rooms[i] = room;

            /* Clear */
            normalTile.Clear();
            spawners.Clear();
            buffs.Clear();
            heals.Clear();
            targetItems.Clear();
            targetArea = new MapGenerator.Coord(-1, -1);
        }
    }
    public void SmoothRoom(int[,] map, int width, int height)
    {
        int dirX = 0, dirY = 0;
        // Spawner 조정
        float spawnerRadius = 3f;
        float spawnerRadiusPow = spawnerRadius * spawnerRadius;
        for(int i = spawners.Count -1 ; i >= 0; i--)
        {
            int x = spawners[i].tileX, y = spawners[i].tileY;
            int countOfNeighbors = 0;
            foreach (MapGenerator.Coord other in spawners)
            {
                int ox = other.tileX, oy = other.tileY;
                if (x == ox && y == oy) break;

                float trueDistPow = (x - ox) * (x - ox) + (y - oy) * (y - oy);

                // other이 있는 방향으로 +1
                if (ox > x) dirX += 1;
                if (ox < x) dirX -= 1;
                if (oy > y) dirY += 1;
                if (oy < y) dirY -= 1;

                // 같은 Battle에 속함
                if (trueDistPow <= spawnerRadiusPow)
                {
                    countOfNeighbors += 1;
                }
            }

            // BattleSize 적절
            if (countOfNeighbors >= mapGenerator.heuristicPred.iminBattleSize &&
                countOfNeighbors <= mapGenerator.heuristicPred.imaxBattleSize)
                continue;
            // 너무 많음
            else if (countOfNeighbors > mapGenerator.heuristicPred.imaxBattleSize)
            {
                dirX *= -1;
                dirY *= -1;
            }
            // 너무 적은 경우에는 상관 없음

            // 더 작은 단위로 움직임
            if (dirX > 0) dirX = 1;
            if (dirX < 0) dirX = -1;
            if (dirY > 0) dirY = 1;
            if (dirY < 0) dirY = -1;

            if (x + dirX < 0 || x + dirX >= width
                || y + dirY < 0 || y + dirY >= height)
                continue;

            if (map[x + dirX, y + dirY] == MapGenerator.StateIndexEnum.Normal)
            {
                map[x, y] = MapGenerator.StateIndexEnum.Normal;
                map[x + dirX, y + dirY] = MapGenerator.StateIndexEnum.Spawner;
                spawners.RemoveAt(i);
                spawners.Add(new MapGenerator.Coord(x + dirX, y + dirY));
            }
        }

        // Weapon 조정
        for(int i = weapons.Count - 1; i >= 0 ; i--)
        {
            int x = weapons[i].tileX, y = weapons[i].tileY;
            float distPow = mapGenerator.heuristicPred.favgDistBetweenWeapons * mapGenerator.heuristicPred.favgDistBetweenWeapons;
            foreach (MapGenerator.Coord other in weapons)
            {
                int ox = other.tileX, oy = other.tileY;

                float trueDistPow = (x - ox) * (x - ox) + (y - oy) * (y - oy);

                if(trueDistPow < distPow)
                {
                    // other이 있는 방향으로 -1
                    if (ox > x) dirX -= 1;
                    if (ox < x) dirX += 1;
                    if (oy > y) dirY -= 1;
                    if (oy < y) dirY += 1;
                }
                else
                {
                    // other이 있는 방향으로 +1
                    if (ox > x) dirX += 1;
                    if (ox < x) dirX -= 1;
                    if (oy > y) dirY += 1;
                    if (oy < y) dirY -= 1;
                }
            }
            // 더 작은 단위로 움직임
            if (dirX > 0) dirX = 1;
            if (dirX < 0) dirX = -1;
            if (dirY > 0) dirY = 1;
            if (dirY < 0) dirY = -1;

            if (x + dirX < 0 || x + dirX >= width
                || y + dirY < 0 || y + dirY >= height)
                continue;
            if (map[x + dirX, y + dirY] == MapGenerator.StateIndexEnum.Normal)
            {
                map[x, y] = MapGenerator.StateIndexEnum.Normal;
                map[x + dirX, y + dirY] = MapGenerator.StateIndexEnum.Weapon;
                weapons.RemoveAt(i);
                weapons.Add(new MapGenerator.Coord(x + dirX, y + dirY));
            }
        }

        // Buff 조정
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            int x = buffs[i].tileX, y = buffs[i].tileY;
            float distPow = mapGenerator.heuristicPred.favgDistBetweenBuff * mapGenerator.heuristicPred.favgDistBetweenBuff;
            foreach (MapGenerator.Coord other in buffs)
            {
                int ox = other.tileX, oy = other.tileY;

                float trueDistPow = (x - ox) * (x - ox) + (y - oy) * (y - oy);

                if (trueDistPow < distPow)
                {
                    // other이 있는 방향으로 -1
                    if (ox > x) dirX -= 1;
                    if (ox < x) dirX += 1;
                    if (oy > y) dirY -= 1;
                    if (oy < y) dirY += 1;
                }
                else
                {
                    // other이 있는 방향으로 +1
                    if (ox > x) dirX += 1;
                    if (ox < x) dirX -= 1;
                    if (oy > y) dirY += 1;
                    if (oy < y) dirY -= 1;
                }
            }
            // 더 작은 단위로 움직임
            if (dirX > 0) dirX = 1;
            if (dirX < 0) dirX = -1;
            if (dirY > 0) dirY = 1;
            if (dirY < 0) dirY = -1;

            if (x + dirX < 0 || x + dirX >= width
                || y + dirY < 0 || y + dirY >= height)
                continue;
            if (map[x + dirX, y + dirY] == MapGenerator.StateIndexEnum.Normal)
            {
                map[x, y] = MapGenerator.StateIndexEnum.Normal;
                map[x + dirX, y + dirY] = MapGenerator.StateIndexEnum.Buff;
                buffs.RemoveAt(i);
                buffs.Add(new MapGenerator.Coord(x + dirX, y + dirY));
            }
        }

        // Heal 조정
        for (int i = heals.Count - 1; i >= 0; i--)
        {
            int x = heals[i].tileX, y = heals[i].tileY;
            float distPow = mapGenerator.heuristicPred.favgDistBetweenHeals * mapGenerator.heuristicPred.favgDistBetweenHeals;
            foreach (MapGenerator.Coord other in heals)
            {
                int ox = other.tileX, oy = other.tileY;

                float trueDistPow = (x - ox) * (x - ox) + (y - oy) * (y - oy);

                if (trueDistPow < distPow)
                {
                    // other이 있는 방향으로 -1
                    if (ox > x) dirX -= 1;
                    if (ox < x) dirX += 1;
                    if (oy > y) dirY -= 1;
                    if (oy < y) dirY += 1;
                }
                else
                {
                    // other이 있는 방향으로 +1
                    if (ox > x) dirX += 1;
                    if (ox < x) dirX -= 1;
                    if (oy > y) dirY += 1;
                    if (oy < y) dirY -= 1;
                }
            }
            // 더 작은 단위로 움직임
            if (dirX > 0) dirX = 1;
            if (dirX < 0) dirX = -1;
            if (dirY > 0) dirY = 1;
            if (dirY < 0) dirY = -1;

            if (x + dirX < 0 || x + dirX >= width
                || y + dirY < 0 || y + dirY >= height)
                continue;
            if (map[x + dirX, y + dirY] == MapGenerator.StateIndexEnum.Normal)
            {
                map[x, y] = MapGenerator.StateIndexEnum.Normal;
                map[x + dirX, y + dirY] = MapGenerator.StateIndexEnum.Heal;
                heals.RemoveAt(i);
                heals.Add(new MapGenerator.Coord(x + dirX, y + dirY));
            }
        }

        // TargetItem 조정
        for(int i = targetItems.Count - 1; i >= 0; i--)
        {
            int x = targetItems[i].tileX, y = targetItems[i].tileY;
            for(int r = x - 1; r <= x + 1; x++)
            {
                for( int c = y - 1; c <= y + 1; c++)
                {
                    if (r < 0 || r >= width || c < 0 || c >= height)
                        continue;
                    if (map[r, c] != MapGenerator.StateIndexEnum.Normal)
                    {
                        // other이 있는 방향으로 -1
                        if (r > x) dirX -= 1;
                        if (r < x) dirX += 1;
                        if (c > y) dirY -= 1;
                        if (c < y) dirY += 1;
                    }
                }
            }
            // 더 작은 단위로 움직임
            if (dirX > 0) dirX = 1;
            if (dirX < 0) dirX = -1;
            if (dirY > 0) dirY = 1;
            if (dirY < 0) dirY = -1;

            if (x + dirX < 0 || x + dirX >= width
                || y + dirY < 0 || y + dirY >= height)
                continue;
            if (map[x + dirX, y + dirY] == MapGenerator.StateIndexEnum.Normal)
            {
                map[x, y] = MapGenerator.StateIndexEnum.Normal;
                map[x + dirX, y + dirY] = MapGenerator.StateIndexEnum.TargetItem;
                targetItems.RemoveAt(i);
                targetItems.Add(new MapGenerator.Coord(x + dirX, y + dirY));
            }
        }

        // TargetArea 조정
        if (targetArea.tileX > -1 && targetArea.tileY > -1)
        {
            int taX = targetArea.tileX, taY = targetArea.tileY;
            for (int r = taX - 1; r <= taX + 1; r++)
            {
                for (int c = taY - 1; c <= taY + 1; c++)
                {
                    if (r < 0 || r >= width || c < 0 || c >= height)
                        continue;
                    if (map[r, c] != MapGenerator.StateIndexEnum.Normal)
                    {
                        // other이 있는 방향으로 -1
                        if (r > taX) dirX -= 1;
                        if (r < taX) dirX += 1;
                        if (c > taY) dirY -= 1;
                        if (c < taY) dirY += 1;
                    }
                }
            }
            // 더 작은 단위로 움직임
            if (dirX > 0) dirX = 1;
            if (dirX < 0) dirX = -1;
            if (dirY > 0) dirY = 1;
            if (dirY < 0) dirY = -1;

            if (taX + dirX < 0 || taX + dirX >= width
                || taY + dirY < 0 || taY + dirY >= height)
                return;
            if (map[taX + dirX, taY + dirY] == MapGenerator.StateIndexEnum.Normal)
            {
                map[taX, taY] = MapGenerator.StateIndexEnum.Normal;
                map[taX + dirX, taY + dirY] = MapGenerator.StateIndexEnum.TargetItem;
                targetArea = new MapGenerator.Coord(taX + dirX, taY + dirY);
            }
        }
    }

    Vector3 GetRealPos(int localX, int localY, int startX, int startY, int sect_x, int sect_y)
    {
        float fstartX = ((sect_x * 100) + startX) * mapGenerator.squareSize;
        float fstartY = ((sect_y * 100) + startY) * mapGenerator.squareSize;

        Vector3 pos = new Vector3(localX * squareSize + fstartX, -12, localY * squareSize + fstartY);
        return pos;
    }

    public void SetObjects(int[,] localmap, int width, int height, int roomStartX, int roomStartY, Room setRoom)
    {
        // Set Room Info
        setRoom.roomSizeMin.x = roomStartX;
        setRoom.roomSizeMax.x = roomStartX + width / 2;
        setRoom.roomSizeMin.z = roomStartY;
        setRoom.roomSizeMax.z = roomStartY + height / 2;
        //

        float[] enemyGenProb = new float[4];
        float totalProb = 0f;
        for (int e = 0; e < 4; e++)
            totalProb += mapGenerator.heuristicPred.enemyGenProb[e];
        for (int e = 0; e < 4; e++)
        {
            enemyGenProb[e] = mapGenerator.heuristicPred.enemyGenProb[e] / totalProb;
            if (e > 0)
                enemyGenProb[e] += enemyGenProb[e - 1];
        }
        // weapon
        float[] weaponGenProb = new float[7];
        totalProb = 0f;
        for (int e = 1; e < 7; e++)
            totalProb += mapGenerator.heuristicPred.propOfWeapons[e];
        weaponGenProb[0] = 0f;
        for (int e = 1; e < 7; e++)
        {
            weaponGenProb[e] = mapGenerator.heuristicPred.propOfWeapons[e] / totalProb;
            if (e > 0)
                weaponGenProb[e] += weaponGenProb[e - 1];
        }
        // Heal
        float[] healGenProb = new float[3];
        totalProb = 0f;
        for (int e = 0; e < 3; e++)
            totalProb += mapGenerator.heuristicPred.propOfHeal[e];
        healGenProb[0] = 0f;
        for (int e = 0; e < 3; e++)
        {
            healGenProb[e] = mapGenerator.heuristicPred.propOfHeal[e] / totalProb;
            if (e > 0)
                healGenProb[e] += healGenProb[e - 1];
        }

        float randFloat = 0f;
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                switch(localmap[x,y])
                {
                    case MapGenerator.StateIndexEnum.Spawner:
                        // Instantiate Spawner
                        GameObject genSpawner = Instantiate(spawner_prefab, mapGenerator.thisSection.transform);
                        genSpawner.transform.position = GetRealPos(x, y, roomStartX, roomStartY,
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);

                        // set vars
                        Spawner spw = genSpawner.GetComponent<Spawner>();
                        spw.fperiod = (float)Random.Range(mapGenerator.heuristicPred.iminSpawnPeriod, mapGenerator.heuristicPred.imaxSpawnPeriod);
                                                
                        randFloat = Random.Range(0f, 100f);
                        randFloat /= 100f;
                        if (randFloat <= enemyGenProb[0])
                            spw.genEnemyIdx = GameManager.EnemyIndexEnum.ShortSmallEnemy;
                        else if (randFloat <= enemyGenProb[1])
                            spw.genEnemyIdx = GameManager.EnemyIndexEnum.ShortBigEnemy;
                        else if (randFloat <= enemyGenProb[2])
                            spw.genEnemyIdx = GameManager.EnemyIndexEnum.LongGroundEnemy;
                        else spw.genEnemyIdx = GameManager.EnemyIndexEnum.LongFlyEnemy;

                        int genNum = Random.Range(mapGenerator.heuristicPred.iminGenEnemyNum, mapGenerator.heuristicPred.imaxGenEnemyNum + 1);
                        switch (spw.genEnemyIdx)
                        {
                            case GameManager.EnemyIndexEnum.ShortSmallEnemy:
                                break;
                            case GameManager .EnemyIndexEnum.ShortBigEnemy:
                                genNum = (int)(genNum * 0.3f);
                                break;
                            case GameManager.EnemyIndexEnum.LongGroundEnemy:
                                genNum = (int)(genNum * 0.7f);
                                break;
                            case GameManager.EnemyIndexEnum.LongFlyEnemy:
                                genNum = (int)(genNum * 0.5f);
                                break;
                        }
                        if (genNum <= 0) genNum = 1;
                        spw.igenEnemyNum = genNum;
                        genSpawner.SetActive(true);

                        // Room Info Update
                        setRoom.spawners.Add(spw);
                        break;

                    case MapGenerator.StateIndexEnum.Weapon:
                        randFloat = Random.Range(0f, 100f);

                        GameManager.ItemIndexEnum item = GameManager.ItemIndexEnum.None;
                        randFloat /= 100f;
                        if (randFloat <= weaponGenProb[1])
                            item = GameManager.ItemIndexEnum.Rifle;
                        else if (randFloat <= weaponGenProb[2])
                            item = GameManager.ItemIndexEnum.Shotgun;
                        else if (randFloat <= weaponGenProb[3])
                            item = GameManager.ItemIndexEnum.Machinegun;
                        else if (randFloat <= weaponGenProb[4])
                            item = GameManager.ItemIndexEnum.Sniper;
                        else if (randFloat <= weaponGenProb[5])
                            item = GameManager.ItemIndexEnum.Crowbar;
                        else item = GameManager.ItemIndexEnum.Lightsaber;

                        GameObject genWeapon = Instantiate(items[(int)item - 2], mapGenerator.thisSection.transform);
                        genWeapon.transform.position = GetRealPos(x, y, roomStartX, roomStartY,
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        genWeapon.SetActive(true);

                        // Room Info Update
                        setRoom.weapons[(int)item - 1]++;

                        // Section Info Update
                        mapGenerator.thisSection.heuristics.iweaponNum++;
                        mapGenerator.thisSection.heuristics.fpropOfWeapons[(int)item - 1]++;
                        mapGenerator.thisSection.heuristics.weapons_coord.Add(new MapGenerator.Coord((int)genWeapon.transform.position.x,
                            (int)genWeapon.transform.position.z));
                        break;

                    case MapGenerator.StateIndexEnum.Buff:
                        int randInt = Random.Range(0, 2);
                        GameManager.ItemIndexEnum buff = GameManager.ItemIndexEnum.None;

                        if (randInt == 0) buff = GameManager.ItemIndexEnum.Steampack;
                        if (randInt == 1) buff = GameManager.ItemIndexEnum.Picture;

                        GameObject genBuff = Instantiate(items[(int)buff - 2], mapGenerator.thisSection.transform);
                        genBuff.transform.position = GetRealPos(x, y, roomStartX, roomStartY,
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        genBuff.SetActive(true);

                        // Room Info Update
                        setRoom.buffItems[(int)buff - 10 - 1]++;

                        // Section Info Update
                        mapGenerator.thisSection.heuristics.ibuffNum++;
                        mapGenerator.thisSection.heuristics.buffs_coord.Add(new MapGenerator.Coord((int)genBuff.transform.position.x,
                            (int)genBuff.transform.position.z));

                        break;

                    case MapGenerator.StateIndexEnum.Heal:
                        randFloat = Random.Range(0f, 100f);

                        GameManager.ItemIndexEnum heal = GameManager.ItemIndexEnum.None;
                        randFloat /= 100f;

                        if (randFloat <= healGenProb[0])
                            heal = GameManager.ItemIndexEnum.FirstAidKit;
                        else if (randFloat <= healGenProb[1])
                            heal = GameManager.ItemIndexEnum.NanoCure;
                        else heal = GameManager.ItemIndexEnum.PoisonCure;

                        GameObject genHeal = Instantiate(items[(int)heal - 2], mapGenerator.thisSection.transform);
                        genHeal.transform.position = GetRealPos(x, y, roomStartX, roomStartY,
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        genHeal.SetActive(true);

                        // Room Info Update
                        setRoom.healitems[(int)heal - 7 - 1]++;

                        // Section Info Update
                        mapGenerator.thisSection.heuristics.iHealNum++;
                        mapGenerator.thisSection.heuristics.fpropOfHeals[(int)heal - 7 - 1]++;
                        mapGenerator.thisSection.heuristics.heals_coord.Add(new MapGenerator.Coord((int)genHeal.transform.position.x,
                            (int)genHeal.transform.position.z));
                        break;

                    case MapGenerator.StateIndexEnum.TargetItem:
                        GameObject genTargetItem = Instantiate(targetItem_prefab, mapGenerator.thisSection.transform);
                        genTargetItem.transform.position = GetRealPos(x, y, roomStartX, roomStartY,
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        genTargetItem.SetActive(true);

                        // Room Info Update
                        setRoom.targetItems++;
                        break;

                    case MapGenerator.StateIndexEnum.TargetArea:
                        GameObject genTargetArea = Instantiate(targetArea_prefab, mapGenerator.thisSection.transform);
                        genTargetArea.transform.position = GetRealPos(x, y, roomStartX, roomStartY,
                            mapGenerator.thisSection.sect_position[0], mapGenerator.thisSection.sect_position[1]);
                        genTargetArea.SetActive(true);

                        // Section Info Update
                        mapGenerator.thisSection.targetArea = genTargetArea.GetComponent<TargetArea>();
                        StageManager.Instance.heuristics.isGenTargetArea = true;
                        break;
                    default: break;
                }
            }
        }
    }
    public void SetTerrainObjects()
    {

    }
}
