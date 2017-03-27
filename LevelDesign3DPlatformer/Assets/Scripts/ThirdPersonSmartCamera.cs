using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonSmartCamera : MonoBehaviour {

    private static ThirdPersonSmartCamera instance;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float startPitch;

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

        currentPitch = Mathf.Clamp(currentPitch + Time.deltaTime * camAdjustSpeed * rightVertical, -30.0f, 60.0f);

        if (temp.magnitude > 0.3f && Mathf.Abs(rightHorizontal) < 0.05f) {
            Debug.LogWarning("Smart Camera requires fine tuning in the late update");

            float angleValue = Vector3.Angle(target.transform.forward, new Vector3(0.0f, currentYaw, 0.0f)) / 180.0f;

            currentYaw = Mathf.LerpAngle(currentYaw, target.rotation.eulerAngles.y, Time.deltaTime * camSmoothSpeed * 1- angleValue);
        } else {
            //currentYaw = Mathf.Clamp(currentYaw - Time.deltaTime * camAdjustSpeed * rightHorizontal, 0.0f, 360.0f);
            currentYaw = currentYaw - Time.deltaTime * camAdjustSpeed * rightHorizontal;
        }

        Quaternion frameDir = Quaternion.Euler(currentPitch, currentYaw, 0.0f);

        Vector3 targetPos = frameOffset - frameDir * Vector3.forward * followDistance;

        //Debug.DrawLine(frameOffset, targetPos, Color.blue);

        CompensateForOcclusion(frameOffset, ref targetPos, wallCollisionOffset);

        //SmoothPosition(transform.position, targetPos);

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
