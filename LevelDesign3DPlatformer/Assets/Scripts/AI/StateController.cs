using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Animator))]
public class StateController : MonoBehaviour {

    public State initialState;
    public Enemy enemyStats;
    public EnemyCharacteristics characteristics;
    public Animator anim;

    [Header("Sighting Properties")]
    [SerializeField]
    private Vector3 viewOffset;
    /*[SerializeField]
    private float fieldOfView;
    [SerializeField]
    private float viewDepth;
    [SerializeField]
    private float minTargetLockRange;
    [SerializeField]
    private float chaseThreshold;*/

    public List<Transform> wayPointList;
    public int currentWayPoint;
    public Character target;

    private NavMeshAgent navAgent;
    private State currentState;
    private float halfView;
    private float currentStateTimer;
    private AnimatorStateInfo animatorStateInfo;

    public Enemy EnemyStats {
        get { return enemyStats; }
    }

    public NavMeshAgent NavAgent{
        get { return navAgent; }
    }

    public float CurrentStateTimer {
        get { return currentStateTimer; }
    }

    public AnimatorStateInfo AnimStateInfo {
        get { return animatorStateInfo; }
    }

    private void OnValidate() {
        if (enemyStats != null) {
            characteristics = enemyStats.characteristics;
            halfView = characteristics.fieldOfView / 2.0f;
        }
    }

    private void Awake() {
        currentWayPoint = 0;
        navAgent = GetComponent<NavMeshAgent>();        
        currentStateTimer = 0.0f;
        currentState = initialState;
        navAgent.speed = characteristics.speed;
        halfView = characteristics.fieldOfView / 2.0f;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        animatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);

        //PollObjectsInSight();
        currentStateTimer += Time.deltaTime;
        currentState.UpdateState(this);
    }

    public bool PollObjectsInSight(ref Character targetFound) {
        //TODO Expand this functionality if necessary to locate objects other than the player

        if(Vector3.Angle(transform.forward, Player.Instance.transform.position - transform.position) < halfView) {
            RaycastHit hit;
            Vector3 viewPosition = transform.position + transform.rotation * viewOffset;
            Vector3 dir = (Player.Instance.Motor.Center - viewPosition).normalized;

            //Debug.DrawRay(viewPosition, dir);

            if (Physics.Raycast(viewPosition, dir, out hit, characteristics.viewDepth)) {
                //Debug.Log(hit.collider.name);
                if(hit.collider.gameObject == Player.Instance.gameObject) {
                    targetFound = hit.collider.GetComponent<Character>();
                    return true;
                }
            }
        }

        return false;
    }

    public bool ValidateTarget() {
        if(target == null) {
            return false;
        }

        if (Vector3.Distance(transform.position, target.transform.position) < characteristics.minTargetLockRange) {
            return true;
        }

        if (Vector3.Angle(transform.forward, target.transform.position - transform.position) < halfView) {
            RaycastHit hit;
            Vector3 viewPosition = transform.position + transform.rotation * viewOffset;
            Vector3 dir = (target.Center - viewPosition).normalized;

            Debug.DrawRay(viewPosition, dir * characteristics.viewDepth, Color.yellow, 0.25f);

            if (Physics.Raycast(viewPosition, dir, out hit, characteristics.chaseThreshold)) {
                if (hit.collider.GetComponent<Character>() == target) {
                    return true;
                }
            }
        }

        return false;
    }

    public void TransitionToState(State nextState) {
        currentState = nextState;
        currentStateTimer = 0.0f;
        //TODO: OnStateChanged
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
        halfView = characteristics.fieldOfView / 2.0f;

        Vector3 viewPosition = transform.position + transform.rotation * viewOffset;

        Debug.DrawRay(viewPosition, Quaternion.AngleAxis(halfView, Vector3.up) * transform.forward * characteristics.viewDepth);
        Debug.DrawRay(viewPosition, Quaternion.AngleAxis(-halfView, Vector3.up) * transform.forward * characteristics.viewDepth);
        Debug.DrawRay(viewPosition, Quaternion.AngleAxis(halfView, Vector3.right) * transform.forward * characteristics.viewDepth);
        Debug.DrawRay(viewPosition, Quaternion.AngleAxis(-halfView, Vector3.right) * transform.forward * characteristics.viewDepth);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, characteristics.minTargetLockRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, characteristics.chaseThreshold);
    }
    #endif
}
