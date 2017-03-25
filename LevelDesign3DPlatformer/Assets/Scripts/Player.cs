using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(CharacterRigidbodyMotor))]
//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterMotor))]
public class Player : Character {

    private static Player instance;


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

        coins = 0;
        keyCount = 0;
    }

    // Use this for initialization
    void Start () {
        Init();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.N)) {
            Damage(1);
        }
	}

    public override void Kill() {
        //Do things...
        GameManager.Instance.PollLives();
    }

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
