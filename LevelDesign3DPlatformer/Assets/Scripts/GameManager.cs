using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public enum SkillSet {
        AbleToJump,
        AbleToWallGrab,
        AbleToWallJump,
        AbleToDoubleJump,
        AbleToPushBlocks
    }

    public enum GameState {
        Inactive,
        Loading,
        Running,
        PlayerDead,
        Paused
    }

    public const int MAX_PLAYER_HEALTH = 16;
    public const int PLAYER_STARTING_HEALTH = 3;
    public const int PLAYER_STARTING_LIVES = 3;

    private static GameManager instance;
    private static RespawnPoint spawnPoint;

    public delegate void OnLevelStart();
    public delegate void OnAlive();
    public delegate void OnGameOver();
    public delegate void OnPause();

    public event OnLevelStart levelStartEvent;
    public event OnGameOver gameOverEvent;
    public event OnPause pauseEvent;
    public event OnPause unPauseEvent;

    #if UNITY_EDITOR
    [SerializeField]
    private bool debuggerMode;
#endif

    [SerializeField]
    private SceneDetails bootingSceneDetails;

    [SerializeField]
    private int startingSceneID;
    [SerializeField]
    private int levelCompleteSceneID;
    [SerializeField]
    private int gameOverSceneID;

    [SerializeField]
    private GameObject characterPrefab;

    //[SerializeField]
    //private Transform respawnPoint;
    [SerializeField]
    private float respawnDelay;

    public float gravity;
    private int playerLives;
    private Player player;
    private CharacterMotor motor;
    //private ThirdPersonCamera thirdPersonCam;
    private ThirdPersonSmartCamera thirdPersonCam;
    //private UI_Manager ui_manager;
    private int secretsOnLevel;
    private int secretsFound;
    [SerializeField] //Debug for now
    private bool[] playerSkillSet = new bool[Enum.GetValues(typeof(SkillSet)).Length];

    private float levelTimer;

    private int coins;
    private int keyCount;

    private bool paused;
    private bool invertCameraX;
    private bool invertCameraY;

    //private bool levelIsRunning;

    private GameState currentgameState = GameState.Inactive;

    public static GameManager Instance {
        get { return instance; }
    }

    public int PlayerLives {
        get { return playerLives; }
    }

    /*public Transform RespawnPoint {
        set { respawnPoint = value; }
    }*/

    public int SecretsOnLevel {
        get { return secretsOnLevel; }
    }

    public int SecretsFound {
        get { return secretsFound; }
    }

    public bool Paused {
        get { return paused; }
    }

    public int KeyCount {
        get { return keyCount; }
    }

    public int Coins {
        get { return coins; }
    }

    public bool InvertCameraX {
        get { return invertCameraX; }
        set { invertCameraX = value; }
    }

    public bool InvertCameraY {
        get { return invertCameraY; }
        set { invertCameraY = value; }
    }

    public GameState CurrentGameState {
        get { return currentgameState; }
    }

    /*public bool LevelIsRunning {
        get { return levelIsRunning; }
        set { levelIsRunning = value; }
    }*/

    public void RegisterSceneDetails(SceneDetails newDetails) {
        bootingSceneDetails = newDetails;
    }

    public static void SetRespawnPoint(RespawnPoint respawnPoint) {
        spawnPoint = respawnPoint;
    }

    private void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        ResetGameState();
    }

    private void Start() {
        if (debuggerMode) {
            //LevelInit();
            currentgameState = GameState.Running;
        }

        bootingSceneDetails.InitScene();
    }

    public void Update() {
        if (currentgameState == GameState.Running) {
            levelTimer += Time.deltaTime;
        }

        if (Input.GetButtonDown("Pause") && (currentgameState == GameState.Running || currentgameState == GameState.Paused)) {
            TogglePause();
        }
    }

    public void LevelInit() {

        //player = Player.Instance;
        //thirdPersonCam = ThirdPersonCamera.Instance;
        thirdPersonCam = ThirdPersonSmartCamera.Instance;
        //GameObject uiGO = Instantiate(uiManagerPrefab, Vector3.zero, Quaternion.identity);
        //ui_manager = uiGO.GetComponent<UI_Manager>();

        spawnPoint = bootingSceneDetails.StartingSpawn;
        SpawnPlayer();
        coins = 0;
        keyCount = 0;

        //motor = player.Motor;
        #if UNITY_EDITOR
        if (debuggerMode) {
            AddKey(99);
            /*motor.skills.ableToJump = true;
            motor.skills.ableToWallGrab = true;
            motor.skills.ableToWallJump = true;
            motor.skills.ableToDoubleJump = true;
            motor.skills.ableToPushBlocks = true;
            motor.ignoreJumpingCap = true;*/
        }
        #endif
    }

    public void AddCoin(int amt) {
        coins += amt;
        //TODO: Add in life increase functionality
    }

    public void AddKey(int amt) {
        keyCount += amt;
    }

    public bool UseKey() {
        if (keyCount > 0) {
            keyCount -= 1;
            return true;
        }

        return false;
    }

    public void PollLives() {
        playerLives--;

        if (playerLives > 0) {           
            StartCoroutine(RespawnPlayer(true));
        } else {
            GameOver();
        }
    }

    public void GameOver() {
        player.gameObject.SetActive(false);
        if (gameOverEvent != null) {
            gameOverEvent();
        }

        GameOverTransition();
    }       

    public void GameOverTransition() {
        StartCoroutine(GameOverInternal());
    }

    public IEnumerator GameOverInternal() {
        yield return new WaitForSeconds(3.0f);
        GameManager.Instance.LoadGameOverScreen();
    }

    public string GetTimeFormatted() {
        int minutes = (int)(levelTimer / 60);
        int seconds = (int)(levelTimer % 60);
        return minutes + ": " + seconds;
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

    public IEnumerator LoadLevelInternal(int levelID) {
        currentgameState = GameState.Loading;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelID, LoadSceneMode.Single);

        while (loadOperation.progress < 0.9f) {
            Debug.Log("Level load progress: " + loadOperation.progress);
            yield return null;
        }

        Debug.Log("Scene is done loading");

        while(spawnPoint == null && bootingSceneDetails == null) {
            yield return null;
        }
        LevelInit();
        FinalizedLoad();
    }

    private void FinalizedLoad() {        
        currentgameState = GameState.Running;
    }

    private IEnumerator RespawnPlayer(bool killOldObject) {
        if(player != null && killOldObject) {
            GameObject.Destroy(player.gameObject);
        }

        yield return new WaitForSeconds(respawnDelay);

        SpawnPlayer();

        /*player.ResetCharacter();
        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;
        player.gameObject.SetActive(true);
        motor.AnimRespawn();
        thirdPersonCam.ResetPosition();*/

        //StopCoroutine(RespawnPlayer());
    }

    public void SubscribeSecret(Secret secret) {
        secretsOnLevel++;
    }

    public void CheckSecret(Secret secret) {
        secretsFound++;
    }

    public void ResetGameState() {
        //Move else where
        playerSkillSet = new bool[Enum.GetValues(typeof(SkillSet)).Length];
        playerLives = PLAYER_STARTING_LIVES;
        secretsOnLevel = 0;
        secretsFound = 0;
        levelTimer = 0.0f;
    }

    public void TogglePause() {
        paused = !paused;

        if (paused) {
            currentgameState = GameState.Paused;
            Time.timeScale = 0.0f;
            pauseEvent();
        } else {
            currentgameState = GameState.Running;
            Time.timeScale = 1.0f;
            unPauseEvent();
        }
    }

    public bool KnowSkill(SkillSet skill) {
        return playerSkillSet[(int)skill];
    }

    public void LearnSkill(SkillSet skill) {
        playerSkillSet[(int)skill] = true;
    }

    public void SpawnPlayer() {
        GameObject go = Instantiate(characterPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        if(thirdPersonCam == null) {
            Debug.Log("Camera is null");
        }
        thirdPersonCam.RegisterCharacter(go);
        player = go.GetComponent<Player>();
        player.ResetCharacter();
        //thirdPersonCam.player = player;
        motor = go.GetComponent<CharacterMotor>();
        motor.AnimRespawn();
        //motor.skills = playerSkillSet;
    }
}
