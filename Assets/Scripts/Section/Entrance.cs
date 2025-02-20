using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entrance : MonoBehaviour
{
    public Entrance linked_Entrance = null;
    public List<int> linked_section_pos = new List<int>();

    public Camera entranceCam = null;
    public GameObject entrancePlane = null;
    public List<Material> mat;
    public Vector3 wallDirection = new Vector3();   // 출입구-벽의 방향
    bool isPlayerNear = false;
    

    public void setWallDir(Vector3 wallDir)
    {
        int layerMask = 1 << LayerMask.NameToLayer("Terrain");
        wallDirection = wallDir;
        RaycastHit underHit;
        RaycastHit sideHit1, sideHit2, sideHit3, sideHit4;
        float[] dist = new float[4];
        float minDist, minDir;
        if (!Physics.Raycast(transform.position, Vector3.down, out underHit, 5f, layerMask))
        {
            Debug.Log("이상!");
            Debug.Log(transform.position);
            if (Physics.Raycast(transform.position + Vector3.back * 10f, Vector3.down, out underHit, 5f, layerMask))
            {
                Debug.Log("아래 방향으로 옮김!");
                transform.position += Vector3.back * 10f;
            }
            else if (Physics.Raycast(transform.position + Vector3.forward * 10f, Vector3.down, out underHit, 5f, layerMask))
            {
                Debug.Log("위 방향으로 옮김!");
                transform.position += Vector3.forward * 10f;
            }
            else if (Physics.Raycast(transform.position + Vector3.right * 10f, Vector3.down, out underHit, 5f, layerMask))
            {
                Debug.Log("오른쪽 방향으로 옮김!");
                transform.position += Vector3.right * 10f;
            }
            else if (Physics.Raycast(transform.position + Vector3.left * 10f, Vector3.down, out underHit, 5f, layerMask))
            {
                Debug.Log("왼쪽 방향으로 옮김!");
                transform.position += Vector3.left * 10f;
            }
        }
        transform.position += Vector3.down * 3f;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.forward, out sideHit1, 30f, layerMask))
            dist[0] = sideHit1.distance;
        else
            dist[0] = 10f;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.back, out sideHit2, 30f, layerMask))
            dist[1] = sideHit2.distance;
        else
            dist[1] = 10f;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.left, out sideHit3, 30f, layerMask))
            dist[2] = sideHit3.distance;
        else
            dist[2] = 10f;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.right, out sideHit4, 30f, layerMask))
            dist[3] = sideHit4.distance;
        else
            dist[3] = 10f;
        minDist = dist[0];
        minDir = 0;
        for (int i = 0; i < 4; i++)
        {
            if (dist[i] < minDist)
            {
                minDist = dist[i];
                minDir = i;
            }
        }
        if (minDir == 0)
            wallDirection = Vector3.forward;
        else if (minDir == 1)
            wallDirection = Vector3.back;
        else if (minDir == 2)
            wallDirection = Vector3.left;
        else
            wallDirection = Vector3.right;

        this.gameObject.transform.rotation = Quaternion.LookRotation(-wallDirection);
        changeDistanceOfPlane();
    }

    /* Functions */
    public void linkEntrance(Entrance pair_entrance)
    {
        // (*)
        if (linked_Entrance != null) return;

        /* linker가 연결할 Entrance로 하여 정상적으로 모든 것이 작동할 수 있도록 한다. */
        /* 이 함수는 RoomObjectGenerator에서 Entrance를 배치할 때 호출된다. */
        linked_Entrance = pair_entrance;

        // 아래 코드는 무한 재귀 함수 호출함. 위 (*) 라인과 같이 수정
        pair_entrance.linkEntrance(this);
    }

    void changeDistanceOfPlane()
    {
        RaycastHit hit;
        RaycastHit hit1, hit2;
        int layerMask = 1 << LayerMask.NameToLayer("Terrain");
        float wallDist;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, wallDirection, out hit, 10f, layerMask))
        {
            wallDist = hit.distance * 0.99f;
        }
        else
        {
            wallDist = 5f;
        }
        entranceCam.gameObject.transform.localPosition = new Vector3(0, 5, -wallDist);
        entrancePlane.gameObject.transform.localPosition = new Vector3(0, 5, -wallDist);

        if ((wallDirection.x > 0) && (wallDirection.z == 0))
        {
            if (Physics.Raycast(transform.position + Vector3.up * 5f + Vector3.forward * 0.5f, wallDirection, out hit1, 10f, layerMask)
                && Physics.Raycast(transform.position + Vector3.up * 5f - Vector3.forward * 0.5f, wallDirection, out hit2, 10f, layerMask))
            {
                if (hit1.distance > hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 45, 0));
                else if (hit1.distance < hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, -45, 0));
            }
        }
        else if ((wallDirection.x < 0) && (wallDirection.z == 0))
        {
            if (Physics.Raycast(transform.position + Vector3.up * 5f + Vector3.forward * 0.5f, wallDirection, out hit1, 10f, layerMask)
                && Physics.Raycast(transform.position + Vector3.up * 5f - Vector3.forward * 0.5f, wallDirection, out hit2, 10f, layerMask))
            {
                if (hit1.distance > hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, -45, 0));
                else if (hit1.distance < hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 45, 0));
            }
        }
        else if ((wallDirection.x == 0) && (wallDirection.z > 0))
        {
            if (Physics.Raycast(transform.position + Vector3.up * 5f + Vector3.right * 0.5f, wallDirection, out hit1, 10f, layerMask)
                && Physics.Raycast(transform.position + Vector3.up * 5f - Vector3.right * 0.5f, wallDirection, out hit2, 10f, layerMask))
            {
                if (hit1.distance > hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, -45, 0));
                else if (hit1.distance < hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 45, 0));
            }
        }
        else if ((wallDirection.x == 0) && (wallDirection.z < 0))
        {
            if (Physics.Raycast(transform.position + Vector3.up * 5f + Vector3.right * 0.5f, wallDirection, out hit1, 10f, layerMask)
                && Physics.Raycast(transform.position + Vector3.up * 5f - Vector3.right * 0.5f, wallDirection, out hit2, 10f, layerMask))
            {
                if (hit1.distance > hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 45, 0));
                else if (hit1.distance < hit2.distance)
                    entrancePlane.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, -45, 0));
            }
        }
    }

    void changeEntranceImage()
    {
        isPlayerNear = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 20f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                isPlayerNear = true;
            }
        }
        if (isPlayerNear)
            linked_Entrance.entranceCam.depth = 10;
        else
            linked_Entrance.entranceCam.depth = 0;
    }

    private void Start()
    {
        this.gameObject.transform.rotation = Quaternion.LookRotation(-wallDirection);
        changeDistanceOfPlane();
    }

    private void Update()
    {
        if (linked_Entrance == null)
        {
            entrancePlane.GetComponent<MeshRenderer>().material = mat[0];
        }
        else
        {
            entrancePlane.GetComponent<MeshRenderer>().material = mat[1];
            changeEntranceImage();
        }
    }
}
