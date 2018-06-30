using UnityEngine;
using System.Collections;


namespace MyGame.GameManagement
{
    public class TimedSelfDestructCartridge : MonoBehaviour
    {

        GameManager gameManager;

        float destructTime = 5f;

        void Awake()
        {
            gameManager = GameManager.Instance;
            if (gameManager != null)
                destructTime = gameManager.GraphicsConfiguration.CartridgeLifeTime;

            else
                destructTime = 10f;
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(destructTime);
            Destroy(gameObject);
        }
    }
}

