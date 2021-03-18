using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMoving : MonoBehaviour
{
    public PlayerController playerScript;

    [Header("Objects")]
    [SerializeField]
    protected GameObject FirstBackground;

    [Header("Parameters")]
    [SerializeField]
    protected float speedMoving;
    [SerializeField]
    protected float startPoint, endPoint;


    void Update()
    {//Перенос в начальную позицию, если заходит за опеределенные координаты
        if (FirstBackground.transform.localPosition.x < endPoint)
        {
            FirstBackground.transform.localPosition = new Vector2(startPoint, FirstBackground.transform.localPosition.y);
        }
        if (playerScript.isGameOver)
        {
            speedMoving = 0;
        }
    }

    void FixedUpdate()
    {//Перемещение влево бэкграунда
        FirstBackground.transform.position = new Vector2(Mathf.MoveTowards(FirstBackground.transform.position.x, endPoint, speedMoving), FirstBackground.transform.position.y);
        //FirstBackground.transform.localPosition = new Vector2(FirstBackground.transform.localPosition.x - speedMoving, FirstBackground.transform.localPosition.y);
    }
}
