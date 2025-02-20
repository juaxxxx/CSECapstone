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
        // InStageUIManager�� �Լ� ȣ�� �ʿ� (��)
        if (win) ISUIManager.stageClear();
        else ISUIManager.stageFail();
    }
    public void GenerateSection(Entrance entrance, bool isFirst)
    {
        /* ���� ����� */
        GameObject newSection = Instantiate(sectionPrefab);
        newSection.SetActive(true);
        Section newSect = newSection.GetComponent<Section>();
        newSect.initNewSection(entrance, isFirst);
        
        /* ���� ���� �� �۾��� �� */
        sectionGeneratedFlag[newSect.sect_position[0], newSect.sect_position[1]] = true;
        sections[newSect.sectIndex] = newSect;
    }

    void SetStageStart()
    {
        /* ���� �������� ���۽� ���� */
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
        /* DontDestory �ɼ��� �������� ���� */
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        ISUIManager = GameObject.Find("InStageUIManager").GetComponent<InStageUIManager>();

        // ��ü ��Ʈ�� �ʱ�ȭ
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
                sectionGeneratedFlag[i, j] = false;
        
        // Section List �ʱ�ȭ
        sections = new List<Section>(25);
        for (int i = 0; i < 25; i++)
            sections.Add(null);
    }
}