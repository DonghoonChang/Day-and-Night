using UnityEngine;

public class UIHUDCrosshair : MonoBehaviour {

    #region Const Variables

    // Distance from the Center
    const float startDistance = 10f;
    const float minDistance = 7.5f;
    const float distanceMultiplier = 7f;
    const float distanceFollow = 5f;

    #endregion

    public RectTransform crosshairUp;
    public RectTransform crosshairDown;
    public RectTransform crosshairLeft;
    public RectTransform crosshairRight;

    [SerializeField]
    private float actualSpread = 0f;
    private float visualSpread = 0f;

    #region Awake to Update

    private void Awake()
    {
        AdjustCrosshair(GetDislocation(0));
    }

    private void Update()
    {
        visualSpread = Mathf.Lerp(visualSpread, actualSpread, Mathf.Abs(visualSpread - actualSpread) * distanceFollow * Time.deltaTime);
        AdjustCrosshair(GetDislocation(visualSpread));
    }

    #endregion

    public void SetCurrentSpread(float spread)
    {
        actualSpread = Mathf.Max(spread, 0);
    }

    private float GetDislocation(float spread)
    {
        return (startDistance + minDistance + spread * distanceMultiplier);
    }

    private void AdjustCrosshair(float dislocation)
    {
        crosshairUp.localPosition = new Vector3(0, dislocation, 0);
        crosshairDown.localPosition = new Vector3(0, -dislocation, 0);
        crosshairLeft.localPosition = new Vector3(dislocation, 0, 0);
        crosshairRight.localPosition = new Vector3(-dislocation, 0, 0);
    }
}
