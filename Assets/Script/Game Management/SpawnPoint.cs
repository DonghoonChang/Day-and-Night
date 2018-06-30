using UnityEngine;
using System.Collections.Generic;

namespace MyGame.GameManagement
{

    public class SpawnPoint : MonoBehaviour
    {

        GameManager gameManager;

        [SerializeField] List<Enemy.Enemy> spawns = new List<Enemy.Enemy>();

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
            foreach (Enemy.Enemy navAnim in spawns)
                SetNoiseTirggers(multiplier);
        }

        public void Spawn()
        {
            GameObject spawn = Instantiate(spawnPrefab, transform.position, Quaternion.LookRotation(Vector3.up));
            Enemy.Enemy enemy = spawn.GetComponent<Enemy.Enemy>();
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
