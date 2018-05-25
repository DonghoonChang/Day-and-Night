using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class EnemyController : MonoBehaviour
{

    [SerializeField] int health = 150;
    [SerializeField] float headshotMultiplier = 3f;
    [SerializeField] int exp = 15;

    public GameObject player;
    EnemyNavController navController;

    int healthPrev;
    int staggerPointH;
    int staggerPointL;

    void Awake()
    {
        navController = GetComponent<EnemyNavController>();
        healthPrev = health;
        staggerPointH = health * 2 / 3;
        staggerPointL = health * 1 / 3;

    }

    public void TakeDamage(Weapon weapon, bool headshot, Vector3 hitpoint, Ray ray)
    {
        int damage = headshot ? Mathf.FloorToInt(weapon.BaseDamage * headshotMultiplier) : weapon.BaseDamage;
        health -= damage;

        if (health <= 0)
            navController.OnKilled(weapon, hitpoint, ray);

        if (health < staggerPointL && staggerPointL < healthPrev)
        {
            navController.Stagger();
        }
        else if (health < staggerPointH && staggerPointH < healthPrev)
        {
            navController.Stagger();
        }

        healthPrev = health;


    }
}
