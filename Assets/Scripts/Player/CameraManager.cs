using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{
    public float ffollowSpeed = 20f;
    public float fsensitivity = 400f;
    public float fclampAngle = 70f;
    public float fminDistance;
    public float fmaxDistance;
    public float ffinalDistance;
    public float fsmoothness = 10f;

    private float frotX;
    private float frotY;

    bool stuck;

    public Transform objectTofollow;
    public Transform realCamera;
    public Vector3 dirNormalized;
    public Vector3 finalDir;

    GameObject player;

    private void Awake()
    {
        player = GameObject.Find("Character");
        objectTofollow = player.transform;
    }
    private void Start()
    {
        frotX = transform.localRotation.eulerAngles.x;
        frotY = transform.localRotation.eulerAngles.y;

        dirNormalized = realCamera.localPosition.normalized;
        ffinalDistance = realCamera.localPosition.magnitude;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void Update()
    {
        stuck = player.GetComponent<Character>().debuffList[4];
        if (!stuck)
        {
            mouseMove();
        }

    }

    private void LateUpdate()
    {
        if (!stuck)
        {
            cameraMove();
        }

    }

    public void changeRot(Vector3 vt)
    {
        frotX = vt.x;
        frotY = vt.y;
    }

    void mouseMove()
    {
        frotX += -(Input.GetAxis("Mouse Y")) * fsensitivity * Time.deltaTime;
        frotY += Input.GetAxis("Mouse X") * fsensitivity * Time.deltaTime;

        frotX = Mathf.Clamp(frotX, -fclampAngle, fclampAngle);
        Quaternion rot = Quaternion.Euler(frotX, frotY, 0);
        transform.rotation = rot;
    }

    void cameraMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, objectTofollow.position, ffollowSpeed * Time.deltaTime);
        finalDir = transform.TransformPoint(dirNormalized * fmaxDistance);

        RaycastHit hit;
        if (Physics.Linecast(transform.position, finalDir, out hit))
        {
            ffinalDistance = Mathf.Clamp(hit.distance, fminDistance, fmaxDistance);
        }
        else
        {
            ffinalDistance = fmaxDistance;
        }
        realCamera.localPosition = Vector3.Lerp(realCamera.localPosition, dirNormalized * ffinalDistance, Time.deltaTime * fsmoothness);
        
    }
}
