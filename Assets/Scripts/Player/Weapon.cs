using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Weapon : MonoBehaviour
{
    public struct WeaponInfo
    {
        public float fattack;
        public float frate;
        public float frange;
        public float ffullAmmo;
        public float fcurrentAmmo;
    }

    public GameManager.WeaponIndexEnum weaponIndex = GameManager.WeaponIndexEnum.Pistol;
    public float fbulletSpeed = 200f;
    float calTime;

    WeaponInfo[] weapons = new WeaponInfo[8];
    public WeaponInfo currentWeapon = new WeaponInfo();

    public GameObject[] weaponObjects;
    public GameObject Character;
    public Transform firePoint;
    Ray fireRay;
    int layerMask;
    public GameObject bulletPrefab;
    public int bulletPoolSize = 80;
    bool[] bulletEnable = new bool[80];
    
    private List<GameObject> bulletObjects;

    Animator anim;
    public BoxCollider meleeAreaCrowbar;
    public BoxCollider meleeAreaLightsaber;
    public TrailRenderer trailEffectCrowbar;
    public TrailRenderer trailEffectLightsaber;

    GameObject UIManager; // HUD 총알 수 체크를 위한 오브젝트
    // Start is called before the first frame update

    private void Awake()
    {
        anim = Character.GetComponentInChildren<Animator>();
        bulletObjects = new List<GameObject>();
        UIManager = GameObject.Find("InStageUIManager"); // HUD 총알 수 체크를 위한 오브젝트

        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.GetComponent<Bullet>().bulletIdx = i;
            obj.SetActive(false);
            bulletObjects.Add(obj);
            bulletEnable[i] = true;
        }

        weapons[0].fattack = 0f;
        weapons[0].frate = 0f;
        weapons[0].frange = 0f;
        weapons[0].ffullAmmo = 0f;
        weapons[0].fcurrentAmmo = 0f;
        weapons[1].fattack = 10f;
        weapons[1].frate = 0.5f;
        weapons[1].frange = 1f;
        weapons[1].ffullAmmo = 9999f;
        weapons[1].fcurrentAmmo = 9999f;
        weapons[2].fattack = 10f;
        weapons[2].frate = 0.25f;
        weapons[2].frange = 2f;
        weapons[2].ffullAmmo = 150f;
        weapons[2].fcurrentAmmo = 150f;
        weapons[3].fattack = 5f;
        weapons[3].frate = 1f;
        weapons[3].frange = 1f;
        weapons[3].ffullAmmo = 20f;
        weapons[3].fcurrentAmmo = 20f;
        weapons[4].fattack = 5f;
        weapons[4].frate = 0.125f;
        weapons[4].frange = 1f;
        weapons[4].ffullAmmo = 250f;
        weapons[4].fcurrentAmmo = 250f;
        weapons[5].fattack = 50f;
        weapons[5].frate = 2f;
        weapons[5].frange = 5f;
        weapons[5].ffullAmmo = 20f;
        weapons[5].fcurrentAmmo = 20f;
        weapons[6].fattack = 50f;
        weapons[6].frate = 0.5f;
        weapons[6].frange = 1f;
        weapons[6].ffullAmmo = 100f;
        weapons[6].fcurrentAmmo = 100f;
        weapons[7].fattack = 100f;
        weapons[7].frate = 1f;
        weapons[7].frange = 1f;
        weapons[7].ffullAmmo = 50f;
        weapons[7].fcurrentAmmo = 50f;
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        for (int idx = 0; idx < bulletPoolSize; idx++)
        {
            if (!bulletObjects[idx].activeInHierarchy && bulletEnable[idx])
            {
                var obj = bulletObjects[idx];
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                bulletEnable[idx] = false;
                return obj;
            }
        }

        return null;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
        StartCoroutine(SetOnBullet(obj.GetComponent<Bullet>().bulletIdx));
    }

    // 같은 총알을 바로 다시 사용할 경우 TrailRender에 의한 이상 이펙트가 발생. 해결하기 위한 지연 코루틴.
    IEnumerator SetOnBullet(int idx)
    {
        yield return new WaitForSeconds(0.75f);
        bulletEnable[idx] = true;
    }

    public void weaponChange(GameManager.WeaponIndexEnum idx)
    {
        weaponObjects[(int)weaponIndex].SetActive(false);
        weaponIndex = idx;
        weaponObjects[(int)weaponIndex].SetActive(true);
        currentWeapon = weapons[(int)weaponIndex];

        if (weaponIndex == GameManager.WeaponIndexEnum.Pistol)
        {
            UIManager.GetComponent<InStageUIManager>().bulletText.text = "∞";
        }
        else
        {
            UIManager.GetComponent<InStageUIManager>().bulletText.text = currentWeapon.fcurrentAmmo.ToString();
        }

        if (Character.GetComponent<Character>().debuffList[0] == true)
            currentWeapon.frate *= 1.25f;
    }

    public void DoAttack()
    {
        if (weaponIndex == GameManager.WeaponIndexEnum.Pistol)
        {
            oneShot();
            anim.SetTrigger("doShot");
        }
        else if (weaponIndex == GameManager.WeaponIndexEnum.Rifle)
        {
            oneShot();
            anim.SetTrigger("doShot");
            currentWeapon.fcurrentAmmo--;
        }
        else if (weaponIndex == GameManager.WeaponIndexEnum.Shotgun)
        {
            multiShot();
            anim.SetTrigger("doShot");
            currentWeapon.fcurrentAmmo--;
        }
        else if (weaponIndex == GameManager.WeaponIndexEnum.Machinegun)
        {
            oneShot();
            anim.SetTrigger("doShot");
            currentWeapon.fcurrentAmmo--;
        }
        else if (weaponIndex == GameManager.WeaponIndexEnum.Sniper)
        {
            oneShot();
            anim.SetTrigger("doShot");
            currentWeapon.fcurrentAmmo--;
        }
        else if (weaponIndex == GameManager.WeaponIndexEnum.Crowbar)
        {
            StopCoroutine(SwingCrowbar());
            StartCoroutine(SwingCrowbar());
            anim.SetTrigger("doSwing");
            currentWeapon.fcurrentAmmo--;
        }
        else if (weaponIndex == GameManager.WeaponIndexEnum.Lightsaber)
        {
            StopCoroutine(SwingLightsaber());
            StartCoroutine(SwingLightsaber());
            anim.SetTrigger("doSwing");
            currentWeapon.fcurrentAmmo--;
        }

        if (weaponIndex == GameManager.WeaponIndexEnum.Pistol)
        {
            UIManager.GetComponent<InStageUIManager>().bulletText.text = "∞";
        }
        else
        {
            UIManager.GetComponent<InStageUIManager>().bulletText.text = currentWeapon.fcurrentAmmo.ToString();
        }
    }

    Ray GetFireDir(int dirNum)
    {
        Vector3 getDir;
        float fdist;
        layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 500f, layerMask))
        {
            getDir = hit.point - firePoint.position;
            fdist = hit.distance;
        }
        else
        {
            getDir = ray.GetPoint(500f) - firePoint.position;
            fdist = 500f;
        }
            

        if (dirNum == 1)
            getDir += firePoint.up * 1.5f * fdist/500f;
        else if (dirNum == 2)
            getDir += (firePoint.up - firePoint.right).normalized * 1.5f * fdist / 500f;
        else if (dirNum == 3)
            getDir -= firePoint.right * 1.5f * fdist / 500f;
        else if (dirNum == 4)
            getDir -= (firePoint.right + firePoint.up).normalized * 1.5f * fdist / 500f;
        else if (dirNum == 5)
            getDir -= firePoint.up * 1.5f * fdist / 500f;
        else if (dirNum == 6)
            getDir += (firePoint.right - firePoint.up).normalized * 1.5f * fdist / 500f;
        else if (dirNum == 7)
            getDir += firePoint.right * 1.5f * fdist / 500f;
        else if (dirNum == 8)
            getDir += (firePoint.right + firePoint.up).normalized * 1.5f * fdist / 500f;
         
        return new Ray(firePoint.position, getDir);
    }

    void oneShot()
    {
        GameObject bullet = GetObject(firePoint.position, firePoint.rotation);
        fireRay = GetFireDir(0);

        if (bullet != null)
        { 
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
        }
    }

    void multiShot()
    {
        GameObject bullet1 = GetObject(firePoint.position, firePoint.rotation);
        GameObject bullet2 = GetObject(firePoint.position, firePoint.rotation);
        GameObject bullet3 = GetObject(firePoint.position, firePoint.rotation);
        GameObject bullet4 = GetObject(firePoint.position, firePoint.rotation);
        GameObject bullet5 = GetObject(firePoint.position, firePoint.rotation);
        GameObject bullet6 = GetObject(firePoint.position, firePoint.rotation);
        GameObject bullet7 = GetObject(firePoint.position, firePoint.rotation);
        GameObject bullet8 = GetObject(firePoint.position, firePoint.rotation);

        if ((bullet1 != null)&& (bullet2 != null) && (bullet3 != null) && (bullet4 != null) && (bullet5 != null) && (bullet6 != null) && (bullet7 != null) && (bullet8 != null))
        {

            fireRay = GetFireDir(1);
            Bullet bulletScript1 = bullet1.GetComponent<Bullet>();
            bulletScript1.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
            fireRay = GetFireDir(2);
            Bullet bulletScript2 = bullet2.GetComponent<Bullet>();
            bulletScript2.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
            fireRay = GetFireDir(3);
            Bullet bulletScript3 = bullet3.GetComponent<Bullet>();
            bulletScript3.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
            fireRay = GetFireDir(4);
            Bullet bulletScript4 = bullet4.GetComponent<Bullet>();
            bulletScript4.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
            fireRay = GetFireDir(5);
            Bullet bulletScript5 = bullet5.GetComponent<Bullet>();
            bulletScript5.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
            fireRay = GetFireDir(6);
            Bullet bulletScript6 = bullet6.GetComponent<Bullet>();
            bulletScript6.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
            fireRay = GetFireDir(7);
            Bullet bulletScript7 = bullet7.GetComponent<Bullet>();
            bulletScript7.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
            fireRay = GetFireDir(8);
            Bullet bulletScript8 = bullet8.GetComponent<Bullet>();
            bulletScript8.Initialize(currentWeapon.fattack, fbulletSpeed, fireRay);
        }
    }

    IEnumerator SwingCrowbar()
    {
        yield return new WaitForSeconds(0.1f);
        meleeAreaCrowbar.enabled = true;
        trailEffectCrowbar.enabled = true;

        yield return new WaitForSeconds(0.5f);
        meleeAreaCrowbar.enabled = false;
        trailEffectCrowbar.enabled = false;
    }

    IEnumerator SwingLightsaber()
    {
        yield return new WaitForSeconds(0.1f);
        meleeAreaLightsaber.enabled = true;
        trailEffectLightsaber.enabled = true;

        yield return new WaitForSeconds(0.5f);
        meleeAreaLightsaber.enabled = false;
        trailEffectLightsaber.enabled = false;
    }

    void Start()
    {
        weaponChange(GameManager.WeaponIndexEnum.Pistol);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentWeapon.fcurrentAmmo == 0f)
            weaponChange(GameManager.WeaponIndexEnum.Pistol);

        calTime += Time.deltaTime;
        if (calTime >= 1f)
        {
            StageManager.Instance.heuristics.UpdateWeaponTime(weaponIndex);
            calTime = 0f;
        }
    }
}
