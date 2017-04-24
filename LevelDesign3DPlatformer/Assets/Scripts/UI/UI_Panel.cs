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

    [SerializeField]
    private UI_Panel previousPanel;

    private UI_Driver attachDriver;

    public virtual void Update() {
        if (previousPanel != null && Input.GetButtonDown("Cancel")) {
            attachDriver.ToggleUIPanel(previousPanel);
        }
    }

    public void Init(UI_Driver driverHandle) {
        attachDriver = driverHandle;

        if (initialElement != null) {
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
