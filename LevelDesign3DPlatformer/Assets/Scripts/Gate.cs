﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Gate : MonoBehaviour {

    private Animator anim;

    public void Awake() {
        anim = GetComponent<Animator>();
    }

    public void OpenGate() {
        anim.SetTrigger("Open");
    }
}
