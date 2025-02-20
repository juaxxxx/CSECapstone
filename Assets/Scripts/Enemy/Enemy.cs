using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    /*
    fhp : 적의 체력
    fattack : 적의 공격력
    fatkSpeed : 적의 공격 속도
    fatkRange : 적의 공격 범위
    fspeed : 적의 이동 속도
     */

    public float fhp;
    public float fattack;
    public float fatkSpeed;
    public float fatkRange;
    public float fspeed;
    public float radius = 10f;

    public Spawner spawner;
    public Animator anim;
    public bool isAlive = true;

    public abstract void Move();
    public abstract IEnumerator Attack();
    public void OnDamage(float damage)
    {
        if (isAlive)
        {
            fhp -= damage;
            StageManager.Instance.heuristics.UpdateDPS(damage);
            if (fhp <= 0)
            {
                isAlive = false;
                if (spawner != null)
                    spawner.icurEnemyNum--;

                anim.SetTrigger("doDie");
                Destroy(gameObject, 3f);
            }
        }
    }
}
