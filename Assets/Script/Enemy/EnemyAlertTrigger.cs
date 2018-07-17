using UnityEngine;

namespace MyGame.Enemy
{
    public class EnemyAlertTrigger : MonoBehaviour
    {
        EnemyCharacter thisEnemy;

        private void Awake()
        {
            thisEnemy = GetComponentInParent<EnemyCharacter>();
        }

        private void OnTriggerEnter(Collider other)
        {
        }

        private void OnTriggerStay(Collider other)
        {
        }

        private void OnTriggerExit(Collider other)
        {
        }
    }
}

