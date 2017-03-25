using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public const int MAX_PLAYER_HEALTH = 16;
    public const int PLAYER_STARTING_HEALTH = 3;

    private static GameManager instance;

    public delegate void OnAlive();

    #if UNITY_EDITOR
    [SerializeField]
    private bool debuggerMode;
#endif

    [SerializeField]
    private Transform respawnPoint;

    public float gravity;
    private int playerLives;
    private Player player;
    private CharacterMotor motor;
    private ThirdPersonCamera thirdPersonCam;

    public Transform RespawnPoint {
        set { respawnPoint = value; }
    }

    public static GameManager Instance {
        get { return instance; }
    }

    private void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        playerLives = 3;
    }

    public void Start() {
        player = Player.Instance;
        thirdPersonCam = ThirdPersonCamera.Instance;
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
        if(playerLives > 0) {
            playerLives--;
            RespawnPlayer();
        } else {
            GameOver();
        }
    }

    public void GameOver() {

    }

    private void RespawnPlayer() {
        motor.AnimRespawn();
        player.ResetCharacter();
        //rigidBody.velocity = Vector3.zero;
        Debug.Log(respawnPoint.position);
        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;
        thirdPersonCam.ResetPosition();
    }
}
