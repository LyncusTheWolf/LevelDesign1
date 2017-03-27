using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ParentOnTag : MonoBehaviour {

    [SerializeField]
    private List<string> tags;

    [SerializeField]
    private bool childCamera;

    private void Awake() {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnTriggerEnter(Collider other) {
        if (tags.Contains(other.tag) && other.transform.parent != this.transform) {
            other.transform.SetParent(this.transform);
            if (childCamera) {
                ThirdPersonSmartCamera.Instance.transform.SetParent(this.transform);
            }

            Debug.Log("Triggered");
        }
    }

    public void OnTriggerExit(Collider other) {
        if (tags.Contains(other.tag) && other.transform.parent == this.transform) {
            other.transform.SetParent(null);
            if (childCamera) {
                ThirdPersonSmartCamera.Instance.transform.SetParent(null);
            }
        }
    }
}
