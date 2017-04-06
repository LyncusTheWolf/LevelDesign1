using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LevelEndPoint : MonoBehaviour {

    private void Awake() {
        GetComponent<Collider>().isTrigger = true;
    }


    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            Debug.Log("You beat the level");
        }
    }
}
