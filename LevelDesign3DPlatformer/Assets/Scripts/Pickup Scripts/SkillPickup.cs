using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class SkillPickup : MonoBehaviour {

    [SerializeField]
    private GameObject pickupParticles;

    private void Awake() {
        GetComponent<SphereCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            ModifyMotor(other.GetComponent<CharacterMotor>());
            Instantiate(pickupParticles, transform.position + Vector3.up, transform.rotation);
            Destroy(this.gameObject);
        }
    }

	protected abstract void ModifyMotor(CharacterMotor motor);
}
