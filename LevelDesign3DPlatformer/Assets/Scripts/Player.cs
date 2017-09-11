using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(CharacterController))]
public class Player : Character {

    private static Player instance;

    private CharacterMotor motor;
    private CharacterController controller;
    //private Rigidbody rigidBody;

    public static Player Instance {
        get { return instance; }
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
    }   
}
