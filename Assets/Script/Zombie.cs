using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Zombie : MonoBehaviour, IKillable {

    [SerializeField] [Range(0, 150)] private int m_Health = 150;
    [SerializeField] [Range(5f, 10f)] private float m_Speed_Walk = 5f;
    [SerializeField] [Range(0f, 5f)] private float m_Attack_Reach = 5f;

    private Animator anime;
    private Player player;
    private bool m_inRange = false;
    private int HealthID = Animator.StringToHash("Health");
    private int SpeedID = Animator.StringToHash("Speed");
    private int inRangeID = Animator.StringToHash("inRange");

    void Awake()
    {
        anime = GetComponent<Animator>();
        player = GameObject.Find("Player").GetComponent<Player>();
        anime.SetInteger(HealthID, m_Health);

    }

    void Update()
    {
        anime.SetBool(inRangeID, (player.transform.position - transform.position).magnitude < m_Attack_Reach);
    }

    public void TakeDamage (int basedamage, bool headshot)
    {
        int damage = headshot ? basedamage * 3 : basedamage;
        m_Health = Mathf.Max(m_Health - damage, 0);
        anime.SetInteger("Health", m_Health);
        Debug.Log(transform.name + "'s Health: " + m_Health);

    }
}
