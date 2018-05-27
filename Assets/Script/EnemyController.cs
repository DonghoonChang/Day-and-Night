using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class EnemyController : MonoBehaviour
{

    [SerializeField] int health = 150;
    [SerializeField] float headshotMultiplier = 3f;
    [SerializeField] int exp = 15;

    GameObject player;
    EnemyNavController navController;

    int healthPrev;
    int staggerPointH;
    int staggerPointL;

    void Awake()
    {
        player = GameObject.Find("Player");
        navController = GetComponent<EnemyNavController>();
        healthPrev = health;
        staggerPointH = health * 2 / 3;
        staggerPointL = health * 1 / 3;

    }

    public void TakeDamage(int baseDamage, int concussion, bool headshot, Vector3 hitpoint, Ray ray)
    {
        int damage = headshot ? Mathf.FloorToInt(baseDamage * headshotMultiplier) : baseDamage;
        health -= damage;

        if (health <= 0)
            navController.OnKilled(concussion, hitpoint, ray);

        if (health < staggerPointL && staggerPointL < healthPrev)
        {
            //navController.Stagger();
        }
        else if (health < staggerPointH && staggerPointH < healthPrev)
        {
            //navController.Stagger();
        }

        healthPrev = health;

        // Blood Splatter
    }
}
