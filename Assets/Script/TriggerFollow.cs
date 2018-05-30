using UnityEngine;

public class TriggerFollow : MonoBehaviour {

    /* This Trigger Responds Only to Noise Triggers */

    EnemyNavController navController;

    private void Awake()
    {
        navController = transform.root.GetComponent<EnemyNavController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player Noise Trigger")
            navController.StartFollowingPlayer();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Player Noise Trigger")
            navController.KeepFollowingPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player Noise Trigger")
            navController.StopFollowingPlayer();
    }

}
