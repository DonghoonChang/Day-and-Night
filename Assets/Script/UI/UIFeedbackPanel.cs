using UnityEngine;
using UIManager = MyGame.GameManagement.UIManager;
using ObjectInteractionResult = MyGame.Object.ObjectInteractionResult;

namespace MyGame.UI
{
    public class UIFeedbackPanel : MonoBehaviour
    {
        UIManager _uiManager;

        private void Start()
        {
            _uiManager = UIManager.Instance;
        }

        public void ReportResult(ObjectInteractionResult result)
        {
            if (_uiManager != null)
               
            Debug.Log(result.message);
        }
    }
}

