using UnityEngine;

public class TriggerWatch : MonoBehaviour {

    /* This Trigger Responds Only to Noise Triggers */

    EnemyNavController navController;

    private void Awake()
    {
        navController = transform.root.GetComponent<EnemyNavController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player Noise Trigger") 
            navController.StartWatchingPlayer();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Player Noise Trigger")
            navController.KeepWatchingPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player Noise Trigger")
            navController.StopWatchingPlayer();
    }
}
