using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    /* Variables */
    /* Singleton */
    private static StageManager instance = null;
    public static StageManager Instance
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

    /* Variables */
    public Character player;
    public Weapon weapon;
    public List<Section> sections;
    public int itargetItemNum = 20;
    public int icurTargetItemNum = 0;
    public int iremainTargetItemNum = 20;
    public HeuristicCompsOfStage heuristics = new HeuristicCompsOfStage();

    InStageUIManager ISUIManager;
    public bool[,] sectionGeneratedFlag = new bool[5, 5];

    public GameObject sectionPrefab;

    private void Start()
    {
        SetStageStart();
    }

    /* Functions */
    public object ParseXyNIndex(int x_or_index, int y = -1)
    {
        if(y == -1)
        {
            int x_axis = x_or_index / 5;
            int y_axis = x_or_index % 5;
            int[] newPos = new int[2];
            newPos[0] = x_axis;
            newPos[1] = y_axis;
            return newPos;
        }
        return (x_or_index * 5) + y;
    }
    public void UpdateCurTargetItemNum()
    {
        icurTargetItemNum++;
        iremainTargetItemNum = itargetItemNum - icurTargetItemNum; // itargetItemmNum - icurTargetItemNum
    }
    public bool CheckClear()
    {
        return itargetItemNum == icurTargetItemNum;
    }
    public void EndStage(bool win)
    {
        // InStageUIManager의 함수 호출 필요 (완)
        if (win) ISUIManager.stageClear();
        else ISUIManager.stageFail();
    }
    public void GenerateSection(Entrance entrance, bool isFirst)
    {
        /* 섹션 만들기 */
        GameObject newSection = Instantiate(sectionPrefab);
        newSection.SetActive(true);
        Section newSect = newSection.GetComponent<Section>();
        newSect.initNewSection(entrance, isFirst);
        
        /* 섹션 만든 후 작업할 것 */
        sectionGeneratedFlag[newSect.sect_position[0], newSect.sect_position[1]] = true;
        sections[newSect.sectIndex] = newSect;
    }

    void SetStageStart()
    {
        /* 최초 스테이지 시작시 설정 */
        // Dummy entrance: linked_pos == 0, 0
        Entrance dummyEnter = new Entrance();
        dummyEnter.linked_section_pos.Add(0);
        dummyEnter.linked_section_pos.Add(0);
        dummyEnter.linked_section_pos[0] = 0;
        dummyEnter.linked_section_pos[1] = 0;
        GenerateSection(dummyEnter, true);
    }

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

        ISUIManager = GameObject.Find("InStageUIManager").GetComponent<InStageUIManager>();

        // 전체 비트맵 초기화
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
                sectionGeneratedFlag[i, j] = false;
        
        // Section List 초기화
        sections = new List<Section>(25);
        for (int i = 0; i < 25; i++)
            sections.Add(null);
    }
}