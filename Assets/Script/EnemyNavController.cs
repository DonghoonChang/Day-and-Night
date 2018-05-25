using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavController : MonoBehaviour
{

    /*
     * Controlls Enemy Navigation and Animation 
     */
    [SerializeField] [Range(0f, 20f)] float rotationSpeed; //The range the enemy actually knows where the player is
    [SerializeField] [Range(0f, 5f)] float stoppingDistance;
    [SerializeField] [Range(0f, 2f)] float walkingSpeed;

    /*
     * Components 
     */
    public GameObject player;
    Rigidbody[] ragdoll;
    Animator animator;
    NavMeshAgent agent;

    int walkingID = Animator.StringToHash("Walking");
    int inRangeID = Animator.StringToHash("InRange");
    int staggerID = Animator.StringToHash("Staggered");

    bool isAttacking = false;
    float deathTime = 10f;


    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ragdoll = GetComponentsInChildren<Rigidbody>();
        
        agent.stoppingDistance = stoppingDistance;
        agent.speed = walkingSpeed;

        foreach(Rigidbody rb in ragdoll)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (isAttacking)
            FacePlayer();
    }

    void StartWalking()
    {
        agent.isStopped = false;
        animator.SetBool(walkingID, true);
        StartCoroutine("MoveToPlayer");
    }
    void StopWalking()
    {   
        agent.isStopped = true;
        animator.SetBool(walkingID, false);
        StopCoroutine("MoveToPlayer");
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

    void DisableMovement()
    {
        StopWalking();
    }

    public void OnKilled(Weapon weapon, Vector3 hitpoint, Ray ray)
    {
        agent.enabled = false;
        animator.enabled = false;
        StopCoroutine("MoveToPlayer");

        foreach(Rigidbody rb in ragdoll)
        {
            rb.isKinematic = false;
            Debug.DrawLine(ray.origin, hitpoint, Color.red, 3f);
            rb.AddForce(ray.direction.normalized * weapon.Force, ForceMode.Impulse);
        }

        Destroy(gameObject, deathTime);
    }

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
        agent.speed = 0;
        animator.SetBool(staggerID, true);
        Invoke("StopStagger", 0.3f);

    }

    void StopStagger()
    {
        agent.speed = walkingSpeed;
        animator.SetBool(staggerID, false);
    }
}
