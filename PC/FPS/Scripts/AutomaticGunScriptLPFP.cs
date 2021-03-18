using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using FPSControllerLPFP;


//Изменить зум только при анимации прицеливания

// ----- Low Poly FPS Pack Free Version -----
public class AutomaticGunScriptLPFP : MonoBehaviour {

    public FpsControllerLPFP movingScript;

    //Animator component attached to weapon
    Animator anim;

    int numberObjects = 0;

    [SerializeField]
    private Transform tfForVertical;
    [SerializeField]
    private Transform tfForHorizontal;
    public Transform tfFlashlight;
    public Transform tfBulletSPR;
    public Transform tfCrosshairCanvas;
    public GameObject CrosshairUI;

    public float CrosshairSize;

    [Header("Gun Camera")]
    //Main gun camera
    public Camera gunCamera;
    public Camera rayCamera;

    [Header("Gun Camera Options")]
    //How fast the camera field of view changes when aiming 
    [Tooltip("How fast the camera field of view changes when aiming.")]
    public float fovSpeed = 15.0f;
    public float fovShootSpeed = 35.0f;
    //Default camera field of view
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    public float defaultFov = 75.0f;

    public float aimFov = 35.0f;

    [Header("UI Weapon Name")]
    [Tooltip("Name of the current weapon, shown in the game UI.")]
    public string weaponName;
    private string storedWeaponName;

    [Header("Weapon Sway")]
    //Enables weapon sway
    [Tooltip("Toggle weapon sway.")]
    public bool weaponSway;

    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmoothValue = 4.0f;

    private float inputX;
    private float inputY;

    private Vector3 initialSwayPositionArms;
    private Vector3 initialSwayPositionLight;
    private Vector3 initialSwayPositionCamera;

    private bool isPlayingOtherAnimations = false;

