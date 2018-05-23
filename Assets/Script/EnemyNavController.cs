using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class EnemyNavController : MonoBehaviour
{

    /*
     * Controlls Enemy Navigation and Animation 
     */
    [SerializeField] [Range(0f, 5f)] float reachableRange; //The range the enemy is able to hit the player
    [SerializeField] [Range(0f, 10f)] float exposedRange; //The range the player presence was exposed to the enemy
    [SerializeField] [Range(0f, 20f)] float alertedRange; //The range the enemy actually knows where the player is
    [SerializeField] [Range(0f, 20f)] float rotationSpeed = 5f; //The range the enemy actually knows where the player is
    [SerializeField] [Range(0f, 2f)] float alertedSpeed = 0.8f;
    [SerializeField] [Range(0f, 2f)] float exposedSpeed = 1.5f;

    public GameObject player;
    public Animator animator;
    NavMeshAgent agent;

    int deadID = Animator.StringToHash("Dead");
    int walkingID = Animator.StringToHash("Walking");
    int inRangeID = Animator.StringToHash("InRange");

    bool isReachable = false;
    bool isExposed = false;
    bool isAlerted = false;
    bool isAttacking = false;
    bool isWalking = false;
    bool isDead = false;

    float currentSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = reachableRange / 1.5f;
        currentSpeed = alertedSpeed;
        agent.speed = currentSpeed;
    }


    void Update()
    {

        float distance = Vector3.Distance(player.transform.position, transform.position);
        isReachable = distance < reachableRange;
        isExposed = distance < exposedRange;
        isAlerted = distance < alertedRange;

        if (isAlerted)
        {
            if (!isWalking)
                StartWalking();

            if (isReachable)
            {
                if (!isAttacking)
                {
                    FacePlayer();
                    StopWalking();
                    StartAttacking();
                }
            }
            else
            {
                if (isAttacking)
                    StopAttacking();
            }

        }
        else
        {
            if (isWalking)
                StopWalking();

        }
    }

    void StartWalking()
    {
        isWalking = true;
        agent.isStopped = false;
        animator.SetBool(walkingID, true);
        StartCoroutine(MoveToPlayer());
    }

    void StopWalking()
    {
        isWalking = false;
        agent.isStopped = true;
        animator.SetBool(walkingID, false);
        StopCoroutine(MoveToPlayer());
    }


    void StartAttacking()
    {
        isAttacking = true;
        animator.SetBool(inRangeID, true);
    }

    void StopAttacking()
    {
        isAttacking = false;
        animator.SetBool(inRangeID, false);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, reachableRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, exposedRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, alertedRange);


    }

    public void IsDead()
    {
        isDead = true;
        animator.SetBool(deadID, true);
        StopWalking();
        StopAttacking();
    }

    void DisableMovement()
    {
        animator.enabled = false;
        agent.enabled = false;
    }
}
