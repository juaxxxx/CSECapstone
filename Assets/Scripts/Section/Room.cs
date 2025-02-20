using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room
{
    /* Variables */
    public int[] weapons = Enumerable.Repeat<int>(0, 7).ToArray<int>();
    public int[] healitems = Enumerable.Repeat<int>(0, 3).ToArray<int>();
    public int[] buffItems = Enumerable.Repeat<int>(0, 2).ToArray<int>();
    public int targetItems = 0;
    public List<Spawner> spawners = new List<Spawner>();
    public Vector3 roomSizeMin = new Vector3(100, 0, 100);
    public Vector3 roomSizeMax = new Vector3(-1, 0, -1);
    public List<int> adjacencyRooms = new List<int>();
    public int linkedSectionIndexOfEntrance = -1;

    public Vector3 roomMinCoord = new Vector3();
    public Vector3 roomMaxCoord = new Vector3();

    public bool CheckInRoom(Vector3 pos)
    {
        if (pos.x < roomMinCoord.x || pos.x > roomMaxCoord.x
            || pos.y < roomMinCoord.y || pos.y > roomMaxCoord.y
            || pos.z < roomMinCoord.z || pos.z > roomMaxCoord.z)
        {
            return false;
        }
        return true;
    }
}
