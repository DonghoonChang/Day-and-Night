using UnityEngine;

public class TriggerAttack : MonoBehaviour {

    /* This Trigger Responds Only to Player Collider (Noise X) */

    EnemyNavController navController;

    private void Awake()
    {
        navController = transform.root.GetComponent<EnemyNavController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            navController.StartAttackingPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            navController.StopAttackingPlayer();
    }
}
