using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(CharacterRigidbodyMotor))]
//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterMotor))]
public class Player : MonoBehaviour {

    public const int MAX_PLAYER_HEALTH = 16;

    private static Player instance;

    [SerializeField]
    private int startingHealth;

    private int currentHealth;
    private int maxHealth;
    private int coins;
    private int keyCount;
    private CharacterMotor motor;
    //private Rigidbody rigidBody;

    public static Player Instance {
        get { return instance; }
    }

    public int KeyCount {
        get { return keyCount; }
    }

    public int Coins {
        get { return coins; }
    }

    public CharacterMotor Motor {
        get { return motor; }
    }

    private void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        motor = GetComponent<CharacterMotor>();
        //rigidBody = GetComponent<Rigidbody>();

        currentHealth = startingHealth;
        maxHealth = startingHealth;
        coins = 0;
        keyCount = 0;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Kill() {
        //Do things...
        GameManager.Instance.PollLives();
    }

    /*private void Respawn() {
        motor.AnimRespawn();
        //rigidBody.velocity = Vector3.zero;
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        ThirdPersonCamera.Instance.ResetPosition();  
    }*/

    public void AddCoin(int amt) {
        coins += amt;
        //TODO: Add in life increase functionality
    }

    public void AddKey(int amt) {
        keyCount += amt;
    }

    public bool UseKey() {
        if(keyCount > 0) {
            keyCount -= 1;
            return true;
        }

        return false;
    }
}
