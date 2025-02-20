using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalItem : MonoBehaviour
{

    public GameManager.ItemIndexEnum item_idx;
    public Vector3 rotate_speed = Vector3.zero;

    void Start()
    {
        /* item idx  ������ ���� üũ */
        if (item_idx < 0)
        {
            Debug.Log("Wrong Item Index");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        /* rotate �Լ� ��� �ϳ��� �Լ��� ��ü */
        transform.Rotate(rotate_speed * Time.deltaTime);
    }
}
