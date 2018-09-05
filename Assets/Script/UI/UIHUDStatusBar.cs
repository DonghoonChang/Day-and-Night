using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    public class UIHUDStatusBar : MonoBehaviour
    {
        const float transitionDelta = 15f;
        const float stoppingDistance = 0.01f;

        [SerializeField] Image _bar;
        [SerializeField] TextMeshProUGUI _valueText;

        int _targetValue = 100;
        float _targetFill = 1f;

        private void Awake()
        {
            _valueText.text = 100.ToString();
        }

        private void Update()
        {
            float distance = Mathf.Abs(_targetFill - _bar.fillAmount);

            if (distance > stoppingDistance)
                _bar.fillAmount = Mathf.Lerp(_bar.fillAmount, _targetFill, distance * transitionDelta * GameTime.deltaTime);
        }

        public void SetTargetFillandValue(float fill, int value)
        {
            _targetValue = value;
            _targetFill = fill;

            _valueText.text = _targetValue.ToString();
        }
    }
}
