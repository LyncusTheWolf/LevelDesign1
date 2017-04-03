using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(CharacterController))]
public class Player : Character {

    private static Player instance;

    private int coins;
    private int keyCount;
    private CharacterMotor motor;
    private CharacterController controller;
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

    public override Vector3 Center {
        get { return transform.position + controller.center; }
    }

    private void Awake() {
        if(instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        motor = GetComponent<CharacterMotor>();
        controller = GetComponent<CharacterController>();

        coins = 0;
        keyCount = 0;
    }

    // Update is called once per frame
    public override void Update() {
        base.Update();
        if (Input.GetKeyDown(KeyCode.N)) {
            Damage(1);
        }
	}

    public override void Kill() {
        //Do things...
        GameManager.Instance.PollLives();
    }

    protected override void Init() {
        currentHealth = GameManager.PLAYER_STARTING_HEALTH;
        maxHealth = GameManager.PLAYER_STARTING_HEALTH;
        ResetCharacter();
        ThirdPersonSmartCamera.Instance.player = this;
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
