using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalItem : MonoBehaviour
{

    public GameManager.ItemIndexEnum item_idx;
    public Vector3 rotate_speed = Vector3.zero;

    void Start()
    {
        /* item idx  오류를 위한 체크 */
        if (item_idx < 0)
        {
            Debug.Log("Wrong Item Index");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        /* rotate 함수 대신 하나의 함수로 대체 */
        transform.Rotate(rotate_speed * Time.deltaTime);
    }
}
