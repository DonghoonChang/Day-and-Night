using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIHUDSniperScope : MonoBehaviour
    {

        static float AlphaVisible = 1f;
        static float AlphaInvisible = 0f;

        [SerializeField]
        Image imageScope;

        [SerializeField]
        Image imageTop;

        [SerializeField]
        Image imageDown;

        [SerializeField]
        Image imageLeft;

        [SerializeField]
        Image imageRight;

        public void ShowScope()
        {
            SetAlpha(AlphaVisible);
        }

        public void HideScope()
        {
            SetAlpha(AlphaInvisible);
        }

        private void SetAlpha(float value)
        {
            Color color = new Color(0, 0, 0, value);

            imageScope.color = color;
            imageTop.color = color;
            imageDown.color = color;
            imageLeft.color = color;
            imageRight.color = color;
        }
    }
}
