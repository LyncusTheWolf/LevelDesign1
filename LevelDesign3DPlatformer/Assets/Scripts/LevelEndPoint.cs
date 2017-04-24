using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LevelEndPoint : MonoBehaviour {

    public static LevelEndPoint instance;

    private void Awake() {
        instance = this;
        GetComponent<Collider>().isTrigger = true;
    }

    public void Start() {
        GameManager.Instance.LevelIsRunning = true;
    }


    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            GameManager.Instance.LevelIsRunning = false;
            LevelManager.Instance.LoadLevelComplete();
        }
    }
}
