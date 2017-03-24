using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {

    [SerializeField]
    private Transform followTarget;
    [SerializeField]
    private float followDistance;
    [SerializeField]
    private float turnSpeed;
    [SerializeField]
    private float rotationSmoothTime;
    [SerializeField]
    private float followSmoothing;
    [SerializeField]
    private float initialPitch;
    [SerializeField]
    private float minPitch;
    [SerializeField]
    private float maxPitch;

    
    private float currentYaw;
    private float currentPitch;
    private Vector3 currentDir;
    private Vector3 rotationalVelocity;

    private CharacterMotor motor;

    private void Awake() {
        motor = followTarget.GetComponent<CharacterMotor>();
    }

    // Use this for initialization
    void Start () {
        currentPitch = initialPitch;
        currentYaw = followTarget.eulerAngles.y;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LateUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float rightHorizontal = Input.GetAxis("RightHorizontal");
        float rightVertical = Input.GetAxis("RightVertical");

        Debug.Log(horizontal + "_" + vertical);

        currentYaw -= rightHorizontal * turnSpeed;
        currentPitch -= rightVertical * turnSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

		Vector3 temp = Vector3.zero; //motor.velocity;
        temp.y = 0;

        if(temp.magnitude > 0.2f) {
        }
            currentYaw = Mathf.Lerp(currentYaw, followTarget.eulerAngles.y, Time.deltaTime * followSmoothing);

        currentDir = Vector3.SmoothDamp(currentDir, new Vector3(currentPitch, currentYaw), ref rotationalVelocity, rotationSmoothTime);
        transform.eulerAngles = currentDir;

        transform.position = followTarget.position - transform.forward * followDistance;

        //transform.LookAt(followTarget);
    }
}
