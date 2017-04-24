using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputInvoker : MonoBehaviour {

    public string inputName;
    public UnityEvent onInput;

	// Update is called once per frame
	void Update () {
        if (Input.GetButton(inputName)) {
            onInput.Invoke();
        }
	}
}
