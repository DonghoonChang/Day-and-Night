using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game.UI
{
    public class UIHUDPrompt : MonoBehaviour
    {

        #region Setting

        static float DefaultClearTime = 3f;
        static int DefaultSpeed = 2;

        #endregion

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
            {
                StopCoroutine(promptEnumerator);
                CancelInvoke();
            }

            promptEnumerator = ShowPrompt(DefaultSpeed);
            StartCoroutine(promptEnumerator);
        }

        public void ShowPrompt(string message = "", int speed = 2, float ClearTime = 3f)
        {
            _message = message;
            promptText.text = "";

            if (promptEnumerator != null)
            {
                StopCoroutine(promptEnumerator);
                CancelInvoke();
            }

            promptEnumerator = ShowPrompt(speed, ClearTime);
            StartCoroutine(promptEnumerator);
        }

        private IEnumerator ShowPrompt(int speed = 2, float ClearTime = 3f)
        {
            int frameCount = 0;

            Dictionary<int, string> tags = new Dictionary<int, string>();
            int mark = 0;

            for (int i = 0; i < _message.Length;)
            {
                if (frameCount % speed == 0)
                {
                    if (_message[i] == '<')
                    {
                        mark = i;
                        string tag = "";

                        while (_message[i] != '>')
                        {
                            tag += _message[i++];
                        }

                        tag += _message[i++];
                        tags[mark] = tag;
                    }


                    promptText.text += _message[i];
                    i++;
                }

                frameCount++;
                yield return null;
            }

            foreach (int key in tags.Keys)
            {
                promptText.text = promptText.text.Insert(key, tags[key]);
            }

            Invoke("ClearText", ClearTime);
        }

        private void ClearText()
        {
            promptText.text = "";
        }
    }
}
