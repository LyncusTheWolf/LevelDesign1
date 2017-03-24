using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
//[RequireComponent(typeof(Rigidbody))]
public class PushableBlock : MonoBehaviour {

    //TODO Fix push blocks

    [SerializeField]
    private float skinThickness;

    private Vector3 currentPush;
    private float fallVelocity;
    private BoxCollider boxCollider;
    //public Rigidbody rigidBody;

    private void Awake() {
        boxCollider = GetComponent<BoxCollider>();
        //rigidBody = GetComponent<Rigidbody>();
    }

    public void Update() {
        fallVelocity -= GameManager.Instance.gravity * Time.fixedDeltaTime;

        RaycastHit hit;
        if(Physics.BoxCast(transform.position + boxCollider.center, boxCollider.size / 2.0f - skinThickness * Vector3.one, currentPush, out hit, Quaternion.identity, currentPush.magnitude * Time.deltaTime + skinThickness / 2.0f)) {
            //Debug.Log(hit.normal);
            //Debug.Log(hit.collider.name);
            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            //transform.Translate(currentPush.normalized * hit.distance);
        } else {
            transform.Translate(currentPush * Time.deltaTime);
        }

        currentPush = Vector3.zero;
    }

    public void SetNextFrameDirection(Vector3 pushStrength) {
        currentPush = pushStrength;
    }
}
