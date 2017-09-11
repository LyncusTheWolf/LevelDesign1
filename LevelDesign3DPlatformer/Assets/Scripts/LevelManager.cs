using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    /*private static LevelManager instance;

    [SerializeField]
    private int startingSceneID;
    [SerializeField]
    private int levelCompleteSceneID;
    [SerializeField]
    private int gameOverSceneID;

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
        StartCoroutine(LoadLevelInternal(id));
        //SceneManager.LoadScene(id);
        //GameManager.Instance.ResetLevel(); 
    }

    public void LoadLevel(string levelName) {
        SceneManager.LoadScene(levelName);
        //GameManager.Instance.ResetLevel();
    }

    public void LoadStartScreen() {
        LoadLevel(startingSceneID);
    }

    public void LoadLevelComplete() {
        LoadLevel(levelCompleteSceneID);
    }

    public void LoadGameOverScreen() {
        LoadLevel(gameOverSceneID);
    }

    public IEnumerator LoadLevelInternal (int levelID) {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelID, LoadSceneMode.Single);
        loadOperation.allowSceneActivation = false;

        while(loadOperation.progress < 0.9f) {
            Debug.Log("Level load progress: " + loadOperation.progress);
            yield return null;
        }

        Debug.Log("Scene is done loading");

        loadOperation.allowSceneActivation = true;
    }*/
}
