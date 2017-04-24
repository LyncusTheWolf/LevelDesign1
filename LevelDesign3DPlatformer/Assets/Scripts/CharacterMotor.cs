using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMotor : MonoBehaviour {
	public struct GroundHit {
		public Vector3 normal;
		public Vector3 angleAxis;
		public Vector3 slopeDirection;
	}

	[System.Serializable]
	public struct SkillSet {
		public bool ableToJump;
		public bool ableToWallGrab;
		public bool ableToWallJump;
		public bool ableToDoubleJump;
        public bool ableToPushBlocks;
	}

    public const float INPUT_DEAD_ZONE = 0.05f;

    #region Animator Strings
    public const string LOCOMOTION_STRING = "Base Layer.Locomotion";
    public const string JUMP_STRING = "Base Layer.Actions.Jump";
    public const string FALL_STRING = "Base Layer.Falling";
    public const string PUSH_STRING = "Base Layer.Actions.Push Block";
    public const string WALL_SLIDE_STRING = "Base Layer.Actions.Wall Slide";
    public const string WALL_JUMP_STRING = "Base Layer.Actions.Wall Jump";
    #endregion

    public SkillSet skills;

	[SerializeField]
	private Animator anim;
	[SerializeField]
	private Transform visuals;

	[SerializeField]
	private float moveSpeed;
	[SerializeField]
	private float aerialSpeed;
	[SerializeField]
	private float turningSpeed;
	[SerializeField]
	private float jumpSpeed;
	[SerializeField]
	private float groundMomentumChangeSpeed;
	[SerializeField]
	private float aerialMomentumChangeSpeed;

	[Header("Wall Jump Controls")]
	[SerializeField]
	private float wallJumpSpeed;
	[SerializeField]
	private float wallSlideSpeed;
	[SerializeField]
	private float wallCastLength;
	[SerializeField]
	private Vector3 characterCenterCast;
	[SerializeField]
	private LayerMask jumpableWallMask;
	[SerializeField] [Range(0.0f, 1.0f)]
	private float wallSlidePenalty;

	[Header("Grounding Controls")]
	[SerializeField]
	private Transform groundCheckPoint;
	[SerializeField]
	private float groundingDistance;
	[SerializeField]
	private LayerMask groundingMask;

    [Header("Pushing Mechanics")]
    [SerializeField]
    private float pushStrength;
    [SerializeField]
    private float pushCastLength;
    [SerializeField]
    private LayerMask pushMask;

	[Header("Visual Effects")]
	[SerializeField]
	private ParticleSystem doubleJumpBurst;

	[HideInInspector]
	public bool ignoreJumpingCap = false;

	//Private cache
	private CharacterController controller;
	private Vector3 velocity;
	private Vector3 moveDelta;
	private bool onWall;
	private bool isGrounded;
	private bool canDoubleJump;
	private bool canWallJump;
	private Vector3 wallNormal;
    private PushableBlock currentBlock;
    private Vector3 blockNormal;

    //Anim Hashes
    private AnimatorStateInfo currentBaseStateInfo;
    private AnimatorTransitionInfo currentTransitionInfo;

    private int locomotionString_ID;
    private int jumpString_ID;
    private int fallString_ID;
    private int pushString_ID;
    private int wallSlideString_ID;
    private int wallJumpString_ID;

	//private Vector3 groundSlope;
	private GroundHit groundValues;

	public Vector3 Velocity{
		get{ return velocity; }
		set{ velocity = value; }
	}

    public Vector3 Center {
        get { return transform.position + controller.center; }
    }

    private void LoadHashes() {
        locomotionString_ID = Animator.StringToHash(LOCOMOTION_STRING);
        jumpString_ID = Animator.StringToHash(JUMP_STRING);
        fallString_ID = Animator.StringToHash(FALL_STRING);
        pushString_ID = Animator.StringToHash(PUSH_STRING);
        wallSlideString_ID = Animator.StringToHash(WALL_SLIDE_STRING);
        wallJumpString_ID = Animator.StringToHash(WALL_JUMP_STRING);
    }

    public void Awake(){
		controller = GetComponent<CharacterController> ();

        LoadHashes();
	}

    public void Start() {
        GameManager.Instance.LevelInit();
    }

	public void Update(){
        if (GameManager.Instance.Paused) {
            return;
        }

        currentBaseStateInfo = anim.GetCurrentAnimatorStateInfo(0);
        currentTransitionInfo = anim.GetAnimatorTransitionInfo(0);
        //AnimatorStateInfo actionStateInfo = anim.GetCurrentAnimatorStateInfo(1);

        //Debug.Log(actionStateInfo.fullPathHash);
        //Debug.Log(currentBaseStateInfo.fullPathHash);

        if(currentBaseStateInfo.fullPathHash == jumpString_ID) {
            Debug.Log("Jumping");
        }

        CheckGrounding();
        ResetAnimatorCues();

		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        //If the input given by the player is below the dead zone threshold then consider the input to be zero for the frame
        if(input.sqrMagnitude <= INPUT_DEAD_ZONE) {
            input = Vector3.zero;
        }

		Vector3 dir = CameraRelativeInput(input);
        float oldVertical = velocity.y;
		moveDelta = velocity;
		oldVertical -= GameManager.Instance.gravity * Time.deltaTime;
		float oldMagnitude = dir.magnitude;

		CheckForClimbableWalls(dir);

        CheckForMovableObject(dir);

        dir = (dir - groundValues.normal * Vector3.Dot(dir, groundValues.normal)).normalized * oldMagnitude;

		//Debug.DrawRay(transform.position, dir * 100.0f, Color.yellow);

        //controller.isGrounded
		moveDelta = Vector3.Lerp(moveDelta, dir * (controller.isGrounded ? moveSpeed : aerialSpeed), Time.deltaTime * (controller.isGrounded || onWall? groundMomentumChangeSpeed : aerialMomentumChangeSpeed));

        if (Input.GetButtonDown("Jump")) {
            if (!skills.ableToJump) {
                Debug.Log("Player is not able to jump yet");
            } else if (canWallJump && onWall) {
                //moveDelta = Vector3.Reflect(moveDelta, wallNormal);                     //Reflect the move direction across the walls normal
                moveDelta = wallNormal;
                moveDelta.y = 0;                                                        //Temporarily kill the y to get the direction that the character is jumping from
                moveDelta = moveDelta.normalized * wallJumpSpeed;                      //Use this direction to create a speed that the character is jumping off the wall from
                onWall = false;
                anim.SetTrigger("Wall Jump");
                JumpInternal();                                               //Reapply the y value as a jump vertical speed
            } else if (isGrounded) {
                if (AnimAbleToJump()) {
                    anim.SetTrigger("Jump");
                }
                JumpInternal();
            }  else if (!isGrounded && canDoubleJump) {
                if (!ignoreJumpingCap) {
                    canDoubleJump = false;
                }
                JumpInternal();
                doubleJumpBurst.Play();
            } else {
                moveDelta.y = oldVertical;
            }
        } else if (currentBlock != null && isGrounded && skills.ableToPushBlocks) {
            moveDelta = -blockNormal * pushStrength;
            currentBlock.SetNextFrameDirection(-blockNormal * pushStrength);
            anim.SetBool("Pushing", true);
            //currentBlock.rigidBody.AddForce(-blockNormal * pushStrength, ForceMode.VelocityChange);
        } else {
            if (onWall && skills.ableToWallGrab) {
                moveDelta *= wallSlidePenalty;
                moveDelta.y = Mathf.Clamp(oldVertical, -wallSlideSpeed, jumpSpeed);
            } else {
                moveDelta.y = oldVertical;
            }
        }

		Vector3 groundVelocity = moveDelta;
		groundVelocity.y = 0;

		if (onWall && skills.ableToWallGrab) {
			Vector3 temp = wallNormal;
			temp.y = 0;
			transform.rotation = Quaternion.LookRotation(temp, transform.up);
			anim.SetBool("OnWall", true);
			//visuals.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
		} else if(oldMagnitude >= 0.01f) {
			transform.rotation = Quaternion.LookRotation(groundVelocity, transform.up);
			anim.SetBool("OnWall", false);
			//visuals.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		} else {
			anim.SetBool("OnWall", false);
			//visuals.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		}

		anim.SetFloat("Velocity", groundVelocity.magnitude);

		velocity = moveDelta;

		controller.Move (moveDelta * Time.deltaTime);
	}

	private Vector3 CameraRelativeInput(Vector3 input) {
		Vector3 root = transform.forward;
		Vector3 camDirection = Camera.main.transform.forward;
		camDirection.y = 0.0f; // kill Y
		Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(camDirection));

		Vector3 moveDirection = referentialShift * input;

		return moveDirection;
	}

	private void JumpInternal() {
		moveDelta.y = jumpSpeed;
	}

	private void CheckGrounding() {
		RaycastHit hit;

		Debug.DrawRay(groundCheckPoint.position, Vector3.up * -groundingDistance, Color.red, 1.25f);
		if (Physics.Raycast(groundCheckPoint.position, -Vector3.up, out hit, groundingDistance, groundingMask)) {
			isGrounded = true;
			/*if (skills.ableToDoubleJump) {
				canDoubleJump = true;
			}*/

			groundValues.normal = hit.normal;
			groundValues.angleAxis = Vector3.Cross(groundValues.normal, transform.up);
			groundValues.slopeDirection = Vector3.Cross(groundValues.normal, groundValues.angleAxis);

            //anim.SetBool("Grounded", true);
            //velocity.y = Mathf.Clamp (velocity.y, 0.0f, float.MaxValue);

			//TODO: Dynamically parent the player to the object they are grounded to.
			//transform.SetParent(hit.collider.transform);

			//Debug.DrawRay(transform.position, groundValues.normal * 4, Color.cyan);
			//Debug.DrawRay(transform.position, groundValues.angleAxis * 4, Color.blue);
			//Debug.DrawRay(transform.position, groundValues.slopeDirection * 4, Color.green);
		} else {
			Debug.Log("No ground");
			isGrounded = false;
			groundValues.normal = transform.up;
			groundValues.angleAxis = Vector3.zero;
			groundValues.slopeDirection = -transform.forward;

            //anim.SetBool("Grounded", false);

            //TODO: Remove parent and set to null
            //transform.SetParent(null);
        }

        if (controller.isGrounded) {
            anim.SetBool("Grounded", true);
            velocity.y = Mathf.Clamp(velocity.y, -0.5f, float.MaxValue);
            if (skills.ableToDoubleJump) {
                canDoubleJump = true;
            }
        } else {
            Debug.Log("Not grounded");
            anim.SetBool("Grounded", false);
        }
	}

	private void CheckForClimbableWalls(Vector3 dir) {
		RaycastHit hit;
		Debug.DrawRay(transform.position + characterCenterCast, transform.forward * wallCastLength, Color.cyan);
		if(!isGrounded && Physics.Raycast(transform.position + characterCenterCast, dir, out hit, wallCastLength, jumpableWallMask)) {
			wallNormal = hit.normal;
			onWall = true;
			if (skills.ableToWallJump) {
				canWallJump = true;
			}

            //transform.SetParent(hit.collider.transform);
        } else {
			canWallJump = false;
			onWall = false;
		}  
	}

    private void CheckForMovableObject(Vector3 dir) {
        RaycastHit hit;
        Debug.DrawRay(transform.position + characterCenterCast, dir * pushCastLength, Color.yellow);
        if(Physics.Raycast(transform.position + characterCenterCast, dir, out hit, pushCastLength, pushMask)) {
            currentBlock = hit.collider.GetComponent<PushableBlock>();
            blockNormal = hit.normal;
        } else {
            currentBlock = null;
        }
    }

	public void AnimRespawn() {
		anim.SetTrigger("Respawn");
	}

    private void ResetAnimatorCues() {
        anim.SetBool("Pushing", false);
    }

    private bool AnimAbleToJump() {
        return currentBaseStateInfo.fullPathHash == locomotionString_ID;
    }
}