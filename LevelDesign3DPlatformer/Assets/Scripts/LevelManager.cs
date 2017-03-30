using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    private static LevelManager instance;

    [SerializeField]
    private int startingSceneID;

    public static LevelManager Instance {
        get { return instance; }
    }

    private void Awake() {
        if(instance != null) {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadLevel(int id) {
        SceneManager.LoadScene(id); 
    }

    public void LoadLevel(string levelName) {
        SceneManager.LoadScene(levelName);
    }

    public void LoadStartScreen() {
        LoadLevel(startingSceneID);
    }
}
