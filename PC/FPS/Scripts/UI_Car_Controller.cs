using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Car_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Script")]
    public HoverCarScript hoverCarScript;

    [Header("Icons")]
    public Image ImageEngine;
    public Image ImageLights;
    public GameObject ImageFlying;

    [Header("Text")]
    public GameObject Instruction;
    public GameObject ShowInstructionKey;

    [Header("Booleans")]
    public bool isShowInstructions;

    [Header("Colors")]
    public Color colorRed;
    public Color colorWhite;
    public Color colorBlack;

    [Header("Input")]
    public KeyCode KeyInstrucion;


    void Start()
    {
        Instruction.SetActive(isShowInstructions);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyInstrucion) && hoverCarScript.isPlayerInCar)
        {
            _Instruction();
        }
        //
        if (hoverCarScript.isTurnEngine && !hoverCarScript.isBroken && ImageEngine.color != colorWhite)
        {
            ImageEngine.color = colorWhite;
        }
        if (hoverCarScript.isBroken && ImageEngine.color != colorRed)
        {
            ImageEngine.color = colorRed;
        }
        if (!hoverCarScript.isTurnEngine && !hoverCarScript.isBroken && ImageEngine.color != colorBlack)
        {
            ImageEngine.color = colorBlack;
        }

        //
        if (hoverCarScript.isLightOn && ImageLights.color != colorWhite)
        {
            ImageLights.color = colorWhite;
        }
        else if (!hoverCarScript.isLightOn && ImageLights.color != colorBlack)
        {
            ImageLights.color = colorBlack;
        }

        //
        if(hoverCarScript.isFlying != ImageFlying.activeSelf)
        {
            ImageFlying.SetActive(hoverCarScript.isFlying);
        }
    }

    void _Instruction()
    {
        isShowInstructions = !isShowInstructions;
        if (isShowInstructions && !Instruction.activeSelf)
        {
            Instruction.SetActive(isShowInstructions);
        }
        else if(!isShowInstructions && Instruction.activeSelf)
        {
            Instruction.SetActive(isShowInstructions);
        }
    }
}
