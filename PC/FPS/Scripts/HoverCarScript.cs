using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCarScript : MonoBehaviour
{
    [Header("Scripts")]
    public FPSControllerLPFP.FpsControllerLPFP fpsControllerLPFP;
    public UI_Car_Controller uiController;

    [Header("GameObjects")]
    public GameObject JetCar;
    public GameObject Player;

    public GameObject canvasUI;

    public GameObject FLWheel;
    public GameObject FRWheel;
    public GameObject RLWheel;
    public GameObject RRWheel;

    public GameObject SteerlignWheel;

    public GameObject FrontLights;

    public GameObject InteriorLight;

    public GameObject Hyroscope;

    public GameObject FirstCamera;
    public GameObject SecondCamera;

    [Header("Particles")]
    public GameObject particleEngineHalfBreaking;

    public GameObject particleEngineSmoke;

    Rigidbody rbJetCar;

    [Header("Transform")]
    public Transform pointExit;

    [Header("Booleans")]
    public bool showInstructionOnSeat;
    public bool inZoneEnter;
    public bool isCanEnterVehicle;
    public bool isFirstCamera;
    public bool isPlayerInCar = false;
    public bool isTurnEngine;
    public bool isFlying;
    public bool isLightOn;
    public bool isInteriorLightOn;
    public bool isBroken;

    [Header("Parameters")]
    public float drag;
    public float minDrag;
    public float maxDrag;
    private float currentDrag;
    [Space(height: 5f)]

    public float angDrag;
    public float minAngDrag;
    public float maxAngDrag;
    [Space(height: 5f)]

    public float maxHPCar;
    public float currentHpCar;
    [Range(0, 100)]
    public float percentCarDamage;
    [Space(height: 5f)]

    public float hoverHeight;
    [Space(height: 5f)]

    public float strengthCarRotationY;
    public float strengthCarRotationZ;
    public float strengthCarRotationX;
    [Space(height: 5f)]

    public float strengthMainEngine;
    public float strengthOtherEngine;
    public float strengthForwardEngine;
    public float strengthHyroscope;
    public float strengthFlyingHyroscope;
    public float strGravity;
    public float angularSteerlingWheels;
    [Space(height: 5f)]


    [Header("Other")]
    public LayerMask layerMask;
    public ForceMode forceMode;

    [Header("Inputs")]

    public KeyCode KeyInCar;
    public KeyCode keyOutCar;

    float inputX;
    float inputY;

    float inputMouseX;
    float inputMouseY;

    bool isKeyE;
    bool isKeySpace;
    bool isKeyLCtrl;
    bool isKeyC;

    //Сделать посадку игрока в машину

    //дым только при заведенном двигателе

    // разбить на функции

    // Start is called before the first frame update
    void Awake()
    {
        rbJetCar = JetCar.GetComponent<Rigidbody>();

        FirstCamera.SetActive(isFirstCamera);
        SecondCamera.SetActive(!isFirstCamera);

        canvasUI.SetActive(isPlayerInCar);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInCar)
        {
            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical");

            inputMouseX = Input.GetAxis("Mouse X");
            inputMouseY = Input.GetAxis("Mouse Y");

            if (Input.GetKeyDown(KeyCode.C))
            {
                _ChangeCamera();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                isFlying = !isFlying;
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                isTurnEngine = !isTurnEngine;
            }
            if (Input.GetKeyDown(KeyCode.L) && !Input.GetKey(KeyCode.LeftControl))
            {
                isLightOn = !isLightOn;
            }
            if (Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.LeftControl))
            {
                isInteriorLightOn = !isInteriorLightOn;
            }
        }
        else
        {
            inputX = 0;
            inputY = 0;

            inputMouseX = 0;
            inputMouseY = 0;
        }

        _SitAndOut();

        if (isInteriorLightOn && !InteriorLight.activeSelf)
        {
            InteriorLight.SetActive(isInteriorLightOn);
        }
        else if (!isInteriorLightOn && InteriorLight.activeSelf)
        {
            InteriorLight.SetActive(isInteriorLightOn);
        }

        if (isLightOn && !FrontLights.activeSelf)
        {
            FrontLights.SetActive(isLightOn);
        }
        else
        {
            if (!isLightOn && FrontLights.activeSelf)
            {
                FrontLights.SetActive(isLightOn);
            }
        }

        //_CameraRotation();


        SteerlignWheel.transform.localEulerAngles = new Vector3(SteerlignWheel.transform.localEulerAngles.x, SteerlignWheel.transform.localEulerAngles.y, -inputX * angularSteerlingWheels * 12);

        FLWheel.transform.localEulerAngles = new Vector3(inputY * angularSteerlingWheels, FLWheel.transform.localEulerAngles.y, inputX * -angularSteerlingWheels);
        FRWheel.transform.localEulerAngles = new Vector3(inputY * angularSteerlingWheels, FRWheel.transform.localEulerAngles.y, inputX * -angularSteerlingWheels);
        RLWheel.transform.localEulerAngles = new Vector3(inputY * angularSteerlingWheels, RLWheel.transform.localEulerAngles.y, inputX * angularSteerlingWheels);
        RRWheel.transform.localEulerAngles = new Vector3(inputY * angularSteerlingWheels, RRWheel.transform.localEulerAngles.y, inputX * angularSteerlingWheels);

    }

    private void FixedUpdate()
    {

        if (!isBroken && isTurnEngine)
        {
            //
            _Raycasting(JetCar.transform, -Vector3.up, strengthMainEngine, forceMode, layerMask, true);
            _Raycasting(FLWheel.transform, -Vector3.up, strengthOtherEngine, forceMode, layerMask, true);
            _Raycasting(FRWheel.transform, -Vector3.up, strengthOtherEngine, forceMode, layerMask, true);
            _Raycasting(RLWheel.transform, -Vector3.up, strengthOtherEngine, forceMode, layerMask, true);
            _Raycasting(RRWheel.transform, -Vector3.up, strengthOtherEngine, forceMode, layerMask, true);

            if (isFlying)
            {
                rbJetCar.AddForceAtPosition(Vector3.up * strengthFlyingHyroscope, Hyroscope.transform.position, forceMode);
                rbJetCar.AddTorque(JetCar.transform.right * Input.GetAxis("Vertical Arrows") * strengthCarRotationX, ForceMode.Acceleration);
                currentDrag = (rbJetCar.velocity.normalized - JetCar.transform.forward.normalized).magnitude;
            }
            else
            {
                rbJetCar.AddForceAtPosition(Vector3.up * strengthHyroscope, Hyroscope.transform.position, forceMode);
                currentDrag = ((rbJetCar.velocity.normalized.x - JetCar.transform.forward.normalized.x)
                + (rbJetCar.velocity.normalized.z - JetCar.transform.forward.normalized.z));


            }

            if (currentDrag > 0)
            {
                rbJetCar.drag = minDrag + currentDrag * drag;
            }
            else rbJetCar.drag = minDrag - currentDrag * drag;
            rbJetCar.drag = Mathf.Clamp(rbJetCar.drag, minDrag, maxDrag);



            rbJetCar.angularDrag = minAngDrag + rbJetCar.velocity.magnitude * 0.025f;
            rbJetCar.angularDrag = Mathf.Clamp(rbJetCar.angularDrag, minAngDrag, maxAngDrag);




            rbJetCar.AddForce(JetCar.transform.forward * strengthForwardEngine * inputY, ForceMode.Acceleration);
            rbJetCar.AddForce(JetCar.transform.right * strengthForwardEngine * inputX * 0.1f, ForceMode.Acceleration);

            rbJetCar.AddTorque(JetCar.transform.up * (strengthCarRotationY - Mathf.Clamp(rbJetCar.velocity.magnitude * 0.1f, 0, 2)) * inputX, ForceMode.Acceleration);
            rbJetCar.AddTorque(JetCar.transform.forward * strengthCarRotationZ * -inputX, ForceMode.Acceleration);
        }

        if (!isFlying || isBroken || !isTurnEngine)
        {
            if (isBroken)
            {
                rbJetCar.AddForce(-Vector3.up * strGravity * 5, ForceMode.Acceleration);
            }
            else rbJetCar.AddForce(-Vector3.up * strGravity, ForceMode.Acceleration);
        }
    }


    public RaycastHit _Raycasting(Transform rPosition, Vector3 rRotation, float strengthHover, ForceMode fMode, LayerMask layer, bool isGlobal)
    {
        RaycastHit hit;
        //сам луч, начинается от позиции этого объекта и направлен в сторону цели
        Ray ray = new Ray(rPosition.position, rRotation);
        //пускаем луч
        Physics.Raycast(ray, out hit, hoverHeight, layer);
        if (hit.collider != null)
        {
            if (isGlobal)
            {
                rbJetCar.AddForceAtPosition(-rRotation * strengthHover * (1.0f - (hit.distance / hoverHeight)), rPosition.position, fMode);
            }
            else rbJetCar.AddForceAtPosition(JetCar.transform.up * strengthHover * (1.0f - (hit.distance / hoverHeight)), rPosition.position, fMode);
            Debug.DrawRay(rPosition.position, rRotation * hit.distance);
            return hit;
        }
        else
        {
            if (rPosition.position.y > JetCar.transform.position.y)
            {
                rbJetCar.AddForceAtPosition(rPosition.up * strengthHover * 0.5f, rPosition.position);
            }
            else rbJetCar.AddForceAtPosition(rPosition.up * -strengthHover * 0.5f, rPosition.position);
            Debug.DrawRay(rPosition.position, rRotation * hit.distance);
            return hit;
        }

    }


    //
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.magnitude *  0.01f > 3000 * 0.01f )
        {
            currentHpCar -= collision.impulse.magnitude * percentCarDamage * 0.01f;
        }
        if(currentHpCar < 500 && !isBroken)
        {
            isBroken = true;
        }
        currentHpCar = Mathf.Clamp(currentHpCar, 0, maxHPCar);
        if(currentHpCar < maxHPCar * 0.5f && !particleEngineHalfBreaking.activeSelf)
        {
            particleEngineHalfBreaking.SetActive(true);
        }
        //Debug.Log(currentHpCar);
    }


    //Если находится в зоне входа в транспорт
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            inZoneEnter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            inZoneEnter = false;
        }
    }


    //Вход / выход из транспорта
    public void _SitAndOut()
    {
        if (inZoneEnter && isCanEnterVehicle)
        {
            if (Input.GetKeyDown(KeyInCar))
            {
                inZoneEnter = false;

                isPlayerInCar = true;


                uiController.Instruction.SetActive(isPlayerInCar && showInstructionOnSeat);

                canvasUI.SetActive(isPlayerInCar);

                FirstCamera.SetActive(isPlayerInCar && isFirstCamera);
                SecondCamera.SetActive(isPlayerInCar &&!isFirstCamera);

                Player.SetActive(!isPlayerInCar);

                isCanEnterVehicle = false;
                StartCoroutine("Timer", 1f);
            }
        }

        if (isPlayerInCar && isCanEnterVehicle)
        {
            if (Input.GetKeyDown(keyOutCar))
            {
                isPlayerInCar = false;

                Player.SetActive(!isPlayerInCar);

                canvasUI.SetActive(isPlayerInCar);

                isFlying = false;

                FirstCamera.SetActive(isPlayerInCar && isFirstCamera);
                SecondCamera.SetActive(isPlayerInCar && !isFirstCamera);

                Player.transform.position = pointExit.position;

                isCanEnterVehicle = false;
                StartCoroutine("Timer", 1f);
            }
        }
    }

    public IEnumerator Timer(float t)
    {
        
        yield return new WaitForSeconds(t);
        isCanEnterVehicle = true;
    }

    public void _ChangeCamera()
    {
        isFirstCamera = !isFirstCamera;

        FirstCamera.SetActive(isFirstCamera);
        SecondCamera.SetActive(!isFirstCamera);
    }

    public void _Repair()
    {
        currentHpCar = maxHPCar;
        particleEngineHalfBreaking.SetActive(false);
    }

}
