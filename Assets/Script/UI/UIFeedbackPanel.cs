using UnityEngine;
using UIManager = Game.GameManagement.UIManager;
using ObjectInteractionResult = Game.Object.ObjectInteractionResult;

namespace Game.UI
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

