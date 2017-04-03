using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonSmartCamera : MonoBehaviour {

    public const float INPUT_DEADZONE = 0.05f;

    private static ThirdPersonSmartCamera instance;

    public Player player;

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

    [SerializeField]
    private int whiskerCount;
    [SerializeField]
    private float whiskerAngleSpread;
    
    private Camera cam;
    private float currentPitch;
    private float currentYaw;
    private CharacterMotor motor;
    private Vector3 velocityCamSmooth = Vector3.zero;
    private Vector3 frameOffset;
    private Vector3 targetLastFrame;

    RaycastHit whiskerHit;
    Quaternion whiskerDirection = Quaternion.identity;

    public static ThirdPersonSmartCamera Instance {
        get { return instance; }
    }

    private void Awake() {
        if (instance != null) {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        cam = GetComponent<Camera>();
        if(wallCollisionOffset < cam.nearClipPlane + 0.05f) {
            //Make sure the wall collision offset is at minimum slightly bigger than the cameras near clip plane
            wallCollisionOffset = cam.nearClipPlane + 0.05f;
        }
    }

    // Use this for initialization
    void Start () {
        LoadTarget(target);

        currentPitch = startPitch;
        currentYaw = target.rotation.eulerAngles.y;
	}

    private void LateUpdate() {
        if(player == null || player.IsAlive == false) {
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float rightHorizontal = Input.GetAxis("RightHorizontal");
        float rightVertical = Input.GetAxis("RightVertical");

        frameOffset = target.position + lookOffset;

        Vector3 temp = motor.Velocity;
        temp.y = 0;

        //currentPitch = transform.rotation.x;
        //currentYaw = transform.rotation.y;

        //Debug.Log(Vector3.Distance(frameOffset, transform.position));

        float rHorzAbs = Mathf.Abs(rightHorizontal);
        float rVertAbs = Mathf.Abs(rightVertical);

        if (temp.magnitude > 0.3f && rVertAbs <= INPUT_DEADZONE) {
            currentPitch = Mathf.Lerp(currentPitch, startPitch, Time.deltaTime * camSmoothSpeed);
        } else if (rVertAbs > INPUT_DEADZONE) {
            currentPitch = Mathf.Clamp(currentPitch + Time.deltaTime * camAdjustSpeed * rightVertical, pitchMinValue, pitchMaxValue);
        }

        if (!PreditiveCollisionCheck()) {          
            if (temp.magnitude > 0.3f && rHorzAbs <= INPUT_DEADZONE) {
                //???
                //Fix lerping functionality for target Yaw
                //float lerpValue = Mathf.Abs(Vector3.Dot(Vector3.ProjectOnPlane(transform.forward, Vector3.up), target.forward));
                //Debug.Log(lerpValue);
                //float targetYaw = Mathf.Lerp(target.rotation.eulerAngles.y + (vertical < 0 ? 90.0f : -90.0f), target.rotation.eulerAngles.y + (horizontal < 0 ? 180.0f: 0.0f), lerpValue);

                //currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * camSmoothSpeed);
                currentYaw = Mathf.LerpAngle(currentYaw, target.rotation.eulerAngles.y, Time.deltaTime * camSmoothSpeed);
            } else if (rHorzAbs > INPUT_DEADZONE) {
                //currentYaw = Mathf.Clamp(currentYaw - Time.deltaTime * camAdjustSpeed * rightHorizontal, 0.0f, 360.0f);
                currentYaw -= Time.deltaTime * camAdjustSpeed * rightHorizontal;
            }
        }  

        Quaternion frameDir = Quaternion.Euler(currentPitch, currentYaw, 0.0f);

        Vector3 targetPos = frameOffset - frameDir * Vector3.forward * followDistance;

        Debug.DrawLine(frameOffset, targetPos, Color.blue);

        if(CompensateForOcclusion(frameOffset, ref targetPos, wallCollisionOffset)) {
            SmoothPosition(transform.position, targetPos);
            Debug.LogWarning("Smart Camera requires fine tuning in the late update");
            Debug.Log("First Occulision Smooth");
            if (CompensateForOcclusion(transform.position, ref frameOffset, wallCollisionOffset)) {
                Debug.Log("Calling Occulision");
                transform.position = targetPos;
            }
        } else {
            SmoothPosition(transform.position, targetPos);
        }        

        //Vector3.Distance(frameOffset, transform.position) > followDistance

        //transform.position = targetPos;

        transform.LookAt(frameOffset);
    }

    private void LoadTarget(Transform t) {
        target = t;
        motor = t.GetComponent<CharacterMotor>();
    }

    /// <summary>
    /// Checks the directions whiskering out from the target and updates the direction of the camera relative to any collisions detected
    /// </summary>
    /// <returns></returns>
    private bool PreditiveCollisionCheck() {
        bool whiskerCollision = false;

        //Check left whiskers
        for (int i = whiskerCount; i > 0; i--) {
            if (WhiskerSubFunction(i)) {
                whiskerCollision = true;
                break;
            }
        }

        //Check right whiskers
        for (int i = -whiskerCount; i < 0; i++) {
            if (WhiskerSubFunction(i)) {
                whiskerCollision = true;
                break;
            }
        }

        /*if(Physics.Raycast(transform.position, frameOffset - transform.position, Vector3.Distance(transform.position, frameOffset), collisionsMask)) {
            if(transform.position.y > frameOffset.y) {
                currentPitch += Time.deltaTime * camAdjustSpeed;
            } else {
                currentPitch -= Time.deltaTime * camAdjustSpeed;
            }
        }*/

        return whiskerCollision;
    }

    private bool WhiskerSubFunction(int index) {
        whiskerDirection = Quaternion.Euler(currentPitch, currentYaw + index * whiskerAngleSpread, 0.0f);

        Vector3 target = frameOffset - whiskerDirection * Vector3.forward * followDistance;
        Debug.DrawLine(frameOffset, target, Color.yellow);

        if (Physics.Linecast(frameOffset, target, out whiskerHit, collisionsMask)) {
            currentYaw -= (1 - whiskerHit.distance / followDistance) * Time.deltaTime * camAdjustSpeed * index;
            return true;
        }

        return false;
    }

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos) {
        Vector3 targetLocation = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime * (Vector3.Distance(frameOffset, transform.position) > followDistance ? 1.0f : 2.5f));

        transform.position = targetLocation;

        /*Collider[] cols = Physics.OverlapSphere(targetLocation, cam.nearClipPlane);
        if(cols.Length == 0) {
            transform.position = targetLocation;
        }*/
    }

    private bool CompensateForOcclusion(Vector3 fromObject, ref Vector3 toTarget, float offset) {

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
            return true;
        }

        return false;
    }

    public void ResetPosition() {
        Debug.Log("Reseting camera");
        currentPitch = startPitch;
        currentYaw = target.rotation.eulerAngles.y;

        Quaternion frameDir = Quaternion.Euler(currentPitch, currentYaw, 0.0f);

        transform.position = target.position + lookOffset - frameDir * Vector3.forward * followDistance;
    }
}
