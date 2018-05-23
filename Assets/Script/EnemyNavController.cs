using UnityEngine;
using UnityEngine.AI;


public class EnemyNavController : MonoBehaviour {

    /*
     * Controlls Enemy Navigation and Animation 
     */
    [SerializeField] [Range(0f, 5f)] float m_ReachableRange; //The range the enemy is able to hit the player
    [SerializeField] [Range(0f, 10f)] float m_ExposedRange; //The range the player presence was exposed to the enemy
    [SerializeField] [Range(0f, 20f)] float m_AlertedRange; //The range the enemy actually knows where the player is
    [SerializeField] [Range(0f, 20f)] float m_IdleSpeed = 5f; //The range the enemy actually knows where the player is
    [SerializeField] [Range(0f, 25f)] float m_WalkingSpeed = 15f; //The range the enemy actually knows where the player is

    public GameObject player;
    public Animator animator;
    NavMeshAgent agent;

    int deadID = Animator.StringToHash("Dead");
    int walkingID = Animator.StringToHash("Walking");
    int inRangeID = Animator.StringToHash("InRange");


    bool m_Reachable = false;
    bool m_Exposed = false;
    bool m_Alerted = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.Warp(transform.position);

    }


    void Update()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        m_Reachable = distance < m_ReachableRange;
        m_Exposed = distance < m_ExposedRange;
        m_Alerted = distance < m_AlertedRange;

        if (m_Alerted)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
            animator.SetBool(walkingID, true);

            if (m_Reachable)
            {
                // change angle
                Debug.DrawLine(transform.position, player.transform.position, Color.magenta);
                agent.isStopped = true;
                animator.SetBool(inRangeID, true);
                facePlayer();
            } else
            {
                agent.isStopped = false;
                animator.SetBool(inRangeID, false);
            }

        } else
        {
            agent.isStopped = true;
            animator.SetBool(walkingID, false);
        }
    }

    void facePlayer()
    {
        Vector3 N = (player.transform.position - transform.position).normalized;
        Quaternion lookrotation = Quaternion.LookRotation(new Vector3(N.x, 0, N.z));
        transform.rotation = lookrotation;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_ReachableRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_ExposedRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_AlertedRange);


    }

    public void isDead()
    {
        animator.SetBool(deadID, true);
    }
}
