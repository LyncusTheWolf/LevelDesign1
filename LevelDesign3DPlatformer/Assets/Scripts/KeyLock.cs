using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class KeyLock : MonoBehaviour {

    [SerializeField]
    private UnityEvent onUnlock;

    private void Awake() {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            if (GameManager.Instance.UseKey()) {
                onUnlock.Invoke();
                gameObject.SetActive(false);
            }
        }
    }
}
