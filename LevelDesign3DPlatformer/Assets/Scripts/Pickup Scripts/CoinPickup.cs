using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CoinPickup : MonoBehaviour {

    [SerializeField]
    private GameObject pickupParticles;

    private void Awake() {
        GetComponent<SphereCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            other.GetComponent<Player>().AddCoin(1);
            Instantiate(pickupParticles, transform.position + Vector3.up, transform.rotation);
            Destroy(gameObject);
        }
    }
}
