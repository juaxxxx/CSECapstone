using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    /*
    fhp : ���� ü��
    fattack : ���� ���ݷ�
    fatkSpeed : ���� ���� �ӵ�
    fatkRange : ���� ���� ����
    fspeed : ���� �̵� �ӵ�
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
