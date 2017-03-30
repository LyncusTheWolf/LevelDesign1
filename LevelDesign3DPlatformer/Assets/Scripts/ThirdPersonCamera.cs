using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

    private static ThirdPersonCamera instance;

    [Header("Debug only values")]
    public bool useSmoothing;

    [Header("Follow Constraints")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float followDistance;
    [SerializeField]
    private float followHeight;
    [SerializeField]
    private Vector3 lookOffset;

    [Header("Orbit Controls")]
    [SerializeField]
    private float orbitSpeed;

    [Header("Occlusion Settings")]
    [SerializeField]
    private float wallCollisionOffset;
    [SerializeField]
    private LayerMask collisionsMask;


    private Vector3 targetPosition;
    private Vector3 lookDir;
    private Vector3 curLookDir;

    [Header("Camera Dampening Settings")]
    [SerializeField]
    private float camSmoothDampTime = 0.1f;
    [SerializeField]
    private float lookDirDampTime = 0.1f;
    private Vector3 velocityCamSmooth = Vector3.zero;
    private Vector3 velocityLookDir = Vector3.zero;

    private CharacterMotor motor;   

    public static ThirdPersonCamera Instance {
        get { return instance; }
    }

    private void Awake() {
        if(instance != null) {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        motor = target.GetComponent<CharacterMotor>();
    }

	// Use this for initialization
	void Start () {
        lookDir = target.forward;
        curLookDir = target.forward;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LateUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float rightHorizontal = Input.GetAxis("RightHorizontal");

        //Debug.Log(rightHorizontal);

        Vector3 characterOffset = target.position + new Vector3(0.0f, followHeight, 0.0f);
        //Vector3 lookAt = characterOffset;

        //Debug.Log("Velocity: " + motor.velocity.magnitude);

        Vector3 temp = motor.Velocity;
        temp.y = 0;

        curLookDir = Quaternion.Euler(0.0f, -rightHorizontal * 2.5f, 0.0f) * curLookDir;

        if (temp.magnitude > 0.3f && useSmoothing && Mathf.Abs(rightHorizontal) < 0.05f) {
            //lookDir = Vector3.Lerp(followTransform.right * (leftX < 0 ? 1.0f : -1.0f), followTransform.forward * (leftY < 0 ? -1.0f : 1.0f), Mathf.Abs(Vector3.Dot(transform.forward, followTransform.forward)));

            lookDir = Vector3.Lerp(target.right * (horizontal < 0 ? 1.0f : -1.0f), target.forward * (vertical < 0 ? -1.0f : 1.0f), Mathf.Abs(Vector3.Dot(transform.forward, target.forward)));

            curLookDir = Vector3.Normalize(characterOffset - transform.position);
            curLookDir.y = 0.0f;

            curLookDir = Vector3.SmoothDamp(curLookDir, lookDir, ref velocityLookDir, lookDirDampTime);
        }

        //curLookDir = Quaternion.Euler(0.0f, orbitSpeed * Time.deltaTime * -rightHorizontal, 0.0f) * curLookDir;

        targetPosition = target.position + transform.up * followHeight - Vector3.Normalize(curLookDir) * followDistance;

        CompensateForOcclusion(target.position + lookOffset, ref targetPosition, wallCollisionOffset);

        //Smoothly interpolate towards the target position based on delta time
        SmoothPosition(this.transform.position, targetPosition);

        transform.LookAt(target.position + lookOffset);
    }

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos) {
        this.transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForOcclusion(Vector3 fromObject, ref Vector3 toTarget, float offset) {

        Debug.DrawLine(fromObject, toTarget, Color.cyan);

        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(fromObject, toTarget, out wallHit, collisionsMask)) {
            //Debug.Log(wallHit.transform.name);
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);

            Vector3 offsetDir = wallHit.point - fromObject;
            offsetDir.Normalize();
            offsetDir *= offset;

            toTarget = new Vector3(wallHit.point.x - offsetDir.x, toTarget.y, wallHit.point.z - offsetDir.z);
            //transform.position = toTarget;
        }
    }

    public void ResetPosition() {
        transform.position = target.position + transform.up * followHeight - target.forward * followDistance;
        transform.LookAt(target);
    }
}
