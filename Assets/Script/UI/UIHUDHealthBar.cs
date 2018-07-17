using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHUDHealthBar : MonoBehaviour {

    static float transitionDelta = 15f;
    static float stoppingDistance = 0.01f;

    public Image healthFill;
    public TextMeshProUGUI healthText;

    int maxHP = 100;
    int targetHP = 100;
    float targetFill = 1f;

    private void Awake()
    {
        healthText.text = 100.ToString();
    }
    private void Update()
    {
        float distance = Mathf.Abs(targetFill - healthFill.fillAmount);

        if (distance > stoppingDistance)
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetFill, distance * transitionDelta * GameTime.deltaTime);
    }

    public void SetTargetHP(int hp)
    {
        targetHP = hp;
        targetFill = ((float) hp) / maxHP ;
    }
}
