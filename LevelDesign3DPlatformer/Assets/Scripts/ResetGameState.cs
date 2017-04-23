using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGameState : MonoBehaviour {

	
    public void ResetGameFunction() {
        GameManager.Instance.ResetGameState();
    }
}
