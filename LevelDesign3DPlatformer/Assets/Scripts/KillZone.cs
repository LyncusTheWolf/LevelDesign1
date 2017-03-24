using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class KillZone : MonoBehaviour {

    private void Awake() {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            other.GetComponent<Player>().Kill();
        }
    }
}
