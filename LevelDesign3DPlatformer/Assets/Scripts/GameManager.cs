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

    public event OnGameOver gameOverEvent;

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

    private void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        ResetLevel();
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

    public void ResetLevel() {
        playerLives = PLAYER_STARTING_LIVES;
        secretsOnLevel = 0;
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
    }
}
