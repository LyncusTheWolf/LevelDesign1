using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class JumpPad : MonoBehaviour {

    [SerializeField]
    private Vector3 jumpForce;

    public void Awake() {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            //Rigidbody rigidbody = other.GetComponent<Rigidbody>();
            //Vector3 temp = rigidbody.velocity;
            //temp.y = jumpForce;
            //rigidbody.velocity = jumpForce;
            CharacterMotor motor = other.GetComponent<CharacterMotor>();
            motor.Velocity = jumpForce;
        }
    }
}
