using UnityEngine;

namespace MyGame.GameManagement
{
    public class SFXManager : MonoBehaviour
    {

        public static SFXManager Instance
        {
            get; private set;
        }

        public GameObject bulletImpactConcrete;
        public GameObject bulletImpactMetal;
        public GameObject bulletImpactWood;
        public GameObject bulletImpactSand;
        public GameObject bulletImpactWater;
        public GameObject bulletImpactFleshA;
        public GameObject bulletImpactFleshB;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}

