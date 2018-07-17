using UnityEngine;
using ObjectInteractionResult = MyGame.Inventory.ObjectInteractionResult;

namespace MyGame.UI
{
    public class UIErrorPanel : MonoBehaviour
    {

        public static void ReportResult(ObjectInteractionResult result)
        {
            Debug.Log(result.message);
        }
    }
}

