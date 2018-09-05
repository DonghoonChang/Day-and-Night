using UnityEngine;

namespace Game.GameManagement
{
    public class VFXManager : MonoBehaviour
    {
        static float VFXTimeReset = 10000f;
        static float PingPongLength = 90f;
        static float PingPongPeriod = 45f;

        public GameObject bulletImpactConcrete;
        public GameObject bulletImpactMetal;
        public GameObject bulletImpactWood;
        public GameObject bulletImpactSand;
        public GameObject bulletImpactWater;
        public GameObject bulletImpactFleshA;
        public GameObject bulletImpactFleshB;

        float _degree = 0f;
        float _VFXTime = 0f;
        bool _isTimerStarted = false;

        public static VFXManager Instance
        {
            get; private set;
        }

        #region Awake to Update

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        void Update()
        {
            if (_isTimerStarted)
            {
                _VFXTime += Time.deltaTime;
                _VFXTime %= VFXTimeReset;

                _degree = Mathf.PingPong(_VFXTime * PingPongLength / PingPongPeriod, PingPongLength);
            }
        }

        #endregion

        public void StartVFXTime()
        {
            _VFXTime = 0f;
            _isTimerStarted = true;
        }

        public void StopVFXTime()
        {
            _VFXTime = 0f;
            _isTimerStarted = false;
        }

        public float GetPingPongDegree()
        {
            return _degree;
        }

        public float GetPingPongSin()
        {
            return Mathf.Sin(_degree);
        }

        public float GetPingPongCos()
        {
            return Mathf.Cos(_degree);
        }

        public float GetPingPongTan()
        {
            return Mathf.Tan(_degree);
        }
    }
}

