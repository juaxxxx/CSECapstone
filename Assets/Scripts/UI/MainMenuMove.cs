using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMove : MonoBehaviour
{
    public float fsmoothness = 10f;
    public RectTransform mainTransform;
    public RectTransform selectTransform;
    public Vector3 mainTarget = new Vector3(960f, 540f, 0f);
    public Vector3 selectTarget = new Vector3(2880f, 540f, 0f);
    public Image backGround;

    private void Update()
    {
        UImove();
    }

    void UImove()
    {
        mainTransform.position = Vector3.Lerp(mainTransform.position, mainTarget, Time.deltaTime * fsmoothness);
        selectTransform.position = Vector3.Lerp(selectTransform.position, selectTarget, Time.deltaTime * fsmoothness);
    }
}
