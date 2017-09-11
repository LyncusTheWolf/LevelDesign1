using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour {

    public UI_Panel mainPanel;
    public UI_Panel pausePanel;

    private void OnEnable() {
        GameManager.Instance.pauseEvent += InitPause;
        GameManager.Instance.unPauseEvent += InitMain;
    }

    private void OnDisable() {
        GameManager.Instance.pauseEvent -= InitPause;
        GameManager.Instance.unPauseEvent -= InitMain;
    }   

    private void InitPause() {
        UI_Driver.Instance.ToggleUIPanel(pausePanel);
    }

    private void InitMain() {
        UI_Driver.Instance.ToggleUIPanel(mainPanel);
    }

    public void TogglePause() {
        GameManager.Instance.TogglePause();
    }
}
