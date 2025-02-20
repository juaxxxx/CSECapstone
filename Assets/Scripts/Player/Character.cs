using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float fhp = 100f;
    public float fspeed = 15f;
    public float fjumpForce = 15f;
    public float fturnSmoothness = 10f;
    float fmaxhp = 100f;
    float hAxis = 0f;
    float vAxis = 0f;
    float attackDelay;
    float fbattleTimeMax = 3f;
    float fcntBattle;
    bool jDown;
    bool aDown;
    bool canJump = true;
    bool startTeleport = true;
    bool endTeleport = true;
    bool doUnderRay = true;
    bool isAlive = true;
    bool isAttackReady = true;
    int iSteamPackCounter = 0;
    int iPictureCounter = 0;
    int iPoisonCounter = 0;
    int iSlowCounter = 0;
    int iRestraintCounter = 0;
    int iStuckCounter = 0;
    int debuffSeed;
    int layerMask;

    public bool[] debuffList = new bool[5]; // ����� ����Ʈ 0 : ���� ��, 1 : �ٸ�, 2 : �̼�, 3 : �ӹ�, 4 : ����
    bool[] buffList = new bool[2]; // ���� ����Ʈ 0 : ������, 1 : ��������
    Vector3 moveVec;
    Vector3 turnRot;

    public Weapon myWeapon;
    GameObject TriggerObject;
    Animator anim;
    Rigidbody rigid;
    Camera cam;
    GameObject stagemanager;
    GameObject cameramanager;
    GameObject SUIManager; //GUI

    
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        cam = Camera.main;
        SUIManager = GameObject.Find("InStageUIManager"); // GUI
        stagemanager = GameObject.Find("StageManager");
        cameramanager = GameObject.Find("CameraManager");
    }
    private void Start()
    {
        SUIManager.GetComponent<InStageUIManager>().currentHP = fhp; // GUI
        SUIManager.GetComponent<InStageUIManager>().targetItem.text = stagemanager.GetComponent<StageManager>().iremainTargetItemNum.ToString();
    }

    float roomCheckTimer = 3f;
    private void Update()
    {
        getInput();
        move();
        jump_control();
        jump();
        Attack();

        // room pos
        if(roomCheckTimer  > 0)
        {
            roomCheckTimer -= Time.deltaTime;
        }
        else
        {
            roomCheckTimer = 3f;
            // 플레이어의 룸 정보 갱신
            StageManager.Instance.heuristics.UpdateWherePlayer(transform.position,
                StageManager.Instance.sections[StageManager.Instance.heuristics.isectionIndexInPlayer]);
        }
    }

    private void LateUpdate()
    {
        rotateCharacter();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Weapon")
        {
            TriggerObject = other.gameObject;
            myWeapon.weaponChange((GameManager.WeaponIndexEnum)TriggerObject.GetComponent<NormalItem>().item_idx);
            anim.SetTrigger("doSwap");
            Destroy(TriggerObject);
        }
        else if (other.tag == "Item")
        {
            NormalItem currentItem = other.GetComponent<NormalItem>();
            switch((int)currentItem.item_idx)
            {
                case 8:
                    takeItem(0);
                    break;
                case 9:
                    takeItem(1);
                    break;
                case 10:
                    takeItem(2);
                    break;
                case 11:
                    takeItem(3);
                    break;
                case 12:
                    takeItem(4);
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "Target")
        {
            takeItem(5);
            Destroy(other.gameObject);
        }
        else if (other.tag == "TargetArea")
        {
            if (StageManager.Instance.CheckClear())
                StageManager.Instance.EndStage(true);
        }
        else if (other.tag == "Bullet1")
        {
            onDamage(10);

            debuffSeed = Random.Range(0, 10);

            if (iPoisonCounter == 0)
                StartCoroutine(poisoned_counter());
            else
                iPoisonCounter = 5;

            if (debuffSeed <= 3)
            {
                if (debuffList[2] == false)
                    StartCoroutine(startSlower());
                else
                    iSlowCounter = 5;
            }
        }
        else if (other.tag == "Bullet2")
        {
            onDamage(10);

            debuffSeed = Random.Range(0, 10);

            if (iPoisonCounter == 0)
                StartCoroutine(poisoned_counter());
            else
                iPoisonCounter = 5;

            if (debuffSeed <= 3)
            {
                if (debuffList[3] == false)
                    StartCoroutine(startRestraint());
                else
                    iRestraintCounter = 2;
            }
        }
        else if (other.tag == "Enemy1")
        {
            onDamage(10);
        }

        else if (other.tag == "Enemy2")
        {
            onDamage(20);
            if (debuffList[1] == false)
            {
                debuffList[1] = true;
                onInjuredLeg();
            }
            else if (debuffList[0] == false)
            {
                debuffList[0] = true;
                onInjuredLeftArm();
            }
            debuffSeed = Random.Range(0, 10);

            if (debuffSeed <= 3)
            {
                if (debuffList[4] == false)
                    StartCoroutine(startStuck());
                else
                    iStuckCounter = 1;
            }
        }
        else if (other.tag == "Entrance")
        {
            Entrance curEnt = other.GetComponent<Entrance>();
            if (curEnt.linked_Entrance != null)
            {
                if (startTeleport && endTeleport)
                {
                    transform.position = curEnt.linked_Entrance.transform.position;
                    cameramanager.transform.position = transform.position;
                    turnRot = transform.rotation.eulerAngles - Quaternion.LookRotation(curEnt.wallDirection).eulerAngles;
                    cameramanager.GetComponent<CameraManager>().changeRot(Quaternion.LookRotation(curEnt.linked_Entrance.wallDirection).eulerAngles + turnRot + new Vector3(0, 180, 0));
                    startTeleport = false;
                }
                else if (endTeleport)
                {
                    endTeleport = false;
                }
            }
            else
            {
                Debug.Log("Generate Section!");
                StageManager.Instance.GenerateSection(curEnt, false);
                if (curEnt.linked_Entrance != null)
                {
                    if (startTeleport && endTeleport)
                    {
                        transform.position = curEnt.linked_Entrance.transform.position;
                        cameramanager.transform.position = transform.position;
                        turnRot = transform.rotation.eulerAngles - Quaternion.LookRotation(curEnt.wallDirection).eulerAngles;
                        cameramanager.GetComponent<CameraManager>().changeRot(Quaternion.LookRotation(curEnt.linked_Entrance.wallDirection).eulerAngles + turnRot + new Vector3(0, 180, 0));
                        startTeleport = false;
                    }
                }
            }

            StageManager.Instance.heuristics.isectionIndexInPlayer = 
                (int)StageManager.Instance.ParseXyNIndex(curEnt.linked_section_pos[0], curEnt.linked_section_pos[1]);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Entrance")
        {
            if (!startTeleport && !endTeleport)
            {
                startTeleport = true;
                endTeleport = true;
            }
        } 
    }

    void takeItem(int item_idx)
    {
        if (item_idx == 0)
        {
            fhp += 20f;
            if (fhp > fmaxhp)
                fhp = fmaxhp;
            SUIManager.GetComponent<InStageUIManager>().currentHP = fhp; // GUI
        }
        else if (item_idx == 1)
        {
            if (debuffList[1] == true)
            {
                debuffList[1] = false;
                onInjuredLeg();
            }
            else if (debuffList[0] == true)
            {
                debuffList[0] = false;
                onInjuredLeftArm();
            }
        }
        else if (item_idx == 2)
        {
            iPoisonCounter = 0;
        }
        else if (item_idx == 3)
        {
            if (buffList[0] == true)
            {
                iSteamPackCounter = 20;
            }
            else
            {
                StartCoroutine(onSteampack());
            }
        }
        else if (item_idx == 4)
        {
            if (buffList[1] == true)
            {
                iPictureCounter = 10;
            }
            else
            {
                StartCoroutine (onPicture());
            }
        }
        else if (item_idx == 5)
        {
            stagemanager.GetComponent<StageManager>().UpdateCurTargetItemNum();
            SUIManager.GetComponent<InStageUIManager>().targetItem.text = stagemanager.GetComponent<StageManager>().iremainTargetItemNum.ToString(); // GUI
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Enemy1")
        {
            onDamage(10);
        }

        if (other.gameObject.tag == "Enemy2")
        {
            onDamage(20);

            debuffSeed = Random.Range(0, 10);

            if (debuffSeed <= 3)
            {
                if (debuffList[4] == false)
                    StartCoroutine(startStuck());
                else
                    iStuckCounter = 1;
            }
        }
        if (other.gameObject.tag == "Enemy3")
        {
            onDamage(10);
        }
        if (other.gameObject.tag == "Enemy4")
        {
            onDamage(10);
        }
    }

    void getInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        moveVec = (transform.TransformDirection(Vector3.forward) * vAxis + transform.TransformDirection(Vector3.right) * hAxis).normalized;
        jDown = Input.GetButtonDown("Jump");
        aDown = Input.GetButton("Fire1");
    }

    void move()
    {
        if(!debuffList[3] && !debuffList[4] && isAlive && !checkHitWall()) // ���� �ӹ� ���� �ƴ� ��
        {
            // ��ġ ����
            transform.position += moveVec * fspeed * Time.deltaTime;

            // �ִϸ��̼� ����
            anim.SetBool("isRun", moveVec != Vector3.zero);
        }

    }

    bool checkHitWall()
    {
        bool wallHit = false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 3f, moveVec, out hit, 2f))
        {
            if (hit.collider.CompareTag("Wall"))
                wallHit = true;
        }

        return wallHit;
    }

    void jump()
    {
        if(jDown && canJump && !debuffList[1] && !debuffList[3] && !debuffList[4] && isAlive)
        {
            rigid.AddForce(Vector3.up * fjumpForce, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            canJump = false;

            doUnderRay = false;
            StartCoroutine(holdJumpAnim());
        }
    }

    IEnumerator holdJumpAnim()
    {
        yield return new WaitForSeconds(0.3f);
        doUnderRay = true;
    }

    void jump_control()
    {
        layerMask = 1 << LayerMask.NameToLayer("Terrain");
        RaycastHit hit;
        float maxDistance = 0.5f;
        if (doUnderRay == true)
        {
            if (Physics.Raycast(rigid.position + Vector3.up * 0.1f, Vector3.down, out hit, maxDistance, layerMask))
            {
                canJump = true;
                anim.SetBool("isJump", false);
            }
            else
            {
                canJump = false;
            }
        }
    }

    public void battleStateOn()
    {
        if (fcntBattle == 0)
            StartCoroutine(battleStateTimeCounter());
        else
            fcntBattle = fbattleTimeMax;
    }

    void Attack()
    {
        attackDelay += Time.deltaTime;
        isAttackReady = myWeapon.GetComponent<Weapon>().currentWeapon.frate < attackDelay;

        if(aDown && isAttackReady && isAlive)
        {
            myWeapon.GetComponent<Weapon>().DoAttack();
            attackDelay = 0;
        }

    }

    void rotateCharacter()
    {
        if (isAlive)
        {
            Vector3 playerRotate = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate), Time.deltaTime * fturnSmoothness);
        }      
    }

    void onDamage(float damage)
    {
        if (!buffList[1] && isAlive)
        {
            fhp -= damage;
            SUIManager.GetComponent<InStageUIManager>().currentHP = fhp; // GUI

            if (fhp <= 0)
            {
                isAlive = false;
                anim.SetTrigger("doDie");
                StageManager.Instance.EndStage(false);
            }
        }
    }

    void onInjuredLeftArm()
    {
        // ���� �� �ı� ��
        if (debuffList[0])
        {
            myWeapon.GetComponent<Weapon>().currentWeapon.frate *= 1.25f;
            SUIManager.GetComponent<InStageUIManager>().armDestroyed.SetActive(true); // GUI
        }

        // ���� �� ȸ�� ��
        if (!debuffList[0])
        {
            myWeapon.GetComponent<Weapon>().currentWeapon.frate *= 0.8f;
            SUIManager.GetComponent<InStageUIManager>().armDestroyed.SetActive(false); // GUI
        }
    }

    void onInjuredLeg()
    {
        if (debuffList[1])
        {
            fspeed *= 0.8f;
            SUIManager.GetComponent<InStageUIManager>().legDestroyed.SetActive(true); // GUI
        }

        if (!debuffList[1])
        {
            fspeed *= 1.25f;
            SUIManager.GetComponent<InStageUIManager>().legDestroyed.SetActive(false); // GUI
        }
    }

    void damaged_poison()
    {
        onDamage(5);
    }
    
    IEnumerator battleStateTimeCounter()
    {
        fcntBattle = fbattleTimeMax;
        while (fcntBattle > 0)
        {
            yield return new WaitForSeconds(1f);
            StageManager.Instance.heuristics.UpdateBattleTime();
            fcntBattle--;
        }
    }

    IEnumerator onSteampack()
    {
        buffList[0] = true;
        fspeed *= 1.25f;
        iSteamPackCounter = 20;
        SUIManager.GetComponent<InStageUIManager>().buffSteam.SetActive(true); // GUI
        while (iSteamPackCounter > 0)
        {
            yield return new WaitForSeconds(1f);
            iSteamPackCounter--;
        }
        buffList[0] = false;
        fspeed *= 0.8f;
        SUIManager.GetComponent<InStageUIManager>().buffSteam.SetActive(false); // GUI
    }

    IEnumerator onPicture()
    {
        buffList[1] = true;
        iPictureCounter = 10;
        SUIManager.GetComponent<InStageUIManager>().buffPicture.SetActive(true); // GUI

        while (iPictureCounter > 0)
        {
            yield return new WaitForSeconds(1f);
            iPictureCounter--;
        }
        buffList[1] = false;
        SUIManager.GetComponent<InStageUIManager>().buffPicture.SetActive(false); // GUI
    }

    IEnumerator poisoned_counter()
    {
        iPoisonCounter = 5;
        SUIManager.GetComponent<InStageUIManager>().debuffPoison.SetActive(true); // GUI

        while (iPoisonCounter > 0)
        {
            yield return new WaitForSeconds(1f);
            damaged_poison();
            iPoisonCounter--;
        }

        SUIManager.GetComponent<InStageUIManager>().debuffPoison.SetActive(false); // GUI
    }
    IEnumerator startSlower()
    {
        debuffList[2] = true;
        fspeed *= 0.8f;
        iSlowCounter = 5;
        SUIManager.GetComponent<InStageUIManager>().debuffSlow.SetActive(true); // GUI

        while (iSlowCounter > 0)
        {
            yield return new WaitForSeconds(1f);
            iSlowCounter--;
        }
        debuffList[2] = false;
        fspeed *= 1.25f;
        SUIManager.GetComponent<InStageUIManager>().debuffSlow.SetActive(false); // GUI
    }

    IEnumerator startRestraint()
    {
        debuffList[3] = true;
        iRestraintCounter = 2;
        SUIManager.GetComponent<InStageUIManager>().debuffRestraint.SetActive(true); // GUI

        while (iRestraintCounter > 0)
        {
            yield return new WaitForSeconds(1f);
            iRestraintCounter--;
        }
        debuffList[3] = false;
        SUIManager.GetComponent<InStageUIManager>().debuffRestraint.SetActive(false); // GUI
    }

    IEnumerator startStuck()
    {
        debuffList[4] = true;
        iStuckCounter = 1;
        SUIManager.GetComponent<InStageUIManager>().debuffStuck.SetActive(true); // GUI

        while (iStuckCounter > 0)
        {
            yield return new WaitForSeconds(1f);
            iStuckCounter--;
        }
        debuffList[4] = false;
        SUIManager.GetComponent<InStageUIManager>().debuffStuck.SetActive(false); // GUI
    }
}
