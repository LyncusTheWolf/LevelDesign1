using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionHooks : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeInvertCameraX(bool newValue) {
        GameManager.Instance.InvertCameraX = newValue;
    }

    public void ChangeInvertCameraY(bool newValue) {
        GameManager.Instance.InvertCameraY = newValue;
    }
}
