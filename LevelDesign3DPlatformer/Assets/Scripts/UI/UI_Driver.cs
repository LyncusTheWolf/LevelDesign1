using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Driver : MonoBehaviour {

    private static UI_Driver instance;


    public delegate void OnExit();

    public event OnExit onExit;

    [SerializeField]
    private UI_Panel initialUIPanel;

    private UI_Panel activeUIPanel;

    public static UI_Driver Instance {
        get {
            #if UNITY_EDITOR
            if (instance == null) {
                Debug.Log("Scene does not contain a UI_Driver but an object is trying to access it. Remove the object trying to access a UI_Driver or add one to the scene");
            }

            return instance;
            #endif
        }
    }

    private void Awake() {
        if(instance != null && instance != this) {
            Destroy(this.gameObject);
            Debug.Log("This scene already contains a UI_Driver, merge over all UI elements into a single driver");
            return;
        }

        instance = this;

        Init();
    }

    public void Init() {
        activeUIPanel = initialUIPanel;
        initialUIPanel.Init(this);
    }

    public void ToggleUIPanel(UI_Panel newPanel) {
        activeUIPanel.Close();
        newPanel.Init(this);
        activeUIPanel = newPanel;
    }
}
