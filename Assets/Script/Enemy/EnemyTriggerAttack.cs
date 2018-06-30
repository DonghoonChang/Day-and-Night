using UnityEngine;

namespace MyGame.Enemy
{
    public class EnemyTriggerAttack : MonoBehaviour
    {

        /*
         *  Triggers Attack
         */

        Enemy thisEnemy;

        private void Awake()
        {
            thisEnemy = transform.root.GetComponent<Enemy>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.OnAttackTriggerEnter();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.tag == "Player")
                thisEnemy.OnAttackTriggerExit();
        }
    }
}

