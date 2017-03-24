using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DrawBridge : MonoBehaviour {

    private Animator anim;

    private bool isRaised;

    private void Awake() {
        anim = GetComponent<Animator>();
        isRaised = false;
    }

    public void LowerBridge() {
        if (isRaised) {
            anim.SetTrigger("LowerBridge");
            isRaised = true;
        }
    }

    public void RaiseBridge() {
        if (!isRaised) {
            anim.SetTrigger("RaiseBridge");
            isRaised = false;
        }
    }
}
