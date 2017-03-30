using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_Panel : MonoBehaviour {

    [SerializeField]
    private Selectable initialElement;

    [SerializeField]
    private UnityEvent onInitialize;
    [SerializeField]
    private UnityEvent onClose;

    public void Update() {
        if (Input.GetButtonDown("Cancel")) {

        }
    }

    public void Init() {
        if(initialElement != null) {
            initialElement.Select();
        }

        //TODO: Resolve this issue later
        onInitialize.Invoke();
        gameObject.SetActive(true);
    }

    public void Close() {
        onClose.Invoke();
        gameObject.SetActive(false);
    }
}
