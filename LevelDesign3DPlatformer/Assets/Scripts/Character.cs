using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour {

    public delegate void OnDamage();
    public delegate void OnReset();

    public event OnDamage onDamage;
    public event OnReset onReset;

    protected int currentHealth;
    protected int maxHealth;

    public int CurrentHealth {
        get { return currentHealth; }
    }

    public int MaxHealth {
        get { return maxHealth; }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    protected void Init() {
        currentHealth = GameManager.PLAYER_STARTING_HEALTH;
        maxHealth = GameManager.PLAYER_STARTING_HEALTH;
    }

    public void ResetCharacter() {
        currentHealth = maxHealth;
        onReset();
    }

    public void Damage(int amt) {
        currentHealth = Mathf.Clamp(currentHealth - amt, 0, maxHealth);
        onDamage();
        if (currentHealth <= 0) {
            Kill();
        }
    }

    public abstract void Kill();
}
