using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour {

    EnemyNavController navController;

    private void Awake()
    {
        navController = transform.root.GetComponent<EnemyNavController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.name == "Player")
            navController.OnAttackTriggerEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.name == "Player")
            navController.OnAttackTriggerExit();
    }
}
