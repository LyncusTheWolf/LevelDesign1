using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour {

    [SerializeField]
    public Transform endPoint;

    private void Awake() {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            other.transform.position = endPoint.position;
            other.transform.rotation = endPoint.rotation;
            ThirdPersonCamera.Instance.ResetPosition();
        }
    }
}
