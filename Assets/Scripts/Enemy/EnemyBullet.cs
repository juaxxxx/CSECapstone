using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.right * -1 * Time.deltaTime);
    }

    void OnEnable()
    {
        StartCoroutine(LiveCounter());
    }

    IEnumerator LiveCounter()
    {
        // 속도에 따라 시간 조절 필요
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("Terrain"))
            Destroy(gameObject);
    }
}

