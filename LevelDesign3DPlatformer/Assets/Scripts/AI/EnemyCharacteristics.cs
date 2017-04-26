using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Characteristics")]
public class EnemyCharacteristics : ScriptableObject {
    public int startingHealth;
    public GameObject dropItem;
    public float speed;
    public float fieldOfView;
    public float viewDepth;
    public float minTargetLockRange;
    public float chaseThreshold;
    public float meleeRange;
    public float minRangeAttack;
    public float maxRangeAttack;
    public float minAttackAngle;
}