    //Used for fire rate
    private float lastFired;
    private bool FireType;
    [Header("Weapon Settings")]
    //How fast the weapon fires, higher value means faster rate of fire
    [Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
    public float fireRate;
    //Eanbles auto reloading when out of ammo
    [Tooltip("Enables auto reloading when out of ammo.")]
    public bool autoReload;
    //Delay between shooting last bullet and reloading
    public float autoReloadDelay;
    //Check if reloading
    public bool isReloading;
    //Holstering weapon
    private bool hasBeenHolstered = false;
    //If weapon is holstered
    public bool holstered;
    //Check if running
    public bool isRunning;
    //Check if aiming
    public bool isAiming;
    //Check if walking
    private bool isWalking;
    //Check if inspecting weapon
    private bool isInspecting;
    private bool isLight;
    public bool isSitting;
    private bool isShooting;
    private bool isGrenadeThrowing;
    private bool isRunningJump;
    private bool isJumping;

    

    Texture2D tex;

    //How much ammo is currently left in magazine
    private int currentMagazineAmmo;
    //How much ammo is currently left
    public int currentAmmo;
    //Totalt amount of ammo
    [Tooltip("How much ammo the weapon should have.")]
    public int maxMagazineAmmo;
    public int maxAmmoSafe;
    private int usedMagazineAmmo;
   
    //Check if out of ammo
    private bool outOfAmmo;

    private bool isAttach = false;


    [Header("Bullet Settings")]
    //Bullet
    [Tooltip("How much force is applied to the bullet when shooting.")]
    public float bulletForce = 400.0f;
    [Tooltip("How long after reloading that the bullet model becomes visible " +
        "again, only used for out of ammo reload animations.")]
    public float showBulletInMagDelay = 0.6f;
    [Tooltip("The bullet model inside the mag, not used for all weapons.")]
    public SkinnedMeshRenderer bulletInMagRenderer;
    [SerializeField]
    private float recoilForceHorisontal;
    [SerializeField]
    private float recoilForceVertical;
    [SerializeField]
    private float recoilSpeed;
    private float recoilStrength;

    [Header("Attachments")]
    public GameObject accelerator;
    public Rigidbody rbAcceleratorRotor;

    [Header("Attachments Settings")]
    public int strengthAcceleratorRotation;

    [Header("Grenade Settings")]
    public float grenadeSpawnDelay = 0.35f;
    public float grenadeThrowForce;

    [Header("Input KeyCode")]
    public KeyCode InputHolester;

    // [Header("Position spawn bullet")]
    // public Vector3 BulletPosition;
    private Ray ray;

    [Header("Muzzleflash Settings")]
    public bool randomMuzzleflash = false;
    //min should always bee 1
    private int minRandomValue = 1;

    [Range(2, 25)]
    public int maxRandomValue = 5;

    private int randomMuzzleflashValue;

    public bool enableMuzzleflash = true;
    public ParticleSystem muzzleParticles;
    public bool enableSparks = true;
    public ParticleSystem sparkParticles;
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Muzzleflash Light Settings")]
    public Light Flashlight;
    public Light muzzleflashLight;
    public float lightDuration = 0.08f;

    [Header("Audio Source")]
    //Main audio source
    public AudioSource mainAudioSource;
    //Audio source used for shoot sound
    public AudioSource shootAudioSource;

    [Header("UI Components")]
    public Text timescaleText;
    public Text currentWeaponText;
    public Text currentAmmoText;
    public Text totalAmmoText;
    public GameObject Crosshair;
    public Text Name;

    [Header("Layers")]
    public LayerMask crosshairLayerMask;

    [System.Serializable]
    public class prefabs
    {
        [Header("Prefabs")]
        public Transform bulletPrefab;
        public Transform casingPrefab;
        public Transform grenadePrefab;
    }
    public prefabs Prefabs;

    [Header("Other")]
    public int SpeedStand;
    public int RayCrosshairLenght;

    [System.Serializable]
    public class spawnpoints
    {  
		[Header("Spawnpoints")]
		//Array holding casing spawn points 
		//(some weapons use more than one casing spawn)
		//Casing spawn point array
		public Transform casingSpawnPoint;
		//Bullet prefab spawn from this point
		public Transform bulletSpawnPoint;

		public Transform grenadeSpawnPoint;
	}
	public spawnpoints Spawnpoints;

	[System.Serializable]
	public class soundClips
	{
		public AudioClip shootSound;
		public AudioClip takeOutSound;
		public AudioClip holsterSound;
		public AudioClip reloadSoundOutOfAmmo;
		public AudioClip reloadSoundAmmoLeft;
		public AudioClip aimSound;
        public AudioClip FlashlightClick;
	}
	public soundClips SoundClips;

	private bool soundHasPlayed = false;

    public AnimationCurve curve;

	private void Awake () 
    { 
        isJumping = movingScript._isGrounded;
        recoilStrength = 1;
        curve.Evaluate(recoilStrength); ////////////////////////////////////Изменить систему отдачи и добавить еще одну кривую для силы 
	}

    [SerializeField]
    private AttachComponent attachComponent;

	private void Start () {

       //transformForVertical = GetComponent<Transform>();

        //Set current ammo to total ammo value
        currentMagazineAmmo = maxMagazineAmmo;
        //Set the animator component
        anim = GetComponent<Animator>();
        tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
        muzzleflashLight.enabled = false;
        isLight = false;
        isSitting = false;
        FireType = true;
        isRunningJump = false;

        //isJumping = !isJumping;

        //Save the weapon name
        storedWeaponName = weaponName;
		//Get weapon name from string to text
		currentWeaponText.text = weaponName;
		//Set total ammo text from total ammo int
		totalAmmoText.text = currentAmmo.ToString();

		//Weapon sway
		initialSwayPositionArms = transform.localPosition;
        //Light sway
        initialSwayPositionLight = tfFlashlight.localRotation.eulerAngles;
        //Camera sway
        initialSwayPositionCamera = gunCamera.transform.localPosition;

        //Set the shoot sound to audio source
        shootAudioSource.clip = SoundClips.shootSound;
	}

    void _SwayPosition(Transform tf, float multiply, float sway, float maxSway, float swaySmooth, Vector3 initialSwayPos)
    {
        //Weapon sway
        if (weaponSway == true)
        {
            float movementX = -Input.GetAxis("Mouse X") * sway;
            float movementY = -Input.GetAxis("Mouse Y") * sway;
            //Clamp movement to min and max values
            movementX = Mathf.Clamp
                (movementX, -maxSway * multiply, maxSway * multiply);
            movementY = Mathf.Clamp
                (movementY, -maxSway * multiply, maxSway * multiply);
            //Lerp local pos
            Vector3 finalSwayPosition = new Vector3
                (movementX, movementY, -(movementX + movementY) * 0.2f);
            tf.localPosition = Vector3.Lerp
                (tf.localPosition, finalSwayPosition +
                    initialSwayPos, Time.deltaTime * swaySmooth);
        }
    }

    void _SwayRotation(Transform tf, float multiply, float speed, Vector3 initialSwayRot)
    {
        //Weapon sway
        if (weaponSway == true)
        {
            tf.localRotation = Quaternion.Euler(Mathf.Lerp(tf.localRotation.x, Input.GetAxis("Mouse Y") * multiply, speed * Time.deltaTime), Mathf.Lerp(tf.localRotation.x, -Input.GetAxis("Mouse X") * multiply, speed * Time.deltaTime), 0);
        }
    }

    void _SwayJump(Transform tf, float multiply, float speed, Vector3 initialSwayRot)
    {
        tf.localRotation = Quaternion.Euler(Mathf.Lerp(tf.localRotation.x, tf.localRotation.x + movingScript._rigidbody.velocity.y * multiply, speed * Time.deltaTime), tf.localRotation.y, tf.localRotation.z);
    }



    private void LateUpdate () {

        if (isAiming)
        {
            _SwayPosition(transform, 1, swayAmount, maxSwayAmount, swaySmoothValue, initialSwayPositionArms);
            //0.0015, 0.005
        }
        else _SwayPosition(transform, 3.5f, swayAmount * 3, maxSwayAmount * 3, swaySmoothValue * 2, initialSwayPositionArms);

        _SwayRotation(tfFlashlight, 20, 4, initialSwayPositionLight);
        _SwayJump(transform, 20, 1, initialSwayPositionArms);
        
        //_Sway(gunCamera.transform,1, initialSwayPositionCamera);
	}
	
	private void Update () {

        //_CrosshairPosition();

        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        if (inputX < 0) inputX = -inputX;
        if (inputY < 0) inputY = -inputY;

        _PickUpOrUse();

        Spawnpoints.bulletSpawnPoint.localPosition = _RaycastingPosition(tfBulletSPR, tfBulletSPR.transform.forward, 0.65f) - new Vector3(0,0,0.1f);

        //Debug.DrawRay(tfBulletSPR.position, tfBulletSPR.transform.forward, Color.red);
        //Debug.Log(Spawnpoints.bulletSpawnPoint.localPosition);

        recoilStrength = Mathf.Lerp(recoilStrength, 1, recoilSpeed * Time.deltaTime);
        //Debug.Log(recoilStrength);

        isAttach = attachComponent.isAttach;
        if (Input.GetKeyDown(KeyCode.E))
        {
            _SwitchLight();
        }
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ReadPixelColor());
        }
        _ItemName();
        if (Input.GetKeyDown(KeyCode.L))
        {
            isLight = !isLight;
            mainAudioSource.clip = SoundClips.FlashlightClick;
            mainAudioSource.Play();
            Flashlight.enabled = isLight;
        }

