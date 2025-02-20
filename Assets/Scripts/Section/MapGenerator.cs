using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    /* Enum */
    /*
    public enum StateIndexEnum
    {
        Normal,
        Die,
        TargetItem,
        TargetArea,
        Terrain,
        Entrance,
        Spawner,
        Weapon,
        Heal,
        Buff
    }
    */
    public static class StateIndexEnum
    {
        public const int Normal = 0;
        public const int Terrain = 1;
        public const int TargetItem = 2;
        public const int TargetArea = 3;
        public const int Die = 4;
        public const int Entrance = 5;
        public const int Spawner = 6;
        public const int Weapon = 7;
        public const int Heal = 8;
        public const int Buff = 9;
    }

    /* Classes */
    public class MapRoom : IComparable<MapRoom>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<MapRoom> connectedRooms;
        public int roomSize;
        public int roomHeight;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public MapRoom()
        {

        }

        public MapRoom(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<MapRoom>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == StateIndexEnum.Terrain)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }
        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (MapRoom connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }
        public static void ConnectRooms(MapRoom roomA, MapRoom roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(MapRoom otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }
        public int CompareTo(MapRoom otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

    /* Structs */
    public struct Coord
    {
        public int tileX, tileY;

        public Coord(int x, int y)
        {
            this.tileX = x;
            this.tileY = y;
        }
    }

    /* Variables */
    public Section thisSection;
    public int width;
    public int height;
    public string seed;
    [Range(0, 100)]
    public int randomFillPercent;
    public List<MapRoom> survivingRooms = new List<MapRoom>();
    int[,] map;
    int[] addRoomSize;
    public List<int[]> eachRoomSize;
    public int[] edgeRoomIdx; // 상,하,좌,우 순서

    float iroomSizeX, iroomSizeZ;
    float minRoomX, minRoomZ, maxRoomX, maxRoomZ;
    int minRoomTileX, minRoomTileY, maxRoomTileX, maxRoomTileY;
    int totalMinRoomTileX, totalMaxRoomTileX, totalMinRoomTileY, totalMaxRoomTileY;
    public int[] curFlag = new int[2]; // 0 : x , 1 : y

    public float squareSize;
    //float minSectionX, minSectionZ, maxSectionX, maxSectionZ;

    public int borderSize;
    public int wallThresholdSize;
    public int roomThresholdSize;
    public bool pass;

    public HeuristicPrediction heuristicPred;
    public Section prevSection = null;
    public GameObject characterpos;
    public GameObject camerapos;
    Vector3 pos;

    private void Awake()
    {
        characterpos = StageManager.Instance.player.gameObject;
        camerapos = GameObject.Find("CameraManager");
    }

    /* Functions */
    public void GenerateMap(bool isFirst = true)
    {
        // heuristicPrediction 계산
        heuristicPred = new HeuristicPrediction(isFirst, prevSection);
        if(!isFirst) ParameterManager.Instance.runRules(this, heuristicPred);

        while (true)
        {
            map = new int[width, height];
            RandomFillMap();

            for (int i = 0; i < 5; i++) SmoothMap();
            ProcessMap();

            if (!pass)
            {
                continue;
            }

            /*Mesh 생성*/
            TerrainMeshGenerator terrainMeshGen = GetComponent<TerrainMeshGenerator>();
            terrainMeshGen.GenerateMesh(map, squareSize, 15);

            if (isFirst)
            {
                Vector3 initpos = new Vector3(survivingRooms[1].tiles[3].tileX * squareSize, 0, survivingRooms[1].tiles[3].tileY * squareSize);
                characterpos.transform.position = initpos;
                camerapos.transform.position = initpos;
            }

            RoomObjectGenerator roomObjectGen = GetComponent<RoomObjectGenerator>();
            roomObjectGen.GenerateRoom(map, survivingRooms, eachRoomSize, edgeRoomIdx);
            
            break;
        }
    }
    void RandomFillMap()
    {   
        seed = (Time.time + UnityEngine.Random.Range(0,10f)).ToString();
        System.Random pRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = StateIndexEnum.Terrain;
                }
                else
                {
                    map[x, y] = (pRandom.Next(0, 100) < randomFillPercent) ? StateIndexEnum.Terrain : StateIndexEnum.Normal;
                }
            }
        }
    }
    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4) map[x, y] = StateIndexEnum.Terrain;
                else if (neighbourWallTiles < 4) map[x, y] = StateIndexEnum.Normal;
            }
        }
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(StateIndexEnum.Terrain);

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = StateIndexEnum.Normal;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(StateIndexEnum.Normal);
        survivingRooms.Clear();


        foreach (List<Coord> roomRegion in roomRegions)
        {
            minRoomX = 1000000;
            minRoomZ = 1000000;
            maxRoomX = 0;
            maxRoomZ = 0;

            foreach (Coord tile in roomRegion)
            {
                if (tile.tileX < minRoomX) minRoomX = tile.tileX;
                if (tile.tileY < minRoomZ) minRoomZ = tile.tileY;
                if (tile.tileX > maxRoomX) maxRoomX = tile.tileX;
                if (tile.tileY > maxRoomZ) maxRoomZ = tile.tileY;
            }
            iroomSizeX = maxRoomX - minRoomX;
            iroomSizeZ = maxRoomZ - minRoomZ;

            if (iroomSizeX < heuristicPred.minRoomSize.x || iroomSizeX > heuristicPred.maxRoomSize.x || iroomSizeZ < heuristicPred.minRoomSize.z || iroomSizeZ > heuristicPred.maxRoomSize.z)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = StateIndexEnum.Terrain;
                }
            }
            else
            {
                survivingRooms.Add(new MapRoom(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        if(survivingRooms.Count != 0)
        {
            survivingRooms[0].isMainRoom = true;
            survivingRooms[0].isAccessibleFromMainRoom = true;
        }

        // survivingRooms가 조건 충족 시
        if (survivingRooms.Count > 2)
        {
            pass = true;

            totalMinRoomTileX = 101;
            totalMinRoomTileY = 101;
            totalMaxRoomTileX = 0;
            totalMaxRoomTileY = 0;
            edgeRoomIdx = new int[4];
            eachRoomSize = new List<int[]>();
            
            for (int i = 0; i < survivingRooms.Count; i++)
            {
                minRoomTileX = 101;
                minRoomTileY = 101;
                maxRoomTileX = 0;
                maxRoomTileY = 0;

                // 방마다 비트맵 크기 구하기
                foreach (Coord tile in survivingRooms[i].edgeTiles)
                {
                    if (minRoomTileX > tile.tileX) minRoomTileX = tile.tileX;
                    if (maxRoomTileX < tile.tileX) maxRoomTileX = tile.tileX;
                    if (minRoomTileY > tile.tileY) minRoomTileY = tile.tileY;
                    if (maxRoomTileY < tile.tileY) maxRoomTileY = tile.tileY;

                    if (totalMinRoomTileX > tile.tileX)
                    {
                        totalMinRoomTileX = tile.tileX;
                        edgeRoomIdx[2] = i;
                    }
                    if (totalMaxRoomTileX < tile.tileX)
                    {
                        totalMaxRoomTileX = tile.tileX;
                        edgeRoomIdx[3] = i;
                    }
                    if (totalMinRoomTileY > tile.tileY)
                    {
                        totalMinRoomTileY = tile.tileY;
                        edgeRoomIdx[1] = i;
                    }
                    if (totalMaxRoomTileY < tile.tileY)
                    {
                        totalMaxRoomTileY = tile.tileY;
                        edgeRoomIdx[0] = i;
                    }
                }

                addRoomSize = new int[4] {minRoomTileX, maxRoomTileX, minRoomTileY, maxRoomTileY}; // 0: min(x), 1: max(x), 2: min(y), 3: max(y)
                eachRoomSize.Add(addRoomSize);

                
            }

            for (int i = 0; i < survivingRooms.Count; i++)
            {
                Room initroom = new Room();
                thisSection.rooms.Add(initroom);
            }

            ConnectClosestRooms(survivingRooms);
        }
        else
        {
            pass = false;
        }
        
    }
    void ConnectClosestRooms(List<MapRoom> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<MapRoom> roomListA = new List<MapRoom>();
        List<MapRoom> roomListB = new List<MapRoom>();
        List<int> roomListAIdx = new List<int>();
        List<int> roomListBIdx = new List<int>();

        if (forceAccessibilityFromMainRoom)
        {
            for (int i =0; i < allRooms.Count; i++)
            {
                if (allRooms[i].isAccessibleFromMainRoom)
                {
                    roomListB.Add(allRooms[i]);
                    roomListBIdx.Add(i);
                }
                else
                {
                    roomListA.Add(allRooms[i]);
                    roomListAIdx.Add(i);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
            for (int i = 0; i < allRooms.Count; i++)
            {
                roomListBIdx.Add(i);
                roomListAIdx.Add(i);
            }
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        MapRoom bestRoomA = new MapRoom();
        MapRoom bestRoomB = new MapRoom();
        bool possibleConnectionFound = false;
        for (int i =0; i < roomListA.Count; i++)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomListA[i].connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            for (int j =0; j < roomListB.Count; j++)
            {
                if (roomListA[i] == roomListB[j] || roomListA[i].IsConnected(roomListB[j]))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomListA[i].edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomListB[j].edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomListA[i].edgeTiles[tileIndexA];
                        Coord tileB = roomListB[j].edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomListA[i];
                            bestRoomB = roomListB[j];

                            if (!thisSection.rooms[roomListAIdx[i]].adjacencyRooms.Contains(roomListBIdx[j]))
                            {
                                thisSection.rooms[roomListAIdx[i]].adjacencyRooms.Add(roomListBIdx[j]);
                            }

                            if (!thisSection.rooms[roomListBIdx[j]].adjacencyRooms.Contains(roomListAIdx[i]))
                            {
                                thisSection.rooms[roomListBIdx[j]].adjacencyRooms.Add(roomListAIdx[i]);
                            }
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }


        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(MapRoom roomA, MapRoom roomB, Coord tileA, Coord tileB)
    {
        MapRoom.ConnectRooms(roomA, roomB);
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 1);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = StateIndexEnum.Normal;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width + 1, height + 1];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == StateIndexEnum.Normal && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = StateIndexEnum.Terrain;
                    }
                }
            }
        }

        return regions;
    }
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width + 1, height + 1];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = StateIndexEnum.Terrain;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == StateIndexEnum.Normal && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = StateIndexEnum.Terrain;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }
}
