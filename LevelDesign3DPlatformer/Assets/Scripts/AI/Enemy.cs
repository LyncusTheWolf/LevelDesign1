using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Enemy : Character {

    public EnemyCharacteristics characteristics;
    //public int startingHealth;

    private CapsuleCollider col;

    //public List<DropPool> drops;

    public override Vector3 Center {
        get { return col.center; }
    }
	
	// Update is called once per frame
	public override void Update () {
        base.Update();
	}

    public override void Kill() {
        
    }

    protected override void Init() {
        col = GetComponent<CapsuleCollider>();
        currentHealth = characteristics.startingHealth;
        maxHealth = characteristics.startingHealth;
        ResetCharacter();
    }
}
