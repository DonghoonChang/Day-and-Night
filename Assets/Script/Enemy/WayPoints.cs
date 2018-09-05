using UnityEngine;

namespace Game.Enemy
{
    public class WayPoints : MonoBehaviour
    {
        public Transform[] points;
        [Range(0f, 10f)]
        public float uponReachWaitTime;
        public bool patrol;
        public bool walk;
    }
}