        if (Input.GetKeyDown(KeyCode.V) && !holstered)
        {
            FireType = !FireType;
            mainAudioSource.clip = SoundClips.aimSound;
            mainAudioSource.Play();

            soundHasPlayed = true;
        }

        _CrosshairPosition();

        //Continosuly check which animation 
        //is currently playing
        _AnimationCheck();

        if (isInspecting || holstered || isReloading || isShooting || isGrenadeThrowing)
        {
            isPlayingOtherAnimations = true;
        }
        else isPlayingOtherAnimations = false;

		//Aiming
		//Toggle camera FOV when right click is held down
		if(Input.GetButton("Fire2") && !isReloading && !isRunning && !isInspecting && !holstered) 
		{
            Crosshair.SetActive(false);
            CrosshairUI.SetActive(false);
            anim.SetBool("Aim", true);
            isAiming = true;
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aim In") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire Pose")) /*Aim Fire Pose || Aim In*/
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire"))
                    {
                    //Start aiming
                    //When right click is released
                    gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                        aimFov-6, fovShootSpeed * Time.deltaTime);
                    }
                else{
                    //Start aiming
                    //When right click is released
                    gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                        aimFov, fovSpeed * Time.deltaTime);
                }
            }
            else
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                defaultFov, fovSpeed * Time.deltaTime);
            }

            if (!soundHasPlayed) 
			{
				mainAudioSource.clip = SoundClips.aimSound;
				mainAudioSource.Play ();
	
				soundHasPlayed = true;
			}
		} 
		else 
		{
            Crosshair.SetActive(true);
            CrosshairUI.SetActive(true);
            //Использовать этот метод для изменения плавного значений
            //When right click is released
            gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
				defaultFov,fovSpeed * Time.deltaTime);

			isAiming = false;
			//Stop aiming
			anim.SetBool ("Aim", false);
				
			soundHasPlayed = false;
		}
		//Aiming end

		//If randomize muzzleflash is true, genereate random int values
		if (randomMuzzleflash == true) 
		{
			randomMuzzleflashValue = Random.Range (minRandomValue, maxRandomValue);
		}

		////Timescale settings
		////Change timescale to normal when 1 key is pressed
		//if (Input.GetKeyDown (KeyCode.Alpha1)) 
		//{
		//	Time.timeScale = 1.0f;
		//	timescaleText.text = "1.0";
		//}
		////Change timesccale to 50% when 2 key is pressed
		//if (Input.GetKeyDown (KeyCode.Alpha2)) 
		//{
		//	Time.timeScale = 0.5f;
		//	timescaleText.text = "0.5";
		//}
		////Change timescale to 25% when 3 key is pressed
		//if (Input.GetKeyDown (KeyCode.Alpha3)) 
		//{
		//	Time.timeScale = 0.25f;
		//	timescaleText.text = "0.25";
		//}
		////Change timescale to 10% when 4 key is pressed
		//if (Input.GetKeyDown (KeyCode.Alpha4)) 
		//{
		//	Time.timeScale = 0.1f;
		//	timescaleText.text = "0.1";
		//}
		////Pause game when 5 key is pressed
		//if (Input.GetKeyDown (KeyCode.Alpha5)) 
		//{
		//	Time.timeScale = 0.0f;
		//	timescaleText.text = "0.0";
		//}

        //if ()
        //{

        //}


		//Set current ammo text from ammo int
		currentAmmoText.text = currentMagazineAmmo.ToString ();

	

		//Play knife attack 1 animation when Q key is pressed
		if (Input.GetKeyDown (KeyCode.Q) && !isInspecting && !holstered && !isReloading && !anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire Pose") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Fire") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire") && !anim.GetCurrentAnimatorStateInfo(0).IsName("GrenadeThrow")) 
		{
			anim.Play ("Knife Attack 1", 0, 0f);
		}
        //Play knife attack 2 animation when F key is pressed
        if (Input.GetKeyDown(KeyCode.F) && !isInspecting && !holstered && !isReloading && !anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire Pose") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Fire") && 
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire") && !anim.GetCurrentAnimatorStateInfo(0).IsName("GrenadeThrow")) 
		{
			anim.Play ("Knife Attack 2", 0, 0f);
		}
			
		//Throw grenade when pressing G key
		if (Input.GetKeyDown (KeyCode.G) && !isPlayingOtherAnimations)
        {

                StartCoroutine (GrenadeSpawnDelay ());
			//Play grenade throw animation
			anim.Play("GrenadeThrow", 0, 0.0f);
            //bool IsThrow = anim.GetCurrentAnimatorStateInfo(1).IsName("GrenadeThrow");


        }

		//If out of ammo
		if (currentMagazineAmmo == 0) 
		{
			//Show out of ammo text
			currentWeaponText.text = "OUT OF AMMO";
			//Toggle bool
			outOfAmmo = true;
			//Auto reload if true
			if (autoReload == true && !isReloading) 
			{
				StartCoroutine (AutoReload ());
			}
		} 
		else 
		{
			//When ammo is full, show weapon name again
			currentWeaponText.text = storedWeaponName.ToString();
			//Toggle bool
			outOfAmmo = false;
			//anim.SetBool ("Out Of Ammo", false);
		}
			
		//Semiautomatic fire
		//Left click  
		if (Input.GetMouseButtonDown (0) && !outOfAmmo && !isReloading && !isInspecting && !isRunning && !holstered && !FireType) 
		{

            _Shoot();
        }

        //Shoot automatic
        //Left click hold 
        if (Input.GetMouseButton(0) && !outOfAmmo && !isReloading && !isInspecting && !isRunning && !holstered && FireType)
        {
            _Shoot(); 
        }

        

        //

        if (isRunning && isJumping)
        {
            isRunningJump = true;
        }
        else if (!isJumping)
        {
            isRunningJump = !isRunningJump;
        }


        //Inspect weapon when T key is pressed
        if (Input.GetKeyDown (KeyCode.T)) 
		{
			anim.SetTrigger("Inspect");
		}

		//Toggle weapon holster when E key is pressed
		if (Input.GetKeyDown (InputHolester) && !hasBeenHolstered) 
		{
			holstered = true;


			mainAudioSource.clip = SoundClips.holsterSound;
			mainAudioSource.Play();

			hasBeenHolstered = true;
		} 
		else if (Input.GetKeyDown (InputHolester) && hasBeenHolstered) 
		{
			holstered = false;
            
            mainAudioSource.clip = SoundClips.takeOutSound;
			mainAudioSource.Play ();

			hasBeenHolstered = false;
		}
		//Holster anim toggle
		if (holstered == true) 
		{
            Crosshair.SetActive(false);
            anim.SetBool ("Holster", true);
		} 
		else 
		{
            if (isAiming == false)
            {
                Crosshair.SetActive(true);
            }
            anim.SetBool ("Holster", false);
		}

		//Reload 
		if (Input.GetKeyDown (KeyCode.R) && !isReloading && !isInspecting) 
		{
			if(currentMagazineAmmo != maxMagazineAmmo)
            {
                //Reload
                Reload();
            }
		}

		//Walking when pressing down WASD keys
		if (Input.GetKey (KeyCode.W) && !isRunning || 
			Input.GetKey (KeyCode.A) && !isRunning || 
			Input.GetKey (KeyCode.S) && !isRunning || 
			Input.GetKey (KeyCode.D) && !isRunning) 
		{
			anim.SetBool ("Walk", true);
		} else {
			anim.SetBool ("Walk", false);
		}

		//Running when pressing down W and Left Shift key
		if ((Input.GetKey (KeyCode.W) && Input.GetKey (KeyCode.LeftShift))) 
		{
            if (!isSitting)
            {
                isRunning = true;
            }
		} else {
			isRunning = false;
		}
		
		//Run anim toggle
		if (isRunning == true) 
		{
			anim.SetBool ("Run", true);
		} 
		else 
		{
			anim.SetBool ("Run", false);
		}
	}

    void _Shoot()
    {
        //Shoot
            if (Time.time - lastFired > 1 / fireRate)
            {
            
                lastFired = Time.time;

                //Remove 1 bullet from ammo
                currentMagazineAmmo -= 1;

                rbAcceleratorRotor.AddRelativeTorque(new Vector3(0,0,1) * strengthAcceleratorRotation, ForceMode.VelocityChange);

                shootAudioSource.clip = SoundClips.shootSound;
                shootAudioSource.Play();

                if (!isAiming) //if not aiming
                {
                    anim.Play("Fire", 0, 0f);
                    //If random muzzle is false
                    if (!randomMuzzleflash &&
                        enableMuzzleflash == true)
                    {
                        muzzleParticles.Emit(1);
                        //Light flash start
                        StartCoroutine(MuzzleFlashLight());
                    }
                    else if (randomMuzzleflash == true)
                    {
                        //Only emit if random value is 1
                        if (randomMuzzleflashValue == 1)
                        {
                            if (enableSparks == true)
                            {
                                //Emit random amount of spark particles
                                sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
                            }
                            if (enableMuzzleflash == true)
                            {
                                muzzleParticles.Emit(1);
                                //Light flash start
                                StartCoroutine(MuzzleFlashLight());
                            }
                        }
                    }
                }
                else //if aiming
                {
                    anim.Play("Aim Fire", 0, 0f);
                    //If random muzzle is false
                    if (!randomMuzzleflash)
                    {
                        muzzleParticles.Emit(1);
                        //If random muzzle is true
                    }
                    else if (randomMuzzleflash == true)
                    {
                        //Only emit if random value is 1
                        if (randomMuzzleflashValue == 1)
                        {
                            if (enableSparks == true)
                            {
                                //Emit random amount of spark particles
                                sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
                            }
                            if (enableMuzzleflash == true)
                            {
                                muzzleParticles.Emit(1);
                                //Light flash start
                                StartCoroutine(MuzzleFlashLight());
                            }
                        }
                    }
                }

                //Spawn bullet from bullet spawnpoint
                Transform bullet = (Transform)Instantiate(
                    Prefabs.bulletPrefab,
                    Spawnpoints.bulletSpawnPoint.transform.position,
                    Spawnpoints.bulletSpawnPoint.transform.rotation);

                //Add velocity to the bullet
                bullet.GetComponent<Rigidbody>().velocity =
                    bullet.transform.forward * bulletForce;

            numberObjects += 1;
            Debug.Log(numberObjects);

            recoilStrength = Mathf.Lerp(recoilStrength, 1.6f, recoilSpeed * Time.deltaTime);
            //if(recoilStrength < 2)
            //{
            //    recoilStrength += recoilSpeed;    

            //}

            if (isAiming)
                {
                    tfForHorizontal.Rotate(0, Random.Range(-recoilForceHorisontal * recoilStrength, recoilForceHorisontal * recoilStrength) + (Input.GetAxis("Horizontal") / 2), 0, Space.Self);
                    tfForVertical.Rotate(Random.Range(-recoilForceVertical * recoilStrength * 0.85f, -recoilForceVertical * recoilStrength / 3) - inputX / 2 - inputY / 2, 0, 0, Space.Self);
                }
                else
                {
                    tfForHorizontal.Rotate(0, Random.Range(-recoilForceHorisontal * recoilStrength, recoilForceHorisontal * recoilStrength) + Input.GetAxis("Horizontal") / 3, 0, Space.Self);
                    tfForVertical.Rotate(Random.Range(-recoilForceVertical * recoilStrength * 0.5f, -recoilForceVertical * recoilStrength) - inputX / 2 - inputY / 2f, 0, 0, Space.Self);
                }

                //Spawn casing prefab at spawnpoint
                Instantiate(Prefabs.casingPrefab,
                    Spawnpoints.casingSpawnPoint.transform.position,
                    Spawnpoints.casingSpawnPoint.transform.rotation);
            }
    }
    void _ItemName()
    {
        RaycastHit hit;
        //сам луч, начинается от позиции этого объекта и направлен в сторону цели
        //ray = new Ray(transform.position + new Vector3(0.2f,+0.2f,0f), transform.forward);
        ray = new Ray(rayCamera.transform.position, rayCamera.transform.forward);
        //пускаем луч
        Physics.Raycast(ray, out hit,3);
        //if (hit.collider.tag == "Item" || hit.collider.tag == "Use")
        //{
        //    Name.text = hit.collider.name;
        //}
        //else Name.text = " ";

        //Ниже лишнее возможно
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Item" || hit.collider.tag == "Use")
            {
                //Name.text = hit.collider.name + "Press E";
                Name.text = "Press E";
            }
            else Name.text = " ";
        }
        else if (Name.text != " ")
        {
            Name.text = " ";
        }

    }
    IEnumerator ReadPixelColor()
    {
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();

        float x = Input.mousePosition.x;
        float y = Input.mousePosition.y;

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
        tex.Apply();

        Color color = tex.GetPixel(0, 0);
        //test.material.color = tex.GetPixel(0, 0);
        //Debug.Log(color);
    }

    //public delegate void _Using(bool isUsed);

    //public static event _Using Use;
    void _SwitchLight()
    {
        RaycastHit hit;
        //сам луч, начинается от позиции этого объекта и направлен в сторону цели
        // ray = new Ray(transform.position + new Vector3(0.2f, +0.2f, 0f), transform.forward);
        ray = new Ray(rayCamera.transform.position, rayCamera.transform.forward);
        //пускаем луч
        Physics.Raycast(ray, out hit,3);
        if (hit.collider != null)
        {
            if (hit.collider.name == "SwitcherLight")
            {
                Animation SwitchAnim = hit.collider.GetComponent<Animation>();
                LightON LightSc = hit.collider.GetComponent<LightON>();
                LightSc.isTurn = !LightSc.isTurn;
            }
        }
    }

    public void _PickUpOrUse()
    {
        RaycastHit hit;
        //сам луч, начинается от позиции этого объекта и направлен в сторону цели
        // ray = new Ray(transform.position + new Vector3(0.2f, +0.2f, 0f), transform.forward);
        ray = new Ray(rayCamera.transform.position, rayCamera.transform.forward);
        //пускаем луч
        Physics.Raycast(ray, out hit, 3);
        if (hit.collider != null)
        {
            if (hit.collider.name == "RifleMagazine") //переработать подбор объектов//конкретно патронов, изменение UI ammo и прочее
            {
                Name.text = "Pick Up - E";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (currentAmmo < maxAmmoSafe)
                    {
                        currentAmmo += maxMagazineAmmo;
                        if (currentAmmo > maxAmmoSafe)
                        {
                            currentAmmo = maxAmmoSafe;
                        }
                        totalAmmoText.text = currentAmmo.ToString();
                        
                    }
                    
                }

            }

            if (hit.collider.name == "Hood")
            {
                Name.text = "Open - E";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hit.transform.gameObject.GetComponent<SpringScript>())
                    {
                        SpringScript springScript = hit.transform.gameObject.GetComponent<SpringScript>();
                        springScript.isOpened = !springScript.isOpened;
                    }
                }
            }
            


        }
        else Name.text = "";

    }

	private IEnumerator GrenadeSpawnDelay () {
		
		//Wait for set amount of time before spawning grenade
		yield return new WaitForSeconds (grenadeSpawnDelay);
        //Spawn grenade prefab at spawnpoint

            Transform grenade = Instantiate(Prefabs.grenadePrefab,
            Spawnpoints.grenadeSpawnPoint.transform.position,
            Spawnpoints.grenadeSpawnPoint.transform.rotation);
            Rigidbody rbGrenade = grenade.GetComponent<Rigidbody>();
        rbGrenade.AddForce(rayCamera.transform.forward * grenadeThrowForce, ForceMode.Impulse );
        rbGrenade.AddForce(rayCamera.transform.up * grenadeThrowForce/4, ForceMode.Impulse);
    }

    private void _CrosshairPosition()
    {
        RaycastHit hit;
        //сам луч, начинается от позиции этого объекта и направлен в сторону цели
        // ray = new Ray(transform.position + new Vector3(0.2f, +0.2f, 0f), transform.forward);
        //Debug.DrawRay(tfCrosshairStartPos.position, tfCrosshairStartPos.forward, Color.red);

        ray = new Ray(tfBulletSPR.position, tfBulletSPR.forward);
        //пускаем луч
        Physics.Raycast(ray, out hit, RayCrosshairLenght, crosshairLayerMask);//Добавить другие слои, если появятся
        if (hit.collider != null)
        {
            tfCrosshairCanvas.localPosition = new Vector3(tfCrosshairCanvas.localPosition.x, tfCrosshairCanvas.localPosition.y, hit.distance);
            tfCrosshairCanvas.localScale = new Vector3(hit.distance * CrosshairSize, hit.distance * CrosshairSize, hit.distance * CrosshairSize);
            if (!isAiming && hit.distance < 80)
            {
                if (CrosshairUI.activeSelf)
                {
                    CrosshairUI.SetActive(false);
                }
                else if (!CrosshairUI.activeSelf)
                {
                    CrosshairUI.SetActive(true);
                }
            }
        }
        else
        {
            tfCrosshairCanvas.localPosition = new Vector3(tfBulletSPR.localPosition.x, tfBulletSPR.localPosition.y, RayCrosshairLenght);
            tfCrosshairCanvas.localScale = new Vector3(RayCrosshairLenght * CrosshairSize, RayCrosshairLenght * CrosshairSize, RayCrosshairLenght * CrosshairSize);
        }
    }// Доделать


    private Vector3 _RaycastingPosition(Transform rPosition, Vector3 rRotation, float rayLength)
    {
        RaycastHit hit;
        //сам луч, начинается от позиции этого объекта и направлен в сторону цели
        Ray ray = new Ray(rPosition.position, rRotation);
        //пускаем луч
        Physics.Raycast(ray, out hit, rayLength);
        if (hit.collider != null)
        {
            //return new Vector3(rPosition.localPosition.x ,rPosition.localPosition.y, hit.point.z - rPosition.position.z);
            return new Vector3(rPosition.localPosition.x, rPosition.localPosition.y, hit.distance);
        }
        else return new Vector3(rPosition.localPosition.x, rPosition.localPosition.y, rPosition.localPosition.z + rayLength); 
        //0.6145f
    }

	private IEnumerator AutoReload () {
		//Wait set amount of time
		yield return new WaitForSeconds (autoReloadDelay);

		if (outOfAmmo == true) 
		{
			//Play diff anim if out of ammo
			anim.Play ("Reload Out Of Ammo", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
			mainAudioSource.Play ();

			//If out of ammo, hide the bullet renderer in the mag
			//Do not show if bullet renderer is not assigned in inspector
			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = false;
				//Start show bullet delay
				StartCoroutine (ShowBulletInMag ());
			}
		} 
		//Restore ammo when reloading
		currentMagazineAmmo = maxMagazineAmmo;
		outOfAmmo = false;
	}

	//Reload
	private void Reload () {
        if (currentAmmo > 0)
        {

            if (outOfAmmo == true)
            {
                //Play diff anim if out of ammo
                anim.Play("Reload Out Of Ammo", 0, 0f);

                mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
                mainAudioSource.Play();

                //If out of ammo, hide the bullet renderer in the mag
                //Do not show if bullet renderer is not assigned in inspector
                if (bulletInMagRenderer != null)
                {
                    bulletInMagRenderer.GetComponent
                    <SkinnedMeshRenderer>().enabled = false;
                    //Start show bullet delay
                    StartCoroutine(ShowBulletInMag());
                }
            }
            else
            {
                //Play diff anim if ammo left
                anim.Play("Reload Ammo Left", 0, 0f);

                mainAudioSource.clip = SoundClips.reloadSoundAmmoLeft;
                mainAudioSource.Play();

                //If reloading when ammo left, show bullet in mag
                //Do not show if bullet renderer is not assigned in inspector
                if (bulletInMagRenderer != null)
                {
                    bulletInMagRenderer.GetComponent
                    <SkinnedMeshRenderer>().enabled = true;
                }
            }
            if (currentAmmo < maxMagazineAmmo)
            {
                usedMagazineAmmo =  maxMagazineAmmo - currentMagazineAmmo;//использованные патроны в магазине 
                if (usedMagazineAmmo > currentAmmo)
                {
                    currentMagazineAmmo += currentAmmo;
                    currentAmmo -= currentAmmo;
                }
                else {
                    currentMagazineAmmo += usedMagazineAmmo;
                    currentAmmo -= usedMagazineAmmo;
                }
                totalAmmoText.text = currentAmmo.ToString();
                outOfAmmo = false;
            }
            else
            {
                //Restore ammo when reloading
                currentAmmo -= maxMagazineAmmo - currentMagazineAmmo;
                currentMagazineAmmo += maxMagazineAmmo - currentMagazineAmmo;
                totalAmmoText.text = currentAmmo.ToString();
                outOfAmmo = false;
            }
        }
    }

	//Enable bullet in mag renderer after set amount of time
	private IEnumerator ShowBulletInMag () {
		
		//Wait set amount of time before showing bullet in mag
		yield return new WaitForSeconds (showBulletInMagDelay);
		bulletInMagRenderer.GetComponent<SkinnedMeshRenderer> ().enabled = true;
	}

	//Show light when shooting, then disable after set amount of time
	private IEnumerator MuzzleFlashLight () {
		
		muzzleflashLight.enabled = true;
		yield return new WaitForSeconds (lightDuration);
		muzzleflashLight.enabled = false;
	}

	//Check current animation playing
	private void _AnimationCheck () {
		
		//Check if reloading
		//Check both animations
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Out Of Ammo") || 
			anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Ammo Left")) 
		{
			isReloading = true;
		} 
		else 
		{
			isReloading = false;
		}

		//Check if inspecting weapon
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Inspect")) 
		{
            isInspecting = true;
		} 
		else 
		{
			isInspecting = false;
		}

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire Pose") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Fire") || anim.GetCurrentAnimatorStateInfo(0).IsName("Aim Fire"))
        {
            isShooting = true;
        }
        else isShooting = false;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("GrenadeThrow"))
        {
            isGrenadeThrowing = true;
        }
        else isGrenadeThrowing = false;
    }
}
// ----- Low Poly FPS Pack Free Version -----