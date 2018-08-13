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

    private float _targetSpread = 0f;
    private float _visualSpread = 0f;

    #region Awake to Update

    private void Awake()
    {
        AdjustCrosshair(GetDislocation(0));
    }

    private void Update()
    {
        _visualSpread = Mathf.Lerp(_visualSpread, _targetSpread, Mathf.Abs(_visualSpread - _targetSpread) * distanceFollow * Time.deltaTime);
        AdjustCrosshair(GetDislocation(_visualSpread));
    }

    #endregion

    public void SetCurrentSpread(float spread)
    {
        _targetSpread = Mathf.Max(spread, 0);
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
