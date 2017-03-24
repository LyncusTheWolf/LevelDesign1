using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class ButtonTrigger : MonoBehaviour {

    public bool fireOnce;

    public UnityEvent onPress;

    private bool pressed;

    private void Awake() {
        GetComponent<BoxCollider>().isTrigger = true;
        pressed = false;
    }

    public void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" && (!pressed || !fireOnce)) {
            onPress.Invoke();
        }
    }
}
