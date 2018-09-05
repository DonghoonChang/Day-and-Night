using UnityEngine;
using TMPro;

namespace Game.UI
{
    public class UIHUDInteractableName : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI _objectNameText;

        public void SetInteractableName(string name)
        {
            _objectNameText.text = name;
        }

        public void ResetName()
        {
            _objectNameText.text = "";
        }
    }
}
