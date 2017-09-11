using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSet : MonoBehaviour {

    [SerializeField]
    private UI_Panel initialUIPanel;

    public UI_Panel InitialPanel {
        get { return initialUIPanel; }
    }
}
