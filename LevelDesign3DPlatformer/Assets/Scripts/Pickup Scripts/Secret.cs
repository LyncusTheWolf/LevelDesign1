using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Secret : MonoBehaviour {

    public void Awake() {
        GetComponent<Collider>().isTrigger = true;
    }

    public void Start() {
        GameManager.Instance.SubscribeSecret(this);
    }

    public void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            GameManager.Instance.CheckSecret(this);
            Destroy(this.gameObject);
        }
    }
}
