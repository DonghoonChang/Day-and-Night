using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavController : MonoBehaviour
{
    /* Numbers of different substate motions for random picking*/
    static int IDLEMOTIONS = 4;
    static int ATTACKMOTIONS = 6;

    enum Walking : int
    {
        STOP = 0, WALKING, CHASING
    }

    static string Idle1 = "Zombie Idle Eyes into Sky";
    static string Idle2 = "Zombie Idle Eyes on Floor";
    static string Idle3 = "Zombie Idle Leg Shuffle1";
    static string Idle4 = "Zombie Idle Leg Shuffle2";
    static string[] Idles = new string[] { Idle1, Idle2, Idle3, Idle4};


    /* Controlls Enemy Navigation and Animation */
    [SerializeField] [Range(0f, 20f)] float rotationSpeed; //The range the enemy actually knows where the player is
    [SerializeField] [Range(0f, 5f)] float stoppingDistance;
    [SerializeField] [Range(0f, 2f)] float walkingSpeed;
    [SerializeField] [Range(0f, 2f)] float chasingSpeed;

    /*
     * Components 
     */
    GameObject player;
    Rigidbody[] ragdoll;
    Animator animator;
    NavMeshAgent agent;

    int idleMotionNumber = 2;
    int IdleID = Animator.StringToHash("IdleMoNo");
    int AttackID = Animator.StringToHash("AttackMoNo");
    int WalkingID = Animator.StringToHash("WalkingMoNo");
    int deadID = Animator.StringToHash("Dead");

    bool isAttacking = false;
    float deathTime = 10f;


    void Awake()
    {
        player = GameObject.Find("Player");
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ragdoll = GetComponentsInChildren<Rigidbody>();
        
        agent.stoppingDistance = stoppingDistance;
        agent.speed = walkingSpeed;

        foreach(Rigidbody rb in ragdoll)
            rb.isKinematic = true;

        animator.SetInteger(IdleID, idleMotionNumber);
        animator.Play(Idles[idleMotionNumber - 1]);
    }
    void Update()
    {
        if (isAttacking)
            FacePlayer();
    }

    /* Movement Related */
    #region Movement
    void StartWalking()
    {
        agent.isStopped = false;
        animator.SetInteger(WalkingID, (int) Walking.WALKING);
        StartCoroutine("MoveToPlayer");
    }
    void StopWalking()
    {   
        agent.isStopped = true;
        animator.SetInteger(WalkingID, (int) Walking.STOP);
        StopCoroutine("MoveToPlayer");
    }
    void StartAttacking()
    {
        isAttacking = true;
        PickAttack();
    }
    void StopAttacking()
    {
        isAttacking = false;
        animator.SetInteger(AttackID, 0);
    }
    IEnumerator MoveToPlayer()
    {
        while (true)
        {
            agent.SetDestination(player.transform.position);
            yield return new WaitForSeconds(0.3f);
        }
    }
    void FacePlayer()
    {
        Vector3 N = (player.transform.position - transform.position).normalized;
        Quaternion lookrotation = Quaternion.LookRotation(new Vector3(N.x, 0, N.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookrotation, Time.deltaTime * rotationSpeed);
    }
#endregion

    /* Aggresiveness Related */
    #region Agreesiveness
    public void OnAlertTriggerEnter()
    {
        if (agent.enabled)
        {
            StartWalking();
        }
    }
    public void OnAlertTriggerExit()
    {
        if (agent.enabled)
            StopWalking();
    }
    public void OnAttackTriggerEnter()
    {
        if (agent.enabled)
        {
            StopWalking();
            StartAttacking();
            isAttacking = true;
        }
    }
    public void OnAttackTriggerExit()
    {   
        if (agent.enabled)
        {
            StartWalking();
            StopAttacking();
            isAttacking = false;
        }
    }
    public void Stagger()
    {

    }
#endregion

    /* Death Related */
    public void OnKilled(int concussion, Vector3 hitpoint, Ray ray)
    {
        animator.SetBool(deadID, true);
        agent.enabled = false;
        animator.enabled = false;
        StopCoroutine("MoveToPlayer");

        foreach (Rigidbody rb in ragdoll)
        {
            rb.isKinematic = false;
            Debug.DrawLine(ray.origin, hitpoint, Color.red, 3f);
            rb.AddForce(ray.direction.normalized * concussion, ForceMode.Impulse);
        }

        Destroy(gameObject, deathTime);
    }

    void PlayDeathAnimation()
    {

    }

    /* Animation Events */
    void StopStagger()
    {

    }

    /* Every attack animation triggers this event in the transition for the next consecutive attack*/
    void PickAttack()
    {
        int pick = Random.Range(1, ATTACKMOTIONS);

        if (1 > pick || pick > ATTACKMOTIONS)
        {
            pick = 1;
        }

        if (isAttacking)
            animator.SetInteger(AttackID, pick);
    }
}
