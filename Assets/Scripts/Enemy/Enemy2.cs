using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : Enemy
{
    GameObject PlayerTrans;
    Character Player;
    Transform tr;
    Transform target;
    Rigidbody rigid;
    public BoxCollider meleeArea;
    Vector3 movePos;
    public bool isAttack;
    public bool isChase = false;
    void Awake()
    {
        fhp = 100f;
        fattack = 10f;
        fatkSpeed = 0.5f;
        fatkRange = 4f;
        fspeed = 5f;

        tr = GetComponent<Transform>();
        PlayerTrans = GameObject.Find("Character");
        Player = PlayerTrans.GetComponent<Character>();
        target = PlayerTrans.GetComponent<Transform>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();
    }
    void FixedUpdate()
    {
        Targeting();
    }

    public override void Move()
    {
        if (isAlive)
        {
            // 플레이어 콜라이더 감지
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, radius);

            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    isChase = true;

                    Player.battleStateOn();
                }
            }
            if (isChase)
            {
                anim.SetBool("isWalk", true);
                float dist = Vector3.Distance(tr.position, target.position);
                movePos = target.position;

                Quaternion rot = Quaternion.LookRotation(movePos - tr.position);
                tr.rotation = Quaternion.Slerp(tr.rotation, rot, Time.deltaTime * 5f);
                tr.Translate(Vector3.forward * Time.deltaTime * fspeed);
            }
            else
            {
                anim.SetBool("isWalk", false);

            }
            isChase = false;
        }
        else
        {

        }
    }

    void Targeting()
    {
        if (isAlive)
        {
            float targetRadius = 1.5f;

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
                                                        targetRadius,
                                                        transform.forward,
                                                        fatkRange,
                                                        LayerMask.GetMask("Player"));
            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }

    public override IEnumerator Attack()
    {
        if (isAlive)
        {

            isAttack = true;
            anim.SetBool("isAttack", true);

            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;

            yield return new WaitForSeconds(0.2f);
            meleeArea.enabled = true;

            yield return new WaitForSeconds(1f);
            anim.SetBool("isAttack", false);
            meleeArea.enabled = false;
            isAttack = false;


            yield return new WaitForSeconds(1f);

        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet playerBullet = other.GetComponent<Bullet>();
            OnDamage(playerBullet.fattack);
            StageManager.Instance.heuristics.UpdateHitEnemyNum(GameManager.EnemyIndexEnum.ShortBigEnemy);
        }
        else if (other.CompareTag("PlayerWeapon"))
        {
            Weapon playerWeapon = GameObject.Find("Weapon").GetComponent<Weapon>();
            OnDamage(playerWeapon.currentWeapon.fattack);
            StageManager.Instance.heuristics.UpdateHitEnemyNum(GameManager.EnemyIndexEnum.ShortBigEnemy);
        }
    }
}
