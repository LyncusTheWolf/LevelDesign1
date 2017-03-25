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

	//private Vector3 groundSlope;
	private GroundHit groundValues;

	public Vector3 Velocity{
		get{ return velocity; }
		set{ velocity = value; }
	}

	public void Awake(){
		controller = GetComponent<CharacterController> ();
	}

	public void Update(){
		CheckGrounding();

		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
		Vector3 dir = CameraRelativeInput(input);
		float oldVertical = velocity.y;
		moveDelta = velocity;
		oldVertical -= GameManager.Instance.gravity * Time.deltaTime;
		float oldMagnitude = dir.magnitude;

		CheckForClimbableWalls(dir);

        CheckForMovableObject(dir);

        dir = (dir - groundValues.normal * Vector3.Dot(dir, groundValues.normal)).normalized * oldMagnitude;

		//Debug.DrawRay(transform.position, dir * 100.0f, Color.yellow);

		moveDelta = Vector3.Lerp(moveDelta, dir * (controller.isGrounded? moveSpeed : aerialSpeed), Time.deltaTime * (controller.isGrounded || onWall? groundMomentumChangeSpeed : aerialMomentumChangeSpeed));

        if (Input.GetButtonDown("Jump")) {
            if (!skills.ableToJump) {
                Debug.Log("Player is not able to jump yet");
            } else if (isGrounded) {
                JumpInternal();
            } else if (canWallJump) {
                //moveDelta = Vector3.Reflect(moveDelta, wallNormal);                     //Reflect the move direction across the walls normal
                moveDelta = wallNormal;
                moveDelta.y = 0;                                                        //Temporarily kill the y to get the direction that the character is jumping from
                moveDelta = moveDelta.normalized * wallJumpSpeed;                      //Use this direction to create a speed that the character is jumping off the wall from
                onWall = false;
                anim.SetTrigger("Wall Jump");
                JumpInternal();                                               //Reapply the y value as a jump vertical speed
            } else if (!isGrounded && canDoubleJump) {
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
		anim.SetTrigger("Jump");
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
            velocity.y = Mathf.Clamp(velocity.y, 0.0f, float.MaxValue);
            if (skills.ableToDoubleJump) {
                canDoubleJump = true;
            }
        } else {
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
}

#region Old Code
/*
[RequireComponent(typeof(CharacterController))]
public class CharacterMotor : MonoBehaviour {

    public SkillSet skills;

    [SerializeField]
    private Transform visualObject;

    [SerializeField]
    private float groundSpeed;
    [SerializeField]
    private float aerialSpeed;
    [SerializeField]
    private float gravity;
    [SerializeField]
    private float jumpSpeed;
    [SerializeField]
    private float aerialMomentum;

    [SerializeField]
    private ParticleSystem particleTrailer;

    public Vector3 velocity;

    private CharacterController controller;
    private Vector3 moveVector;
    private bool canJump;
    private bool canDoubleJump;
    private float verticalVelocity;
    private Vector3 groundedVelocity;
    private Vector3 wallNormal;
    private bool onWall;

    private Animator anim;

    private void Awake() {
        controller = GetComponent<CharacterController>();
        anim = visualObject.GetComponent<Animator>();
        //particleTrailer.Stop();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        moveVector = Vector3.zero;

        Vector3 input = Vector3.zero;
        input.x = Input.GetAxis("Horizontal");
        input.z = Input.GetAxis("Vertical");
        input = Vector3.ClampMagnitude(input, 1.0f);
        input = CameraRelativeInput(input);

        if (controller.isGrounded) {
            moveVector = input;
            moveVector *= groundSpeed;

            //EvaluateParticleTrailer(input);
        } else {
            moveVector = groundedVelocity;
            moveVector += input * aerialSpeed;
        }

        verticalVelocity = verticalVelocity - gravity * Time.deltaTime;
        if (Input.GetButtonDown("Jump")) {
            if (onWall) {
                Vector3 reflection = Vector3.Reflect(velocity, wallNormal);
                Vector3 projected = Vector3.ProjectOnPlane(reflection, Vector3.up);
                groundedVelocity = projected.normalized * groundSpeed + wallNormal * aerialSpeed;
            }

            if (canJump) {
                canJump = false;
                verticalVelocity += jumpSpeed;
            } else if (canDoubleJump) {
                canDoubleJump = false;
                verticalVelocity += jumpSpeed;
            }
        }

        moveVector.y = verticalVelocity;
        velocity = moveVector;

        anim.SetFloat("Velocity", groundedVelocity.magnitude);

        VisualAllign(moveVector);
        CollisionFlags flags = controller.Move(moveVector * Time.deltaTime);

        if ((flags & CollisionFlags.Below) != 0) {
            //Kill the y values on total velicity and assign this to grounded velocity

            Vector3 temp = velocity;
            temp.y = 0;
            groundedVelocity = temp;
            verticalVelocity = -3.0f;
            if (skills.ableToJump) {
                canJump = true;
            }

            if (skills.ableToDoubleJump) {
                canDoubleJump = true;
            }

            onWall = false;
            Debug.Log("Is grounded");
        } else if ((flags & CollisionFlags.Sides) != 0) {
            if (skills.ableToJump) {
                canJump = true;
            }

            if (skills.ableToWallJump) {
                onWall = true;
            }
        } else if ((flags & CollisionFlags.Above) != 0) {
            verticalVelocity = 0;
        } else {
            canJump = false;
            onWall = false;
        }
    }

    private void VisualAllign(Vector3 dir) {
        dir.y = 0;
        transform.LookAt(transform.position + dir);
    }

    //TODO fix later
    private void EvaluateParticleTrailer(Vector3 input) {
        if (input.magnitude > 0.2f && !particleTrailer.isPlaying) {
            Debug.Log("Play trailer");
            particleTrailer.Play();
        } else if(input.magnitude < 0.2f && particleTrailer.isPlaying) {
            particleTrailer.Stop();
        }
    }

    private Vector3 CameraRelativeInput(Vector3 input) {
        Vector3 root = transform.forward;
        Vector3 camDirection = Camera.main.transform.forward;
        camDirection.y = 0.0f; // kill Y
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(camDirection));

        Vector3 moveDirection = referentialShift * input;

        return moveDirection;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        wallNormal = hit.normal;
    }
}*/
#endregion