using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDelay : MonoBehaviour {

    public int levelID;
    public float delay;

	// Use this for initialization
	void Start () {
        StartCoroutine(LoadCoroutine());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator LoadCoroutine() {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.LoadLevel(levelID);
    }
}
