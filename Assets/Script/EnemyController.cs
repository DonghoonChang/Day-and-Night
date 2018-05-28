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

    int staggerTriggerDamage;

    void Awake()
    {
        player = GameObject.Find("Player");
        navController = GetComponent<EnemyNavController>();
        staggerTriggerDamage = health / 3;
    }

    public void OnHit(int baseDamage, int concussion, string partName, Vector3 hitpoint, Ray ray)
    {
        int damage = partName.ToLower().Contains("head") ? Mathf.FloorToInt(baseDamage * headshotMultiplier) : baseDamage;

        health -= damage;

        /* Play Death Animation on Death */
        if (health <= 0)
            navController.OnKilled(partName, concussion, hitpoint, ray);

        else if (damage >= staggerTriggerDamage)
            navController.StaggerMajor();

        else
            navController.StaggerMinor();


        // Blood Splatter
    }
}
