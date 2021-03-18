using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    public PlayerController playerScript;
    public Counts countsScript;
    public MenuScript menuScript;
    
    [Header("Objects")]
    [SerializeField]
    protected GameObject Pipe;
    [SerializeField]
    protected GameObject BeforePipe;
    [SerializeField]
    protected GameObject AfterPipe;

    private Rigidbody2D rbPipe;

    private bool isContact = false;

    private float contactYposition;

    [Header("Parameters")]
    [SerializeField]
    protected float speedMoving;
    [SerializeField]
    protected float startRangeY;
    [SerializeField]
    protected float maxRangeY;
    [SerializeField]
    protected float startPoint;
    [SerializeField]
    protected float endPoint;
    [SerializeField]
    protected float maxHeight;
    [SerializeField]
    protected float MinHeight;
    private float currentRangeY;

    [Header("Parameters Prefab")]
    public Quaternion tfRotation;

    private void Start()
    {
        //rbPipe.velocity = new Vector2(-speedMoving, 0);
        rbPipe = Pipe.GetComponent<Rigidbody2D>();
        currentRangeY = startRangeY;
    }

    // Update is called once per frame
    void Update()
    {
 
        //Перенос в начальную позицию и присваивается рандомное значение по Y в заданном диапазоне, если заходит за опеределенные координаты
        if(Pipe.transform.position.x < endPoint)
        {
            if (menuScript.isPlay)
            {
                Pipe.transform.position = new Vector2(startPoint, Random.Range(MinHeight, maxHeight));
                if (isContact)
                {
                    Pipe.transform.GetChild(0).transform.eulerAngles = new Vector3(0, 0, 0);
                    Pipe.transform.GetChild(1).transform.eulerAngles = new Vector3(0, 0, 0);
                    isContact = false;
                }
            }
            else
            {
                Pipe.transform.position = new Vector2(startPoint, 0);
            }

        }

        //Остановка после проигрыша
        if (playerScript.isGameOver)
        {
            speedMoving = 0;
        }

        //расчет расстояния между трубами по Y 
        if(currentRangeY < maxRangeY)
        {
            currentRangeY = startRangeY + countsScript.numberLateCounts * 0.02f;
        }

        //если расстояние по Y между трубами больше текущего максимального расстояния, выполняет рандом заного
        if(BeforePipe.transform.position.y - Pipe.transform.position.y > currentRangeY && Pipe.transform.position.x > 12 && menuScript.isPlay == true)
        {
            Pipe.transform.position = new Vector2(Pipe.transform.position.x, Random.Range(MinHeight, maxHeight));
        }
    }

    void FixedUpdate()
    {

        //перемещение влево труб с помощью физики
        rbPipe.velocity = new Vector2(-speedMoving, 0);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isContact = true;
        contactYposition = collision.GetContact(0).point.y;
        //Debug.Log(collision.GetContact(0).point.y - collision.transform.position.y);
    }
}
