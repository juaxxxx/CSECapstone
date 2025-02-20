using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section : MonoBehaviour
{
    /* Variables */
    public int sectIndex = -1;
    // 더 이상 필요하지 않음
    public Vector3 minCoord = new Vector3();
    public Vector3 maxCoord = new Vector3();
    public Vector3 sectionSize = new Vector3();
    //
    public List<Room> rooms = new List<Room>();
    public HeuristicCompsOfSection heuristics;
    public MapGenerator mapGenerator;
    public TargetArea targetArea = null;
    public List<Entrance> entracnes = new List<Entrance>(4); // 0, 1, 2, 3 : 상, 하, 좌, 우
    public List<int> sect_position = new List<int>();


    public void initNewSection(Entrance enterance, bool isFirst)
    {
        sect_position = enterance.linked_section_pos;
        sectIndex = (int)StageManager.Instance.ParseXyNIndex(sect_position[0], sect_position[1]);
        heuristics = new HeuristicCompsOfSection();
        // init entrances
        for (int i = 0; i < 4; i++) entracnes.Add(null);

        mapGenerator.thisSection = this;
        if(isFirst)
            mapGenerator.prevSection = null;
        else mapGenerator.prevSection = StageManager.Instance.sections[StageManager.Instance.heuristics.isectionIndexInPlayer];
        mapGenerator.GenerateMap(isFirst);
        heuristics.SetValues(rooms);
        Destroy(mapGenerator.gameObject);
    }

    private void Awake()
    {
        sect_position.Add(-1);
        sect_position.Add(-1);
    }
}
