using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuScript : MonoBehaviour
{
    [Header("Scripts")]
    public PlayerController playerScript;

    [Header("GameObjects")]
    public GameObject MainMenu;

    [Header("Boolean")]
    public bool isPlay = false;
    public bool isPause = false;

    [Header("Text")]
    public Text textGameOver;
    public Text textLives;
    public Text textInstruction;

    [Header("Button")]
    public GameObject Button;

    [Header("Other")]
    public LayerMask layerMask;
    public Touch touch;

    [Header("Scene")]
    private Scene mainScene;

    private void Awake()
    {
        isPause = true;
        _Pause();
        textInstruction.gameObject.SetActive(true);
        textGameOver.gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        

        if(!isPlay)
        {
            textInstruction.gameObject.SetActive(true);
        }
        else if(isPlay && textInstruction.gameObject.activeSelf)
        {
            textInstruction.gameObject.SetActive(false);
        }

        textGameOver.gameObject.SetActive(playerScript.isGameOver);

        _PressButton();

    }

    public void _Pause()
    {
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0;
            MainMenu.SetActive(isPause);
        }
        else
        {
            Time.timeScale = 1;
            MainMenu.SetActive(isPause);
        }
    }

    public void _RestartGame()
    {
        //загрузка сцены заного, если игрок проиграл
        if (playerScript.isGameOver)
        {
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }

    public void _StartPlay()
    {
        isPlay = true;
    }

    public void _PressButton()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(Input.touchCount > 0)
            {
                if (Input.GetTouch(Input.touchCount - 1).phase == TouchPhase.Began && !isPause && !((Input.GetTouch(Input.touchCount - 1).position.x > (Screen.width - (Screen.width * 0.2f))) && (Input.GetTouch(Input.touchCount - 1).position.y > (Screen.height - (Screen.height * 0.3f)))))
                {
                    playerScript._JumpImpulse();
                    _StartPlay();
                    _RestartGame();
                }
                else if (!isPause)
                {
                    _Pause();
                }else if((Input.GetTouch(Input.touchCount - 1).position.x > (Screen.width - (Screen.width * 0.2f))) && (Input.GetTouch(Input.touchCount - 1).position.y > (Screen.height - (Screen.height * 0.3f))))
                {
                    _Pause();
                } Debug.Log(((Input.GetTouch(Input.touchCount - 1).position.x > (Screen.width - (Screen.width * 0.2f))) && (Input.GetTouch(Input.touchCount - 1).position.y > (Screen.height - (Screen.height * 0.3f)))));
            }
            //пофиксить паузу при рестарте в начале

        }
    }
}
