using UnityEngine;
using System.Collections;


namespace MyGame.GameManagement
{
    public class TimedSelfDestruct : MonoBehaviour
    {
        [SerializeField]
        float destructTime;


        IEnumerator Start()
        {
            yield return new WaitForSeconds(destructTime);

            Destroy(gameObject);
        }
    }
}

