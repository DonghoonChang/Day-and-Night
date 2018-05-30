using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;


public class EnemyNavController : MonoBehaviour
{
    /* Takes Care of Animation and Navigation */


    /* Animations*/
    static int ATTACKMOTIONS = 6;
    static int idleID = Animator.StringToHash("Zombie Idle");
    static int RotationID = Animator.StringToHash("Rotation");
    static int AttackID = Animator.StringToHash("AttackMoNo");
    static int WalkingID = Animator.StringToHash("WalkingMoNo");
    static int staggerID = Animator.StringToHash("StaggerMoNo");

    /* Controlls Enemy Navigation and Animation */
    [SerializeField] [Range(0f, 2f)] float speedChase;
    [SerializeField] [Range(0f, 2f)] float speedFollow;
    [SerializeField] [Range(0f, 5f)] float rotSpeedAttack;
    [SerializeField] [Range(0f, 5f)] float rotSpeedChase;
    [SerializeField] [Range(0f, 5f)] float rotSpeedFollow;
    [SerializeField] [Range(0f, 2f)] float rotSpeedWatch;


    /* Components */
    public GameManagement gameManager;
    GameObject player;
    Rigidbody[] ragdoll;
    Animator animator;
    NavMeshAgent agent;

    /* Animation States */
    bool isWatching = false;
    bool isFollowing = false;
    bool isChasing = false;
    bool isAttacking = false;
    bool inMajorStagger = false;
    float deathTime = 10f;

    /* AI Triggers */
    public float attackTriggerRadious = 1f;
    public float followTriggerRadious = 2.5f;
    public float watchTriggerRadious = 6f;
    public float fovRadius = 6f;
    public float fovRadiusLosePlayer = 6f;

    SphereCollider attackTrigger;
    SphereCollider followTrigger;
    SphereCollider watchTrigger;

    /* Extra */
    public int thisIdleMoNo = 1;
    Quaternion targetLookDir = Quaternion.identity;
    LayerMask onlyPlayerLayer = (1 << 8);

    /* Coroutines */
    IEnumerator WatchPlayerCo;
    IEnumerator FollowPlayerCo;
    IEnumerator ChasePlayerCo;
    IEnumerator RotateToAttackPlayerCo;


    void Awake()
    {

        WatchPlayerCo = WatchPlayer();
        FollowPlayerCo = FollowPlayer();
        ChasePlayerCo = ChasePlayer();
        RotateToAttackPlayerCo = RotateToPlayer();

        player = gameManager.player;
        animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();
        agent.speed = speedFollow;

        ragdoll = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in ragdoll)
            rb.isKinematic = true;

        SphereCollider[] colliders = GetComponentsInChildren<SphereCollider>();
        foreach(SphereCollider col in colliders)
        {
            if (col.name == "Attack Trigger")
                attackTrigger = col;

            else if (col.name == "Follow Trigger")
                followTrigger = col;

            else if (col.name == "Watch Trigger")
                watchTrigger = col;
                
        }

