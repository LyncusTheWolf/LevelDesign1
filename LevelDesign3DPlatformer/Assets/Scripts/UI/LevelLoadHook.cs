using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoadHook : MonoBehaviour {

	public void LoadFirstLevel() {
        LevelManager.Instance.LoadLevel(1);
    } 
}
