using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

namespace RPG {
    public class PlayerCharacterManager : MonoBehaviour {

        [HideInInspector]
        public int IDLE = Animator.StringToHash("Idle");
        [HideInInspector]
        public int RUN = Animator.StringToHash("Run");
        [HideInInspector]
		public int WALK = Animator.StringToHash("Walk");
        [HideInInspector]
        public int CROUCH = Animator.StringToHash("Crouch");
        [HideInInspector]
        public int LOCOMOTION = Animator.StringToHash("Locomotion");
        [HideInInspector]
        public int COVER_TYPE = Animator.StringToHash("CoverType");
        [HideInInspector]
        public string DRAW_WEAPON = "drawWeapon";
		[HideInInspector]
		public string STRAFE_SPEED = "StrafeSpeed";
        [HideInInspector]
        public string IN_COVER = "InCover";
        [HideInInspector]
        public string HORIZONTAL = "Horizontal";
        [HideInInspector]
        public string SPEED = "Speed";
        [HideInInspector]
		public string HIGH_COVER = "HighCover";
		[HideInInspector]
		public string LOW_COVER = "LowCover";
        [HideInInspector]
        public string IS_SHOOTING = "IsShooting";

        [HideInInspector] public PlayerInput inputAction;
        [HideInInspector] public Animator animator;
        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public RigBuilder rigBuilder;

        [Header("Components")]

        [Header("Camera:")]
        public Transform camObj;
        public GameObject aimCam;
        public GameObject normalCam;
        public Transform aimCamLookAt;
        public Transform aimCamLookAt_LS;
        public Transform aimCamLookAt_RS;
		[Header("Rig")]
        public RigLayer bodyLayer;
        public RigLayer aimLayer;
        
        public TwoBoneIKConstraint leftHandGrip;
        public TwoBoneIKConstraint rightHandGrip;
		
        public Transform leftHandGripHoldPose;
		public Transform rightHandGripHoldPose;

        public Transform rightHandGripWhileAiming_RS;
        public Transform leftHandGripWhileAiming_RS;
        public Transform leftHandGripWhileAiming_LS;
        public Transform rightHandGripWhileAiming_LS;
        
        public Transform weaponAimPos;
        public Transform LS_WeaponAimPos;
        public Transform RS_WeaponAimPos;

		public string currentState;

        public StateMachine playerSM;
        public PlayerIdlingState idling;
        public PlayerRunningState running;
        public PlayerWalkingState walking;
        public PlayerAimingState aiming;
        public PlayerCrouchState crouching;


		public Vector2 movementInput;
        public Vector2 mouseInput;

        [Header("UI")]
        public GameObject crossHair;

		[Header("Player stats")]
		public float runSpeed;
        public float rotSpeed;
        public float walkSpeed;
        public float crouchSpeed;
        public float aimSpeed;
        public float animDampTime = 0.5f;
        public float aimSensitivityY;
        public float aimSensitivityX;

        [Header("Cover Variables")]
        public float checkForCoverDist;
        public LayerMask whatIsCover;
        public float moveToCoverStopDist;
        public bool checkingForCover;
        public float moveToCoverSpeed;
        public bool inCover;
        public float coverSpeed;
        public Transform coverHelperTransform;
        public Transform coverHelperRightTransform;
        public Transform coverHelperLeftTransform;

		[Header("Movement Flags")]
        public bool isWalking;
        public bool isInteracting;
        public bool isCrouching;
        public bool isAiming;

        [Header("Combat Flags")]
		public RaycastWeapon weapon;
		public bool isShooting = false;
        public bool readyToShoot; //TEMPORARY. NEED TO CHANGE TO ADD DIFFERENT WEAPON TYPES

        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;
            inputAction = GetComponent<PlayerInput>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            rigBuilder = GetComponent<RigBuilder>();
            playerSM = new StateMachine();  
            idling = new PlayerIdlingState(this, playerSM);
            running = new PlayerRunningState(this, playerSM);
            walking = new PlayerWalkingState(this, playerSM);
            aiming = new PlayerAimingState(this, playerSM);
            crouching = new PlayerCrouchState(this, playerSM);
            crossHair.SetActive(false);
            aimCamLookAt = aimCamLookAt_RS;
            CinemachineFreeLook ac = aimCam.GetComponent<CinemachineFreeLook>();
			ac.Priority = 0;
            ac.Follow = aimCamLookAt;
            ac.LookAt = aimCamLookAt;
			normalCam.GetComponent<CinemachineFreeLook>().Priority = 1;
			leftHandGrip.data.target = leftHandGripHoldPose;
            rightHandGrip.data.target = rightHandGripHoldPose;
            rigBuilder.Build();
            playerSM.Initialize(idling);
            //TEMPORARY NEED TO CHANGE
            weapon = GetComponentInChildren<RaycastWeapon>();
        }

        private void Update() {
            playerSM.currentState.LogicalUpdate();
            playerSM.currentState.HandleInput();
            currentState = playerSM.currentState.ToString();
        }

        private void FixedUpdate() {
            playerSM.currentState.PhysicsUpdate();

        }

       

        private void OnDrawGizmosSelected() {
            playerSM.currentState.OnDrawGizmosSelected();
        }

    }

}