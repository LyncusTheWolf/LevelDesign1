using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public const int MAX_PLAYER_HEALTH = 16;
    public const int PLAYER_STARTING_HEALTH = 3;
    public const int PLAYER_STARTING_LIVES = 3;

    private static GameManager instance;

    public delegate void OnAlive();
    public delegate void OnGameOver();
    public delegate void OnPause();

    public event OnGameOver gameOverEvent;
    public event OnPause pauseEvent;
    public event OnPause unPauseEvent;

    #if UNITY_EDITOR
    [SerializeField]
    private bool debuggerMode;
    #endif

    [SerializeField]
    private Transform respawnPoint;
    [SerializeField]
    private float respawnDelay;

    public float gravity;
    private int playerLives;
    private Player player;
    private CharacterMotor motor;
    //private ThirdPersonCamera thirdPersonCam;
    private ThirdPersonSmartCamera thirdPersonCam;
    private int secretsOnLevel;
    private int secretsFound;

    private float levelTimer;

    private bool paused;
    private bool invertCameraX;
    private bool invertCameraY;

    private bool levelIsRunning;

    public static GameManager Instance {
        get { return instance; }
    }

    public int PlayerLives {
        get { return playerLives; }
    }

    public Transform RespawnPoint {
        set { respawnPoint = value; }
    }

    public int SecretsOnLevel {
        get { return secretsOnLevel; }
    }

    public int SecretsFound {
        get { return secretsFound; }
    }

    public bool Paused {
        get { return paused; }
    }

    public bool InvertCameraX {
        get { return invertCameraX; }
        set { invertCameraX = value; }
    }

    public bool InvertCameraY {
        get { return invertCameraY; }
        set { invertCameraY = value; }
    }

    public bool LevelIsRunning {
        get { return levelIsRunning; }
        set { levelIsRunning = value; }
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

    public void Update() {
        if (levelIsRunning) {
            levelTimer += Time.deltaTime;
        }
    }

    public void LevelInit() {
        player = Player.Instance;
        //thirdPersonCam = ThirdPersonCamera.Instance;
        thirdPersonCam = ThirdPersonSmartCamera.Instance;
        motor = player.Motor;
        #if UNITY_EDITOR
        if (debuggerMode) {
            player.AddKey(99);
            motor.skills.ableToJump = true;
            motor.skills.ableToWallGrab = true;
            motor.skills.ableToWallJump = true;
            motor.skills.ableToDoubleJump = true;
            motor.skills.ableToPushBlocks = true;
            motor.ignoreJumpingCap = true;
        }
        #endif
    }

    public void PollLives() {
        playerLives--;

        if (playerLives > 0) {           
            StartCoroutine(RespawnPlayer());
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

    private IEnumerator RespawnPlayer() {
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        player.ResetCharacter();
        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;
        player.gameObject.SetActive(true);
        motor.AnimRespawn();       
        thirdPersonCam.ResetPosition();

        StopCoroutine(RespawnPlayer());
    }

    public void SubscribeSecret(Secret secret) {
        secretsOnLevel++;
    }

    public void CheckSecret(Secret secret) {
        secretsFound++;
    }

    public void ResetGameState() {
        playerLives = PLAYER_STARTING_LIVES;
        secretsOnLevel = 0;
        secretsFound = 0;
        levelTimer = 0.0f;
    }

    public void TogglePause() {
        paused = !paused;

        if (paused) {
            Time.timeScale = 0.0f;
            pauseEvent();
        } else {
            Time.timeScale = 1.0f;
            unPauseEvent();
        }
    }

    public void GameOverTransition() {
        StartCoroutine(GameOverInternal());
    }

    public IEnumerator GameOverInternal() {
        yield return new WaitForSeconds(3.0f);
        LevelManager.Instance.LoadGameOverScreen();
    }

    public string GetTimeFormatted() {
        int minutes = (int)(levelTimer / 60);
        int seconds = (int)(levelTimer % 60);
        return minutes + ": " + seconds;
    }
}
