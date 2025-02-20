using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float fattack = 5f;
    public float fbulletSpeed = 200f;
    float fliveTime = 2f;

    Ray fireRay;
    public GameObject weapon;
    public int bulletIdx;


    public void Initialize(float bulletAttack, float bulletSpeed, Ray ray)
    {
        fattack = bulletAttack;
        fbulletSpeed = bulletSpeed;
        fireRay = ray;
    }

    void bulletMove()
    {
        transform.position += fireRay.direction * fbulletSpeed * Time.deltaTime;
    }

    IEnumerator liveCounter()
    {
        fliveTime = 2f;
        while(fliveTime > 0)
        {
            yield return new WaitForSeconds(1f);
            fliveTime--;
        }
        weapon.GetComponent<Weapon>().ReturnObjectToPool(gameObject);
    }
    private void Awake()
    {
        weapon = GameObject.Find("Weapon");
    }
    private void OnEnable()
    {
        StartCoroutine(liveCounter());
    }
    void Update()
    {
        bulletMove();
    }

    private void OnTriggerEnter(Collider other)
    {
        weapon.GetComponent<Weapon>().ReturnObjectToPool(gameObject);
    }
}
