using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occelate : MonoBehaviour {

    public float speed;
    public Vector3 axis;

	// Update is called once per frame
	void Update () {
        Vector3 temp = transform.localPosition;
        temp = axis * Mathf.Sin(Time.time * Mathf.PI * speed);
        transform.localPosition = temp;

    }
}
