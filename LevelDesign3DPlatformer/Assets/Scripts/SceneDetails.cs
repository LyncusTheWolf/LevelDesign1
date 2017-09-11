using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDetails : MonoBehaviour {

    [SerializeField]
    private GameObject screenSet;

    [SerializeField]
    private bool loadAsPlayable;

    [SerializeField]
    private RespawnPoint startingSpawn;

    public RespawnPoint StartingSpawn {
        get { return startingSpawn; }
    }

    public void Awake() {
        if(GameManager.Instance != null) {
            GameManager.Instance.RegisterSceneDetails(this);
        }
    }

    // Use this for initialization
    public void InitScene () {
        GameObject go = GameObject.Instantiate(screenSet);
        UI_Driver.Instance.LoadPanelSet(go.GetComponent<ScreenSet>());
        if (loadAsPlayable) {
            GameManager.Instance.LevelInit();
        }
	}
}
