using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetArea : MonoBehaviour
{
    public int sectionIdx;
    public int roomIdx;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
