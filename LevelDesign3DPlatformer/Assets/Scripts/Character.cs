using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour {

    public delegate void OnDamage();
    public delegate void OnReset();

    public event OnDamage onDamage;
    public event OnReset onReset;

    public float protectionTimer;

    protected int currentHealth;
    protected int maxHealth;

    protected bool isAlive;

    private float currentProtectionTimer;

    public int CurrentHealth {
        get { return currentHealth; }
    }

    public int MaxHealth {
        get { return maxHealth; }
    }

    public bool IsAlive {
        get { return isAlive; }
    }

    public abstract Vector3 Center { get; }

    // Use this for initialization
    void Start () {
        Init();
	}
	
	// Update is called once per frame
	public virtual void Update () {
        currentProtectionTimer -= Time.deltaTime;
    }

    protected abstract void Init();

    public void ResetCharacter() {
        currentHealth = maxHealth;
        isAlive = true;
        if (onReset != null) {
            onReset();
        }
    }

    public void Damage(int amt) {
        if(currentProtectionTimer > 0.0f) {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amt, 0, maxHealth);
        onDamage();
        if (currentHealth <= 0) {
            Kill();
        } else {
            currentProtectionTimer = protectionTimer;
        }
    }

    public abstract void Kill();
}
