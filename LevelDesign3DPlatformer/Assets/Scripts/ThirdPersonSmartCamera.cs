using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonSmartCamera : MonoBehaviour {

    public const float INPUT_DEADZONE = 0.05f;

    private static ThirdPersonSmartCamera instance;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float startPitch;
    [SerializeField]
    private float pitchMinValue;
    [SerializeField]
    private float pitchMaxValue;

    [SerializeField]
    private float followDistance;
    [SerializeField]
    private Vector3 lookOffset;

    [SerializeField]
    private float camAdjustSpeed;
    [SerializeField]
    private float camSmoothSpeed;

    [SerializeField]
    private float camSmoothDampTime = 0.1f;

    [Header("Occlusion Settings")]
    [SerializeField]
    private float wallCollisionOffset;
    [SerializeField]
    private LayerMask collisionsMask;


    private float currentPitch;
    private float currentYaw;
    private CharacterMotor motor;
    private Vector3 velocityCamSmooth = Vector3.zero;

    public static ThirdPersonSmartCamera Instance {
        get { return instance; }
    }

    private void Awake() {
        if (instance != null) {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    // Use this for initialization
    void Start () {
        LoadTarget(target);

        currentPitch = startPitch;
        currentYaw = target.rotation.eulerAngles.y;
	}

    private void LateUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float rightHorizontal = Input.GetAxis("RightHorizontal");
        float rightVertical = Input.GetAxis("RightVertical");

        Vector3 frameOffset = target.position + lookOffset;

        Vector3 temp = motor.Velocity;
        temp.y = 0;

        //currentPitch = transform.rotation.x;
        //currentYaw = transform.rotation.y;

        Debug.Log(Vector3.Distance(frameOffset, transform.position));

        float rHorzAbs = Mathf.Abs(rightHorizontal);
        float rVertAbs = Mathf.Abs(rightVertical);

        if (temp.magnitude > 0.3f && rHorzAbs <= INPUT_DEADZONE) {
            Debug.LogWarning("Smart Camera requires fine tuning in the late update");

            //Vector3 lookDir = Vector3.Lerp(followTransform.right * (leftX < 0 ? 1.0f : -1.0f), followTransform.forward * (leftY < 0 ? -1.0f : 1.0f), Mathf.Abs(Vector3.Dot(transform.forward, followTransform.forward)));

            float targetYaw = Mathf.Lerp(Quaternion.LookRotation(target.right).eulerAngles.y * (vertical < 0 ? 1.0f : -1.0f), target.rotation.eulerAngles.y, Mathf.Abs(Vector3.Dot(Vector3.ProjectOnPlane(transform.forward, Vector3.up), target.forward)));

            currentYaw = Mathf.LerpAngle(currentYaw, target.rotation.eulerAngles.y, Time.deltaTime * camSmoothSpeed);
            
        } else if(rHorzAbs > INPUT_DEADZONE) {
            //currentYaw = Mathf.Clamp(currentYaw - Time.deltaTime * camAdjustSpeed * rightHorizontal, 0.0f, 360.0f);
            currentYaw = currentYaw - Time.deltaTime * camAdjustSpeed * rightHorizontal;           
        }

        if(temp.magnitude > 0.3f && rVertAbs <= INPUT_DEADZONE) {
            currentPitch = Mathf.Lerp(currentPitch, startPitch, Time.deltaTime * camSmoothSpeed);
        } else if(rVertAbs > INPUT_DEADZONE) {
            currentPitch = Mathf.Clamp(currentPitch + Time.deltaTime * camAdjustSpeed * rightVertical, pitchMinValue, pitchMaxValue);
        }

        Quaternion frameDir = Quaternion.Euler(currentPitch, currentYaw, 0.0f);

        Vector3 targetPos = frameOffset - frameDir * Vector3.forward * followDistance;

        //Debug.DrawLine(frameOffset, targetPos, Color.blue);

        CompensateForOcclusion(frameOffset, ref targetPos, wallCollisionOffset);

        SmoothPosition(transform.position, targetPos);

        //Vector3.Distance(frameOffset, transform.position) > followDistance

        transform.position = targetPos;

        transform.LookAt(frameOffset);
    }

    private void LoadTarget(Transform t) {
        target = t;
        motor = t.GetComponent<CharacterMotor>();
    }

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos) {
        transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForOcclusion(Vector3 fromObject, ref Vector3 toTarget, float offset) {

        Debug.DrawLine(fromObject, toTarget, Color.cyan);

        RaycastHit hit = new RaycastHit();
        if (Physics.Linecast(fromObject, toTarget, out hit, collisionsMask)) {

            Debug.Log(hit.transform.name);
            Debug.DrawRay(hit.point, hit.normal, Color.red);

            toTarget = hit.point + hit.normal * offset;

            //Vector3 offsetDir = wallHit.point - fromObject;
            //offsetDir.Normalize();
            //offsetDir *= -offset;

            //toTarget = new Vector3(wallHit.point.x - offsetDir.x, toTarget.y, wallHit.point.z - offsetDir.z);
            //transform.position = toTarget;
        }
    }

    public void ResetPosition() {
        currentPitch = startPitch;
        currentYaw = target.rotation.eulerAngles.y;
    }
}
