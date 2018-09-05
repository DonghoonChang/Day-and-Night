using UnityEngine;

namespace Game.Enemy
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/EnemyBehaviorCard")]
    public class EnemyBehaviorCard : ScriptableObject
    {
        public EnemyAnimations animations;
    }
}

