using System;
using System.Linq;
using UnityEngine;


namespace FPSControllerLPFP
{
    /// Manages a first person character
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(AudioSource))]
    //[RequireComponent(typeof(CharacterController))]

    public class FpsControllerLPFP : MonoBehaviour
    {
#pragma warning disable 649
        //Связать со скриптом оружия с переменной isSitting и isAiming !!!!!!!!
        public AutomaticGunScriptLPFP AutomaticGunScript;
        public HandgunScriptLPFP HandGunScript;
        private bool isFirstWeapon = true;


        [Header("Arms")]
        [Tooltip("The transform component that holds the gun camera."), SerializeField]
        private Transform arms;
        private Transform tfFirstWeapon;
        private Transform tfSecondaryWeapon;
        //private float fovFirstWeapon;
        //private float fovSecondaryWeapon;
        [SerializeField]
        private GameObject firstWeapon;
        [SerializeField]
        private GameObject secondaryWeapon;
        [SerializeField]
        private KeyCode inputChangeOnFirstWeapon, inputChangeOnSecondaryWeapon;



        [Tooltip("The position of the arms and gun camera relative to the fps controller GameObject."), SerializeField]
        private Vector3 armPosition;

		[Header("Audio Clips")]
        [Tooltip("The audio clip that is played while walking."), SerializeField]
        private AudioClip walkingSound;

        [Tooltip("The audio clip that is played while running."), SerializeField]
        private AudioClip runningSound;

		[Header("Movement Settings")]
        [Tooltip("How fast the player moves while walking and strafing."), SerializeField]
        private float walkingSpeed = 5f;
        [SerializeField]
        private float sittingSpeed = 3f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float runningSpeed = 10f;
        [SerializeField]
        public float speedDuck = 1.5f;
        [SerializeField]
        public float speedStandUp = 1.2f;
        [Tooltip("Approximately the amount of time it will take for the player to reach maximum running or walking speed."), SerializeField]
        private float movementSmoothness = 0.125f;

        [Tooltip("Amount of force applied to the player when jumping."), SerializeField]
        private float jumpForce = 35f;

        public float Height;
        public Vector3 colliderPosition;
        private float standartHeight;
        private Vector3 standartColliderPosition;

        [Header("Look Settings")]
        [Tooltip("Rotation speed of the fps controller."), SerializeField]
        private float mouseSensitivity = 7f;

        [Tooltip("Approximately the amount of time it will take for the fps controller to reach maximum rotation speed."), SerializeField]
        private float rotationSmoothness = 0.05f;

        [Tooltip("Minimum rotation of the arms and camera on the x axis."),
         SerializeField]
        private float minVerticalAngle = -90f;

        [Tooltip("Maximum rotation of the arms and camera on the axis."),
         SerializeField]
        private float maxVerticalAngle = 90f;

        [Tooltip("The names of the axes and buttons for Unity's Input Manager."), SerializeField]
        private FpsInput input;
      

        

#pragma warning restore 649

        [HideInInspector]
        public Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private AudioSource _audioSource;
        private SmoothRotation _rotationX;
        private SmoothRotation _rotationY;
        private SmoothVelocity _velocityX;
        private SmoothVelocity _velocityZ;
        internal bool _isGrounded;
        private Ray ray;
        public float rayLenght = 1f;

       

        private readonly RaycastHit[] _groundCastResults = new RaycastHit[8];
        private readonly RaycastHit[] _wallCastResults = new RaycastHit[8];

        /// Initializes the FpsController on start.
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _collider = GetComponent<CapsuleCollider>();
            _audioSource = GetComponent<AudioSource>();
			arms = AssignCharactersCamera();
            _audioSource.clip = walkingSound;
            _audioSource.loop = true;
            _rotationX = new SmoothRotation(RotationXRaw);
            _rotationY = new SmoothRotation(RotationYRaw);
            _velocityX = new SmoothVelocity();
            _velocityZ = new SmoothVelocity();
            Cursor.lockState = CursorLockMode.Locked;
            ValidateRotationRestriction();
            standartHeight = _collider.height;
            standartColliderPosition = _collider.center;
            tfFirstWeapon = firstWeapon.transform;
            tfSecondaryWeapon = secondaryWeapon.transform;
            
        }

        public void Start()
        {
            //fovFirstWeapon = AutomaticGunScript.defaultFov;
            //fovSecondaryWeapon = HandGunScript.defaultFov;

            

        }
			
        private Transform AssignCharactersCamera()
        {
            Transform t = transform;
			arms.SetPositionAndRotation(t.position, t.rotation);
			return arms;
        }
        
        /// Clamps <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> to valid values and
        /// ensures that <see cref="minVerticalAngle"/> is less than <see cref="maxVerticalAngle"/>.
        private void ValidateRotationRestriction()
        {
            minVerticalAngle = ClampRotationRestriction(minVerticalAngle, -90, 90);
            maxVerticalAngle = ClampRotationRestriction(maxVerticalAngle, -90, 90);
            if (maxVerticalAngle >= minVerticalAngle) return;
            Debug.LogWarning("maxVerticalAngle should be greater than minVerticalAngle.");
            float min = minVerticalAngle;
            minVerticalAngle = maxVerticalAngle;
            maxVerticalAngle = min;
        }

        private static float ClampRotationRestriction(float rotationRestriction, float min, float max)
        {
            if (rotationRestriction >= min && rotationRestriction <= max) return rotationRestriction;
            string message = string.Format("Rotation restrictions should be between {0} and {1} degrees.", min, max);
            Debug.LogWarning(message);
            return Mathf.Clamp(rotationRestriction, min, max);
        }
			
        /// Checks if the character is on the ground.
        private void OnCollisionStay()
        {
            Bounds bounds = _collider.bounds;
            Vector3 extents = bounds.extents;
            float radius = extents.x - 0.01f;
            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                _groundCastResults, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
            if (!_groundCastResults.Any(hit => hit.collider != null && hit.collider != _collider)) return;
            for (int i = 0; i < _groundCastResults.Length; i++)
            {
                _groundCastResults[i] = new RaycastHit();
            }

            _isGrounded = true;
        }
			
        /// Processes the character movement and the camera rotation every fixed framerate frame.
        private void FixedUpdate()
        {
            // FixedUpdate is used instead of Update because this code is dealing with physics and smoothing.
            _isGrounded = false;
        }
			
        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        private void Update()
        {//Переписать систему спринта через : /////////////////////////////////////////////////////////////////////////////////////////////////
            //Mathf.Lerp(_rigidbody.velocity);
            Debug.Log(Input.GetAxis("Fire3"));
            arms.position = transform.position + transform.TransformVector(armPosition);
            Jump();
            RotateCameraAndCharacter();
            MoveCharacter();
            SitDown();
            PlayFootstepSounds();

            //Смена оружия
            if (Input.GetKeyDown(inputChangeOnFirstWeapon) && !isFirstWeapon)
            {
                isFirstWeapon = true;
                arms = firstWeapon.transform;
                firstWeapon.SetActive(true);
                secondaryWeapon.SetActive(false);
                tfFirstWeapon.rotation = tfSecondaryWeapon.rotation;
                //tfFirstWeapon.SetPositionAndRotation(tfSecondaryWeapon.localPosition, tfSecondaryWeapon.localRotation);
            }
            if (Input.GetKeyDown(inputChangeOnSecondaryWeapon) && isFirstWeapon)
            {
                isFirstWeapon = false;
                arms = secondaryWeapon.transform;
                firstWeapon.SetActive(false);
                secondaryWeapon.SetActive(true);
                tfSecondaryWeapon.rotation = tfFirstWeapon.rotation;
                //tfSecondaryWeapon.SetPositionAndRotation(tfFirstWeapon.localPosition, tfFirstWeapon.localRotation);
            }
        }

        private void SitDown()
        {
            RaycastHit hit;
            //сам луч, начинается от позиции этого объекта и направлен в сторону цели
            //ray = new Ray(transform.position + new Vector3(0.2f,+0.2f,0f), transform.forward);
            ray = new Ray(arms.position, arms.up);
            //пускаем луч
            Physics.Raycast(ray, out hit, rayLenght);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (_collider.height >= Height)
                {
                    _collider.height = Mathf.Lerp(_collider.height,
                Height, speedDuck * Time.deltaTime);
                }//_collider.center = ColliderPosition;

                if (_collider.center.y <= standartColliderPosition.y+0.25f)
                {
                    _collider.center = new Vector3(_collider.center.x, _collider.center.y + (2 * Time.deltaTime), _collider.center.z);
                }

            }
            else if (hit.collider == null)
            {
                {
                    if (_collider.height <= standartHeight)
                    {
                        _collider.height += 3 * Time.deltaTime * speedStandUp;
                    }
                    if (_collider.center.y >= standartColliderPosition.y)
                    {
                        _collider.center = new Vector3(_collider.center.x, _collider.center.y - (1 * Time.deltaTime), _collider.center.z);
                    }
                }
            }
        }

        private void RotateCameraAndCharacter()
        {
            float rotationX = _rotationX.Update(RotationXRaw, rotationSmoothness);
            float rotationY = _rotationY.Update(RotationYRaw, rotationSmoothness);
            float clampedY = RestrictVerticalRotation(rotationY);
            _rotationY.Current = clampedY;
			Vector3 worldUp = arms.InverseTransformDirection(Vector3.up);
			Quaternion rotation = arms.rotation * Quaternion.AngleAxis(rotationX, worldUp) * Quaternion.AngleAxis(clampedY, Vector3.left);
            transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
			arms.rotation = rotation;
        }
			
        /// Returns the target rotation of the camera around the y axis with no smoothing.
        private float RotationXRaw
        {
            get { return input.RotateX * mouseSensitivity; }
        }
			
        /// Returns the target rotation of the camera around the x axis with no smoothing.
        private float RotationYRaw
        {
            get { return input.RotateY * mouseSensitivity; }
        }
			
        /// Clamps the rotation of the camera around the x axis
        /// between the <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> values.
        private float RestrictVerticalRotation(float mouseY)
        {
			float currentAngle = NormalizeAngle(arms.eulerAngles.x);
            float minY = minVerticalAngle + currentAngle;
            float maxY = maxVerticalAngle + currentAngle;
            return Mathf.Clamp(mouseY, minY + 0.01f, maxY - 0.01f);
        }
			
        /// Normalize an angle between -180 and 180 degrees.
        /// <param name="angleDegrees">angle to normalize</param>
        /// <returns>normalized angle</returns>
        private static float NormalizeAngle(float angleDegrees)
        {
            while (angleDegrees > 180f)
            {
                angleDegrees -= 360f;
            }

            while (angleDegrees <= -180f)
            {
                angleDegrees += 360f;
            }

            return angleDegrees;
        }

        private void MoveCharacter()
        {
            Vector3 direction = new Vector3(input.Move, 0f, input.Strafe).normalized;
            Vector3 worldDirection = transform.TransformDirection(direction);
            Vector3 velocity;
            if (AutomaticGunScript.isAiming)
            {
                velocity = worldDirection * (input.Run ? runningSpeed / 1.4f : walkingSpeed / 1.4f);
            }
            else velocity = worldDirection * (input.Run ? runningSpeed : walkingSpeed);
            //velocity = worldDirection * (input.Run ? runningSpeed / 2 : walkingSpeed / 2);
            //Checks for collisions so that the character does not stuck when jumping against walls.
            bool intersectsWall = CheckCollisionsWithWalls(velocity);
            if (intersectsWall)
            {
                _velocityX.Current = _velocityZ.Current = 100f;
                return;
            }

            float smoothX = _velocityX.Update(velocity.x, movementSmoothness);
            float smoothZ = _velocityZ.Update(velocity.z, movementSmoothness);
            Vector3 rigidbodyVelocity = _rigidbody.velocity;
            Vector3 force = new Vector3(smoothX - rigidbodyVelocity.x, 0f, smoothZ - rigidbodyVelocity.z);
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private bool CheckCollisionsWithWalls(Vector3 velocity)
        {
            if (_isGrounded) return false;
            Bounds bounds = _collider.bounds;
            float radius = _collider.radius;
            float halfHeight = _collider.height * 0.5f - radius * 1.0f;
            Vector3 point1 = bounds.center;
            point1.y += halfHeight;
            Vector3 point2 = bounds.center;
            point2.y -= halfHeight;
            Physics.CapsuleCastNonAlloc(point1, point2, radius, velocity.normalized, _wallCastResults,
                radius * 0.04f, ~0, QueryTriggerInteraction.Ignore);
            bool collides = _wallCastResults.Any(hit => hit.collider != null && hit.collider != _collider);
            if (!collides) return false;
            for (int i = 0; i < _wallCastResults.Length; i++)
            {
                _wallCastResults[i] = new RaycastHit();
            }

            return true;
        }

        private void Jump()
        {
            if (!_isGrounded || !input.Jump) return;
            _isGrounded = false;
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void PlayFootstepSounds()
        {
            if (_isGrounded && _rigidbody.velocity.sqrMagnitude > 0.1f)
            {
                _audioSource.clip = input.Run ? runningSound : walkingSound;
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
            }
            else
            {
                if (_audioSource.isPlaying)
                {
                    _audioSource.Pause();
                }
            }
        }
			
        
        /// A helper for assistance with smoothing the camera rotation.
        private class SmoothRotation
        {
            private float _current;
            private float _currentVelocity;

            public SmoothRotation(float startAngle)
            {
                _current = startAngle;
            }
				
            /// Returns the smoothed rotation.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDampAngle(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }
			
        /// A helper for assistance with smoothing the movement.
        private class SmoothVelocity
        {
            private float _current;
            private float _currentVelocity;

            /// Returns the smoothed velocity.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDamp(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }
			
        /// Input mappings
        [Serializable]
        private class FpsInput
        {
            [Tooltip("The name of the virtual axis mapped to rotate the camera around the y axis."),
             SerializeField]
            private string rotateX = "Mouse X";

            [Tooltip("The name of the virtual axis mapped to rotate the camera around the x axis."),
             SerializeField]
            private string rotateY = "Mouse Y";

            [Tooltip("The name of the virtual axis mapped to move the character back and forth."),
             SerializeField]
            private string move = "Horizontal";

            [Tooltip("The name of the virtual axis mapped to move the character left and right."),
             SerializeField]
            private string strafe = "Vertical";

            [Tooltip("The name of the virtual button mapped to run."),
             SerializeField]
            private string run = "Fire3";

            [Tooltip("The name of the virtual button mapped to jump."),
             SerializeField]
            private string jump = "Jump";

            /// Returns the value of the virtual axis mapped to rotate the camera around the y axis.
            public float RotateX
            {
                get { return Input.GetAxisRaw(rotateX); }
            }
				         
            /// Returns the value of the virtual axis mapped to rotate the camera around the x axis.        
            public float RotateY
            {
                get { return Input.GetAxisRaw(rotateY); }
            }
				        
            /// Returns the value of the virtual axis mapped to move the character back and forth.        
            public float Move
            {
                get { return Input.GetAxisRaw(move);}
                
            }
				       
            /// Returns the value of the virtual axis mapped to move the character left and right.         
            public float Strafe
            {
                get { return Input.GetAxisRaw(strafe); }
            }
				    
            /// Returns true while the virtual button mapped to run is held down.          
            public bool Run
            {
                get { return Input.GetButton(run); }
            }
				     
            /// Returns true during the frame the user pressed down the virtual button mapped to jump.          
            public bool Jump
            {
                get { return Input.GetButtonDown(jump); }
            }
        }
    }
}