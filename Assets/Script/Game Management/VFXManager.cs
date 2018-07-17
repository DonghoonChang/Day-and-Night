using UnityEngine;

public class VFXManager : MonoBehaviour {

    const float VFXTimeReset = 10000f;
    const float PingPongLength = 90f;
    const float PingPongPeriod = 45f; // from 0 to pingpongLength

    float _VFXTime = 0f;
    bool _isTimerStarted = false;
    float _degree = 0f;

    #region Properties

    public static VFXManager Instance
    {
        get; private set;

    }

    #endregion

    #region Awake to Update

    void Awake () {

        if (Instance == null)
            Instance = this;
	}

    void Update()
    {
        if (_isTimerStarted)
        {
            _VFXTime += Time.deltaTime;
            _VFXTime %= VFXTimeReset;

            _degree = Mathf.PingPong(_VFXTime * PingPongLength / PingPongPeriod, PingPongLength);
        }
    }
    #endregion

    public void StartVFXTime()
    {
        _VFXTime = 0f;
        _isTimerStarted = true;
    }

    public void StopVFXTime()
    {
        _VFXTime = 0f;
        _isTimerStarted = false;
    }

    public float GetPingPongDegree()
    {
        return _degree;
    }

    public float GetPingPongSin()
    {
        return Mathf.Sin(_degree);
    }

    public float GetPingPongCos()
    {
        return Mathf.Cos(_degree);
    }

    public float GetPingPongTan()
    {
        return Mathf.Tan(_degree);
    }
}
