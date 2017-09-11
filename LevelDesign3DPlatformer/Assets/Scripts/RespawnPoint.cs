using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RespawnPoint : MonoBehaviour {

    [SerializeField]
    private GameObject visual;

    private bool active;

    private void Awake() {
        GetComponent<SphereCollider>().isTrigger = true;
        active = true;
    }

    public void OnTriggerEnter(Collider other) {
        if (active && other.tag == "Player") {
            //GameManager.Instance.RespawnPoint = this.transform;
            GameManager.SetRespawnPoint(this);
            active = false;
            visual.SetActive(false);
        }
    }
}
