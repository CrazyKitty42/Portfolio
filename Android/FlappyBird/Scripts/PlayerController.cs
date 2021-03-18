using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public MenuScript menuScript;

    private GameObject Player;

    [Header("GameObjects")]
    public GameObject ObjectForLooking;
    public GameObject EngineParticles;
    public GameObject SpriteObject;
    public GameObject PointForFire;

    private SpriteRenderer spriteRendererPlayer;

    private Rigidbody2D rbPlayer;

    [HideInInspector]
    public bool isGameOver = false;

    [Header("Material")]
    public Material playerMaterial;

    [Header("Boolean")]
    public bool isImpulse;
    private bool isInvincible = false;
    private bool isContact = false;

    [Header("Parameters")]
    [SerializeField]
    protected int maxLives;
    [SerializeField]
    protected float timeForInvincible;
    [SerializeField]
    protected float speedFlashingInInvincible;
    [SerializeField]
    protected float gravityScale;
    private float timeLast;
    private int currentLives;
    public int jumpForce;
    public int maxSpeed;

    private int currentLevelHeal;
    private int nextLevelHeal;

    [Header("Prefabs")]
    public GameObject explosionEffectFireworksPrefab;
    public GameObject explosionEffectPrefab;
    public GameObject hitEffectPrefab;
    public GameObject fireEffectPrefab;
    public GameObject healPrefab;
    public GameObject healUpPrefab;

    [Header("Prefab Parameters")]
    public RigidbodyConstraints2D rbPrefabConstaint2D;

    [Header("Sounds")]
    public AudioSource sPickUpMonet;
    public AudioSource sPickUpLife;
    public AudioSource sHit;
    public AudioSource sExplosion;

    [Header("Animators")]
    public Animator animatorEngine1;
    public Animator animatorEngine2;
    public Animator animatorAlpha;

    //[Header("Animations")]
    //public AnimationClip animationEngine;


    //Создание сохранения с текущим уровнем ХП и рекордом 
    //Создать предметы для повышения и восстановления ХП

    private void Awake()
    {
        Player = this.gameObject;

        spriteRendererPlayer = Player.GetComponent<SpriteRenderer>();

        rbPlayer = Player.GetComponent<Rigidbody2D>();
        rbPlayer.gravityScale = 0;

        currentLives = maxLives;
        menuScript.textLives.text = "Lives: " + currentLives;
    }


    // Update is called once per frame
    void Update()
    {
        PointForFire.transform.position = Player.transform.position;

        if (menuScript.isPlay)
        {
            rbPlayer.gravityScale = gravityScale;
        }
        //Ограничение скорости полета по Y
        if(rbPlayer.velocity.y > maxSpeed)
        {
            rbPlayer.velocity = new Vector2(0, Mathf.Clamp(maxSpeed, -maxSpeed, maxSpeed - 1));
        }

        if (!isGameOver)
        {
            SpriteObject.transform.LookAt(new Vector2(10, rbPlayer.velocity.y), Vector3.up);
        }

        if (isInvincible && !isGameOver && !animatorAlpha.GetCurrentAnimatorStateInfo(0).IsName("Invincible"))
        {
            animatorAlpha.Play("Invincible");
        }
        else animatorAlpha.StopPlayback();

        _TimerInvincible();
        //_InvincibleFlashing();

        //Debug.Log(playerMaterial.color.a);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //проверка столкновений с объектом
        isContact = true;
        if (!isInvincible)
        {
            if (menuScript.isPlay)
            {
                if (currentLives > 0)
                {
                    currentLives -= 1;
                }
                menuScript.textLives.text = "Lives: " + currentLives;

                if (currentLives > 0)
                {
                    Instantiate(hitEffectPrefab, collision.GetContact(0).point, new Quaternion(0, 0, 0, 0));
                    sHit.Play();
                }
                else
                {
                    Instantiate(hitEffectPrefab, collision.GetContact(0).point, new Quaternion(0, 0, 0, 0));
                    Instantiate(explosionEffectPrefab, Player.transform.position, new Quaternion(0, 0, 0, 0));
                    Instantiate(explosionEffectFireworksPrefab, Player.transform.position, new Quaternion(0, 0, 0, 0));
                    Instantiate(fireEffectPrefab, Player.transform.position, new Quaternion(0, 0, 0, 0), PointForFire.transform);
                    rbPlayer.constraints = rbPrefabConstaint2D;
                    rbPlayer.AddTorque(120);
                    EngineParticles.SetActive(false);
                    sExplosion.Play();
                    //Destroy(explosionEffectPrefab, 1);
                }
            }
            

            if(collision.gameObject.tag == "Pipe")
            {
                if(Player.transform.position.y > collision.transform.position.y)
                {
                    collision.transform.GetChild(0).eulerAngles = new Vector3(collision.transform.rotation.x, collision.transform.rotation.y, collision.transform.rotation.z + 5);
                }
                else if(Player.transform.position.y < collision.transform.position.y)
                {
                    collision.transform.GetChild(1).eulerAngles = new Vector3(collision.transform.rotation.x, collision.transform.rotation.y, collision.transform.rotation.z - 5);
                }
            }
        }
    }


    public void _JumpImpulse()
    {
        if(!isGameOver && menuScript.isPlay)
        {
            if (isImpulse)
            {
                rbPlayer.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else rbPlayer.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);

            animatorEngine1.StopPlayback();
            animatorEngine2.StopPlayback();
            animatorEngine1.Play("FireEngine");
            animatorEngine2.Play("FireEngine");
        }
    }

    private void _TimerInvincible()
    {
        if (currentLives <= 0)
        {
            isGameOver = true;
        }

        if (isContact && timeLast <= 0)
        {
            isInvincible = true;
            timeLast = timeForInvincible;
            isContact = false;
        }
        else
        {
            if (timeLast > 0)
            {
                timeLast -= Time.deltaTime;
            }
            isContact = false;
            if (timeLast <= 0)
            {
                isInvincible = false;
            }
        }
    }
}
