using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterRigidbodyMotor : MonoBehaviour {

    //Motor internal struct used to allow the input to be captured between physic update ticks
    public struct InputCache {
        public bool jumpDown;
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
    private Vector3 wallCastOffset;
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

    [Header("Visual Effects")]
    [SerializeField]
    private ParticleSystem doubleJumpBurst;

    [HideInInspector]
    public bool ignoreJumpingCap = false;

    //Private cache
    private Rigidbody rigidBody;
    private new CapsuleCollider collider;
    private Vector3 moveDelta;
    private bool onWall;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool canWallJump;
    private Vector3 wallNormal;

    private InputCache inputCache;

    //private Vector3 groundSlope;
    private GroundHit groundValues;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetButtonDown("Jump")){
            inputCache.jumpDown = true;
        }
    }

    private void FixedUpdate() {
        CheckGrounding();

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        Vector3 dir = CameraRelativeInput(input);
        float oldVertical = rigidBody.velocity.y;
        moveDelta = rigidBody.velocity;
        oldVertical -= GameManager.Instance.gravity * Time.fixedDeltaTime;
        float oldMagnitude = dir.magnitude;

        //transform.LookAt(transform.position + dir);

        //Lerp the transform direction?
        /*if(oldMagnitude >= 0.01f && isGrounded) {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), Time.fixedDeltaTime * turningSpeed);
        }*/

        CheckForClimbableWalls(dir);

        //dir = Quaternion.Euler(groundValues.angleAxis * 90.0f) * dir;
        dir = (dir - groundValues.normal * Vector3.Dot(dir, groundValues.normal)).normalized * oldMagnitude;

        //dir = (transform.forward - groundValues.normal * Vector3.Dot(transform.forward, groundValues.normal)).normalized * oldMagnitude;

        Debug.DrawRay(transform.position, dir * 100.0f, Color.yellow);

        moveDelta = Vector3.Lerp(moveDelta, dir * (isGrounded? moveSpeed : aerialSpeed), Time.fixedDeltaTime * (isGrounded || onWall? groundMomentumChangeSpeed : aerialMomentumChangeSpeed));

        if(inputCache.jumpDown) {
            if (!skills.ableToJump) {
                Debug.Log("Player is not able to jump yet");
            } else if (isGrounded) {
                JumpInternal();
            } else if(canWallJump) {
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

        rigidBody.velocity = moveDelta;

        //Clear the input cache
        ResetInputs();
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
            if (skills.ableToDoubleJump) {
                canDoubleJump = true;
            }

            groundValues.normal = hit.normal;
            groundValues.angleAxis = Vector3.Cross(groundValues.normal, transform.up);
            groundValues.slopeDirection = Vector3.Cross(groundValues.normal, groundValues.angleAxis);

            anim.SetBool("Grounded", true);

            //Debug.DrawRay(transform.position, groundValues.normal * 4, Color.cyan);
            //Debug.DrawRay(transform.position, groundValues.angleAxis * 4, Color.blue);
            //Debug.DrawRay(transform.position, groundValues.slopeDirection * 4, Color.green);
        } else {
            Debug.Log("No ground");
            isGrounded = false;
            groundValues.normal = transform.up;
            groundValues.angleAxis = Vector3.zero;
            groundValues.slopeDirection = -transform.forward;

            anim.SetBool("Grounded", false);
        }
    }

    private void CheckForClimbableWalls(Vector3 dir) {
        RaycastHit hit;
        Debug.DrawRay(transform.position + wallCastOffset, transform.forward * wallCastLength, Color.cyan);
        if(Physics.Raycast(transform.position + wallCastOffset, dir, out hit, wallCastLength, jumpableWallMask)) {
            wallNormal = hit.normal;
            onWall = true;
            if (skills.ableToWallJump) {
                canWallJump = true;
            }
        } else {
            canWallJump = false;
            onWall = false;
        }  
    }

    private void ResetInputs() {
        inputCache.jumpDown = false;
    }
    
    public void AnimRespawn() {
        anim.SetTrigger("Respawn");
    } 
}
