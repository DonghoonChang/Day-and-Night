using System.Collections;
using UnityEngine;
using TMPro;

namespace MyGame.UI
{
    public class UIHUDPrompt : MonoBehaviour
    {
        static float ClearTime = 3f;
        static int DefaultSpeed = 2;

        [SerializeField]
        TextMeshProUGUI promptText;

        string _message;
        IEnumerator promptEnumerator;

        void Awake()
        {
            promptText.text = "";
        }

        public void ShowPromptDefaultSpeed(string message)
        {
            _message = message;
            promptText.text = "";

            if (promptEnumerator != null)
                StopCoroutine(promptEnumerator);

            promptEnumerator = ShowPrompt(DefaultSpeed);
            StartCoroutine(promptEnumerator);
        }

        public void ShowPrompt(string message, int speed)
        {
            _message = message;
            promptText.text = "";

            if (promptEnumerator != null)
                StopCoroutine(promptEnumerator);

            promptEnumerator = ShowPrompt(speed);
            StartCoroutine(promptEnumerator);
        }

        private IEnumerator ShowPrompt(int speed)
        {
            int frameCount = 0;

            for (int i = 0; i < _message.Length;)
            {
                if (frameCount % speed == 0)
                {
                    promptText.text += _message[i];
                    i++;
                }

                frameCount++;
                yield return null;
            }


            Invoke("ClearText", ClearTime);
        }

        private void ClearText()
        {
            promptText.text = "";
        }
    }
}
