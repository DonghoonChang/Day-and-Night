using UnityEngine;

namespace MyGame.Enemy
{
    public class EnemyTriggerFollow : MonoBehaviour
    {

        /*
         *  Triggers Follow
         */

        Enemy thisEnemy;

        private void Awake()
        {
            thisEnemy = transform.root.GetComponent<Enemy>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.OnFollowTriggerEnter();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.OnFollowTriggerEnter();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.ResetAlertStatus();
        }

    }
}
