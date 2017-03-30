using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Driver : MonoBehaviour {

    public delegate void OnExit();

    public event OnExit onExit;

    [SerializeField]
    private UI_Panel initialUIPanel;

    private UI_Panel activeUIPanel;

    private void Awake() {
        Init();
    }

    public void Init() {
        activeUIPanel = initialUIPanel;
        initialUIPanel.Init();
    }

    public void ToggleUIPanel(UI_Panel newPanel) {
        activeUIPanel.Close();
        newPanel.Init();
        activeUIPanel = newPanel;
    }
}