        attackTrigger.radius = attackTriggerRadious / transform.localScale.x;
        followTrigger.radius = followTriggerRadious / transform.localScale.x;
        watchTrigger.radius = watchTriggerRadious / transform.localScale.x;

    }

    private void Update()
    {
        if (!isChasing)
        {
            if (CanSeePlayer(fovRadius))
                StartChasingPlayer();
        }

        else
        {
            if (!CanSeePlayer(fovRadiusLosePlayer))
                StopChasingPlayer();
        }
    }

    /* Stagger Animation */
    #region Stagger & Death Animation

    public void StaggerMinor()
    {
        /* Major Stagger Overrides Minor */
        if (!inMajorStagger)
        {
            animator.SetInteger(staggerID, 1);
        }
    }

    public void StaggerMajor()
    {
        inMajorStagger = true;
        animator.SetInteger(staggerID, 2);
    }

    public void ResetStaggerMinor()
    {
        animator.SetInteger(staggerID, 0);
    }

    public void ResetStaggerMajor()
    {
        inMajorStagger = false;
        animator.SetInteger(staggerID, 0);
    }

    /* Death Animation */
    public void OnKilled(string partName, int concussion, Vector3 hitpoint, Ray ray)
    {
        agent.enabled = false;
        animator.enabled = false;
        StopCoroutine("MoveToPlayer");

        foreach (Rigidbody rb in ragdoll)
        {
            rb.isKinematic = false;

            if (rb.name == partName)
                rb.AddForce(ray.direction.normalized * concussion, ForceMode.Impulse);
        }

        Destroy(gameObject, deathTime);
    }

    void PlayDeathAnimation()
    {

    }
    #endregion

    /* Enemy Alertness*/
    #region Enemy Alertness

    public void StartAttackingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        if (isFollowing)
            StopFollowingPlayer();

        if (isWatching)
            StopWatchingPlayer();

        if (isChasing)
            StopChasingPlayer();

        isAttacking = true;
        StartCoroutine(RotateToAttackPlayerCo);
        PickAttack();
    }

    void PickAttack()
    {
        int pick = Random.Range(1, ATTACKMOTIONS + 1);

        if (isAttacking)
            animator.SetInteger(AttackID, pick);
    }

    public void StopAttackingPlayer()
    {
        if (!agent.enabled)
            return;

        isAttacking = false;
        animator.SetInteger(AttackID, 0);
        StopCoroutine(RotateToAttackPlayerCo);

    }

    public void StartChasingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking Overrides Alert */
        if (isAttacking)
            return;

        if (isFollowing)
            StopFollowingPlayer();

        if (isWatching)
            StopWatchingPlayer();

        isChasing = true;
        agent.isStopped = false;
        agent.speed = speedChase;
        StartCoroutine(ChasePlayerCo);
        animator.SetInteger(WalkingID, 2);

        Debug.Log("Start Chasing Player");
    }

    public void KeepChasingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking Overrides Alert */
        if (isAttacking)
            return;

        if (isFollowing)
            StopFollowingPlayer();

        if (isWatching)
            StopWatchingPlayer();

        if (!isChasing)
            StartChasingPlayer();

        Debug.Log("Keep Chasing Player");
    }

    public void StopChasingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking Overrides Alert */
        if (isAttacking)
            return;

        if (isFollowing)
            StopFollowingPlayer();

        if (isWatching)
            StopWatchingPlayer();

        isChasing = false;
        agent.isStopped = true;
        StopCoroutine(ChasePlayerCo);
        animator.SetInteger(WalkingID, 0);

        Debug.Log("Stop Chasing Player");
    }

    /* In Alert Mode, the Enemy Walks Towards the Source of Noise, or Target */
    public void StartFollowingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking Overrides Alert */
        if (isAttacking || isChasing)
            return;

        if (isWatching)
            StopWatchingPlayer();

        isFollowing = true;
        agent.isStopped = false;
        agent.speed = speedFollow;
        StartCoroutine(FollowPlayerCo);
        animator.SetInteger(WalkingID, 1);

        Debug.Log("Start Following Player");
    }

    public void KeepFollowingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking Overrides Alert */
        if (isAttacking || isChasing)
            return;

        if (isWatching)
            StopWatchingPlayer();

        if (!isFollowing)
            StartFollowingPlayer();

        Debug.Log(isAttacking);
        Debug.Log(isChasing);
        Debug.Log(isFollowing);
        Debug.Log(isWatching);
        Debug.Log(agent.destination);
        Debug.Log(player.transform.position);
        Debug.Log("Keep Following Player");
    }

    public void StopFollowingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking Overrides Alert */
        if (isAttacking || isChasing || !isFollowing)
            return;

        if (isWatching)
            StopWatchingPlayer();

        isFollowing = false;
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        animator.SetInteger(WalkingID, 0);
        StopCoroutine(FollowPlayerCo);

        Debug.Log("Stop Following Player");
    }

    /* In Caution Mode, the Enemy Rotates to Face the Source of the Noise, or Target */
    public void StartWatchingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking & Following & Chasing Overrides Caution*/
        if (isFollowing ||isChasing || isAttacking)
            return;

        Vector3 playerDirection = player.transform.position - transform.position;
        Quaternion lookPlayerRotation = Quaternion.LookRotation(playerDirection);
        if (Quaternion.Angle(transform.rotation, lookPlayerRotation) >= 20f)
        {
            isWatching = true;
            bool right = Vector3.Dot(transform.right, (player.transform.position - transform.position)) > 0;
            animator.SetInteger(RotationID, right ? 1 : -1);
            StartCoroutine(WatchPlayerCo);
        }

        Debug.Log("Start Watching Player");
    }

    public void KeepWatchingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking & Alert Overrides Caution*/
        if (isAttacking || isChasing || isFollowing || isWatching)
            return;

        Vector3 playerDirection = player.transform.position - transform.position;
        Quaternion lookPlayerRotation = Quaternion.LookRotation(playerDirection);
        if (Quaternion.Angle(transform.rotation, lookPlayerRotation) >= 20f)
        {
            StartWatchingPlayer();
        }

        Debug.Log("Keep Watching Player");
    }

    public void StopWatchingPlayer()
    {
        /* Agent Disabled == Dead */
        if (!agent.enabled)
            return;

        /* Attacking & Alert Overrides Caution*/
        if (isFollowing || isChasing || isAttacking)
            return;

        isWatching = false;
        animator.SetInteger(RotationID, 0);
        StopCoroutine(WatchPlayerCo);

        Debug.Log("Stop Watching Player");
    }

    #endregion

    #region Coroutines

    IEnumerator WatchPlayer()
    {
        while (isWatching && !isFollowing && !isChasing && !isAttacking)
        {
            Vector3 playerDirection = player.transform.position - transform.position;
            Quaternion lookPlayerRotation = Quaternion.LookRotation(playerDirection);

            if (Quaternion.Angle(transform.rotation, lookPlayerRotation) < 10f)
                StopWatchingPlayer();

            transform.rotation = Quaternion.Lerp(transform.rotation, lookPlayerRotation, Time.deltaTime * rotSpeedWatch);

            yield return 0;
        }
    }

    IEnumerator FollowPlayer()
    {
        while (!isWatching && isFollowing && !isChasing && !isAttacking)
        {
            agent.SetDestination(player.transform.position);
            yield return 0;
        }
    }

    IEnumerator ChasePlayer()
    {
        while (!isWatching && !isFollowing && isChasing && !isAttacking)
        {
            agent.SetDestination(player.transform.position);
            yield return 0;
        }
    }

    IEnumerator RotateToPlayer()
    {
        while (!isWatching && !isFollowing && !isChasing && isAttacking)
        {

            Vector3 playerDirection = player.transform.position - transform.position;
            Quaternion lookPlayerRotation = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookPlayerRotation, Time.deltaTime * rotSpeedAttack);

            yield return 0;
        }
    }
    #endregion

    bool CanSeePlayer(float fovDistance)
    {
        /* Approximate FOV -> Left/Right 45 degrees */
        bool front = Vector3.Dot(transform.forward, player.transform.position - transform.position) > 0.5; // 0.5 ~~ COS(45)

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool close = distance < fovDistance;

        Vector3 relDir = (player.transform.position + player.transform.up) - (transform.position + transform.up);
        Ray ray = new Ray(transform.position + transform.up, relDir);
        bool lineOfSight = Physics.Raycast(ray, distance * 1.5f, onlyPlayerLayer);

        return front && close && lineOfSight;
    }

}
