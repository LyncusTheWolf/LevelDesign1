using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WarpZone : MonoBehaviour {

    public Transform endPoint;

    private void Awake() {
        GetComponent<SphereCollider>().isTrigger = true;
    }


    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            other.transform.position = endPoint.position;
            other.transform.rotation = endPoint.rotation;
            ThirdPersonSmartCamera.Instance.ResetPosition();
        }
    }
}
