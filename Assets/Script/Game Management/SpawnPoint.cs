using UnityEngine;
using System.Collections.Generic;
using EnemyCharacter = MyGame.Enemy.EnemyCharacter;


namespace MyGame.GameManagement
{

    public class SpawnPoint : MonoBehaviour
    {

        GameManager gameManager;

        [SerializeField] List<EnemyCharacter> spawns = new List<Enemy.EnemyCharacter>();

        public GameObject spawnPrefab;
        public GameObject spawnMarker;


        #region Awake to Updates


        void Awake()
        {
            if (spawnMarker != null)
                spawnMarker.SetActive(false);

            Spawn();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            gameManager.AddSpawnPoint(this);
        }


#endregion

        #region Helpers


        public void SetNoiseTirggers(float multiplier)
        {
            foreach (EnemyCharacter navAnim in spawns)
                SetNoiseTirggers(multiplier);
        }

        public void Spawn()
        {
            GameObject spawn = Instantiate(spawnPrefab, transform.position, Quaternion.LookRotation(Vector3.up));
            EnemyCharacter enemy = spawn.GetComponent<EnemyCharacter>();
            spawn.SetActive(true);

            if (enemy != null)
                spawns.Add(enemy);

            else
            {
                Debug.LogError(transform.name + ": Spawn Point Spawed Obejct without EnemyNavAnim Component");
                Destroy(spawn);
            }
        }

        public void SpawnRepeatedly(int delay, int interval = 0)
        {
            if (interval == 0)
                Invoke("Spawn", delay);

            else
                InvokeRepeating("Spawn", delay, interval);
        }

        public void Stop()
        {
            CancelInvoke();
        }


#endregion

    }
}
