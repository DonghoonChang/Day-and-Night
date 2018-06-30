using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using GameManager = MyGame.GameManagement.GameManager;
using SFXManager = MyGame.GameManagement.SFXManager;
using WeaponStats = MyGame.Inventory.Weapon.WeaponStats;
using HitInfo = MyGame.Inventory.Weapon.HitInfo;

namespace MyGame.Enemy {

    public class EnemyAnimator: MonoBehaviour
    {
        /*
         * Takes Care of Animation and Navigation
         */

        #region Static Variables

        /* Animations*/
        static int ATTACKMOTIONS = 4;
        static int attackID = Animator.StringToHash("AttackMoNo");
        static int WalkingID = Animator.StringToHash("WalkingMoNo");
        static int staggerID = Animator.StringToHash("StaggerMoNo");
        static int idleID = Animator.StringToHash("IdleMoNo");
        static int idleoffsetID = Animator.StringToHash("IdleOffset");
        static float setAgentToPlayerFrequency = 0.1f;

        #endregion

        SFXManager sfxManager;

        /* Controlls Enemy Navigation and Animation */
        [Range(0f, 2f)] public float walkSpeedSlow;
        [Range(0f, 2f)] public float walkSpeedFast;
        [Range(0f, 2f)] public float rotateSpeedSlow;
        [Range(0f, 2f)] public float rotateSpeedFast;
        [Range(0f, 20f)] public float watchStopAngle;


        /* Components */
        Animator animator;
        NavMeshAgent navAgent;
        EnemyRagdollMapperRoot mappingController;


        /* Game Objects */
        Player.Player player;
        Rigidbody[] ragdollRigidBodies;
        Dictionary<string, Rigidbody> ragdollRBDic = new Dictionary<string, Rigidbody>();


        /* Coroutines */
        IEnumerator FacePlayerCo;
        IEnumerator SetAgentToPlayerCo;


        void Awake()
        {
            animator = GetComponent<Animator>();
            navAgent = GetComponent<NavMeshAgent>();
            mappingController = GetComponent<EnemyRagdollMapperRoot>();

            animator.SetFloat(idleoffsetID, Random.Range(0f, 0.35f));

            ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in ragdollRigidBodies)
            {
                rb.useGravity = false;
                ragdollRBDic.Add(rb.name, rb);
            }

            FacePlayerCo = FacePlayerCoroutine(rotateSpeedFast);
            SetAgentToPlayerCo = SetAgentToPlayerCoroutine(setAgentToPlayerFrequency);
        }

        private void Start()
        {
            player = GameManager.Instance.Player;
            sfxManager = SFXManager.Instance;
        }

        /* Stagger Animation */
        #region Hit, Stagger & Death Animation

        public void StartStagger()
        {
            navAgent.isStopped = true;
            animator.SetInteger(staggerID, 1);
        }

        public void StopStagger()
        {
            animator.SetInteger(staggerID, 0);
        }

        public void StopStaggerAgent()
        {
            navAgent.isStopped = false;
        }

        void DisplayBloodSFX(int index, Transform parent, Vector3 normal)
        {
            GameObject bloodsfx = Instantiate(sfxManager.bulletImpactFleshA, parent, true);
            bloodsfx.transform.SetParent(parent, false);
            bloodsfx.transform.localPosition = Vector3.zero;
            bloodsfx.transform.rotation = Quaternion.LookRotation(normal);
            Destroy(bloodsfx, 0.5f);
        }

        public void OnHit(HitInfo[] hitInfos, WeaponStats weaponStats, bool staggered)
        {
            foreach(HitInfo hitInfo in hitInfos)
            {
                Rigidbody hitRB = ragdollRBDic[hitInfo.hit.transform.name];
                hitRB.AddForceAtPosition(hitInfo.ray.direction.normalized * weaponStats.concussion, hitInfo.hit.point, ForceMode.VelocityChange);
                hitRB.AddTorque(hitInfo.ray.direction.normalized * weaponStats.concussion * 2, ForceMode.VelocityChange);
                DisplayBloodSFX(1, hitRB.transform, hitInfo.hit.normal);
            }

            if (staggered)
                StartStagger();
        }

