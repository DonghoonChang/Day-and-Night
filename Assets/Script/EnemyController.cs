using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class EnemyController : MonoBehaviour
{

    [SerializeField] float health = 150f;
    [SerializeField] float headshotMultiplier = 3f;
    [SerializeField] int exp = 15;

    public Animator animator;
    public GameObject player;
    EnemyNavController navController;
    void Awake()
    {
        navController = GetComponent<EnemyNavController>();
    }


    void Update()
    {

    }
    
    public void TakeDamage(float baseDamge, bool headshot)
    {
        health = headshot ? health - baseDamge * headshotMultiplier : health - baseDamge;
        if (health == 0)
            navController.IsDead();
    }
}
