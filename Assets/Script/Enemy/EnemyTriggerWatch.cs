using UnityEngine;

namespace MyGame.Enemy
{
    public class EnemyTriggerWatch : MonoBehaviour
    {

        /*
        *  Triggers Watch
        */

        Enemy thisEnemy;

        private void Awake()
        {
            thisEnemy = transform.root.GetComponent<Enemy>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.OnWatchTriggerEnter();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.OnWatchTriggerEnter();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.OnWatchTriggerExit();
        }
    }

}
