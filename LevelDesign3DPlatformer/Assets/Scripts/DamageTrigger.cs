using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageTrigger : MonoBehaviour {

    public Character owner;
    public int amt;

    public void OnTriggerEnter(Collider other) {
        Character otherChar = other.GetComponent<Character>();
        if(otherChar != null && otherChar != owner) {
            otherChar.Damage(amt);
        }
    }
}
