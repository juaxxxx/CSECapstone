using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    /* Variables */
    public float fperiod = 0f;
    public float fgenTimer = 0f;
    public float fgenRadius = 3f;
    public GameManager.EnemyIndexEnum genEnemyIdx;
    public int igenEnemyNum = 0;
    public int icurEnemyNum = 0;
    bool isFirst = true;

    Vector3[] dirs = new Vector3[8]
    {
        new Vector3(1f, 0f, 0f),
        new Vector3(0f, 0f, 1f),
        new Vector3(1f, 0f, 1f),
        new Vector3(-1f, 0f, 0f),
        new Vector3(0f, 0f, -1f),
        new Vector3(-1f, 0f, 1f),
        new Vector3(-1f, 0f, -1f),
        new Vector3(1f, 0f, -1f)
    };
    int layerMask;

    Spawner I;

    /* Functions */
    void generateEnemy()
    {
        if(igenEnemyNum > icurEnemyNum)
        {
            // 생성시 StageManager에 휴리스틱 값 넘김
            updateEnemySurvive();

            List<int> possibleDirIdx = new List<int>();
            /* Compute generating position */
            for(int i = 0; i < 8; i++)
            {
                // Ray에 지형이 충돌함
                if (Physics.Raycast(transform.position, dirs[i], fgenRadius, layerMask))
                    possibleDirIdx.Add(i);
            }

            /* Generate Enemies */
            while (igenEnemyNum > icurEnemyNum)
            {
                int genDir = Random.Range(0, possibleDirIdx.Count);
                float distance = Random.Range(0f, fgenRadius);
                // EnemyManager에서 prefab을 받아와서 내 자식으로 오브젝트 생성
                GameObject genEnemy = Instantiate(EnemyManager.Instance.enemyPrefabs[(int)genEnemyIdx - 1],
                    transform);
                // 위치 지정
                genEnemy.transform.position = transform.position + (dirs[genDir] * distance);
                Enemy enemy = genEnemy.GetComponent<Enemy>();
                enemy.spawner = I;
                // 활성화
                genEnemy.SetActive(true);
                icurEnemyNum++;
            }
        }
    }

    // StageManager에 생존 횟수 넘겨주기
    void updateEnemySurvive()
    {
        if (!isFirst)
        {
            StageManager.Instance.heuristics.idieEnemyNums[(int)genEnemyIdx - 1] = igenEnemyNum - icurEnemyNum;
        }
        StageManager.Instance.heuristics.igenEnemyNums[(int)genEnemyIdx - 1] = igenEnemyNum - icurEnemyNum;
        if (isFirst) isFirst = false;
    }

    void OnEnable()
    {
        layerMask = 1 << LayerMask.NameToLayer("Terrain");
        I = GetComponent<Spawner>();
    }

    void Update()
    {
        for(int i = 0; i < 8; i++)
        {
            Debug.DrawRay(transform.position, dirs[i], new Color(1, 0, 0));
        }

        fgenTimer -= Time.deltaTime;
        if (fgenTimer < 0)
        {
            generateEnemy();
            fgenTimer = fperiod;
        } 
    }
}
