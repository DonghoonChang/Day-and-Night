using UnityEngine;

namespace MyGame.UI
{
    public class UIFloatingCanvas : MonoBehaviour
    {

        public void ToggleUI(bool on)
        {
            if (on)
                gameObject.SetActive(true);

            else
                gameObject.SetActive(false);
        }
    }
}

