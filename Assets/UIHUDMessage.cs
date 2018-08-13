using UnityEngine;

namespace MyGame.UI
{
    public class UIHUDMessage : MonoBehaviour
    {
        [SerializeField] UIHUDInteractableName _interactable;
        [SerializeField] UIHUDPrompt _prompt;

        public void ShowInteractableName(string name)
        {
            _interactable.SetInteractableName(name);
        }

        public void ShowResult(bool success, string msg)
        {
            _prompt.ShowPromptDefaultSpeed(msg);
        }

        public void ShowPromptDefaultSpeed(string msg)
        {
            _prompt.ShowPromptDefaultSpeed(msg);
        }

        public void ShowPrompt(string msg, int speed)
        {
            _prompt.ShowPrompt(msg, speed);
        }
    }
}
