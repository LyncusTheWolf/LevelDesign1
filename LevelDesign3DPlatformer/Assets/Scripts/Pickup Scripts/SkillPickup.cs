using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SkillPickup : MonoBehaviour {

    public GameManager.SkillSet skillOnPickup;

    [SerializeField]
    private GameObject pickupParticles;

    private void Awake() {
        GetComponent<SphereCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            GameManager.Instance.LearnSkill(skillOnPickup);
            Instantiate(pickupParticles, transform.position + Vector3.up, transform.rotation);
            Destroy(this.gameObject);
        }
    }
}