        public void OnKilled(HitInfo[] hitInfos, WeaponStats stats)
        {
            StopAllCoroutines();
            animator.enabled = false;
            navAgent.enabled = false;

            foreach (Rigidbody rb in ragdollRigidBodies)
                rb.useGravity = true;


            foreach (HitInfo hitInfo in hitInfos)
            {
                Rigidbody hitRB = ragdollRBDic[hitInfo.hit.transform.name];
                hitRB.AddForceAtPosition(hitInfo.ray.direction.normalized * stats.concussion, hitInfo.hit.point, ForceMode.VelocityChange);
                hitRB.AddTorque(hitInfo.ray.direction.normalized * stats.concussion * 2, ForceMode.VelocityChange);
                DisplayBloodSFX(1, hitRB.transform, hitInfo.hit.normal);
            }

            mappingController.OnKilled();
        }


        void PlayDeathAnimation()
        {

        }
        #endregion

        #region Behaivour

        public void StopAllBehaviour()
        {
            StopAllCoroutines();

            navAgent.isStopped = true;
            navAgent.speed = 0f;

            animator.SetInteger(idleID, 0);
            animator.SetInteger(WalkingID, 0);
            animator.SetInteger(staggerID, 0);
            animator.SetInteger(attackID, 0);
        }

        public void StartAttackBehaviour()
        {
            StopAllBehaviour();
            StartCoroutine(FacePlayerCo);
            PickAttack();
        }

        void PickAttack()
        {
            int pick = Random.Range(1, ATTACKMOTIONS + 1);

            animator.SetInteger(attackID, pick);
        }

        public void StopAttackBehaviour()
        {
            StopAllBehaviour();
        }

        public void StartChaseRoutine()
        {
            StopAllBehaviour();

            navAgent.isStopped = false;
            navAgent.speed = walkSpeedFast;

            animator.SetInteger(WalkingID, 2);
            StartCoroutine(SetAgentToPlayerCo);
        }

        public void StopChaseBehaviour()
        {
            StopAllBehaviour();
        }

        public void StartFollowBehaviour()
        {
            StopAllBehaviour();

            navAgent.isStopped = false;
            navAgent.speed = walkSpeedSlow;

            animator.SetInteger(WalkingID, 1);
            StartCoroutine(SetAgentToPlayerCo);
        }

        public void StopFollowBehaviour()
        {
            StopAllBehaviour();
        }

        public void StartWatchBehaviour(bool right, Quaternion lookRotation)
        {
            StopAllBehaviour();

            animator.SetInteger(idleID, right ? 2 : 1);
            IEnumerator facePlayerCo = FaceNoiseCoroutine(lookRotation, rotateSpeedSlow, watchStopAngle);
            StartCoroutine(facePlayerCo);
        }

        public void StopWatchBehaviour()
        {
            StopAllBehaviour();
        }


        #endregion

        #region Coroutines

        IEnumerator FaceNoiseCoroutine(Quaternion lookRotation, float angularSpeed, float angularThreshhold)
        {
            while (Quaternion.Angle(transform.rotation, lookRotation) > angularThreshhold)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed);
                yield return 0;
            }

            animator.SetInteger(idleID, 0);
        }

        IEnumerator FacePlayerCoroutine(float angularSpeed)
        {
            while (true)
            {
                Vector3 relVec = player.transform.position - transform.position;
                Quaternion lookRoation = Quaternion.LookRotation(relVec);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRoation, angularSpeed * Time.deltaTime);
                yield return null;
            }
        }

        IEnumerator SetAgentToPlayerCoroutine(float frequency)
        {
            while (true)
            {   
                navAgent.SetDestination(player.transform.position);
                yield return new WaitForSeconds(frequency);
            }
        }

        #endregion

    }

}
