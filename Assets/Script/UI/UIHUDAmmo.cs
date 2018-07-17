using UnityEngine;
using TMPro;

public class UIHUDAmmo : MonoBehaviour {

    [SerializeField]
    TextMeshProUGUI _currentAmmoText;

    [SerializeField]
    TextMeshProUGUI _totalAmmoText;

    int _currentAmmo = 0;
    int _totalAmmo = 0;

    private void Awake()
    {
        if (_currentAmmoText != null)
            _currentAmmoText.text = _currentAmmo.ToString();

        if (_totalAmmoText != null)
            _totalAmmoText.text = _totalAmmo.ToString();
    }

    public void SetCurrentAmmo(int count)
    {
        _currentAmmo = count;
        _currentAmmoText.text = _currentAmmo.ToString();
    }

    public void SetTotalAmmo(int count)
    {
        _totalAmmo = count;
        _totalAmmoText.text = _totalAmmo.ToString();
    }

}
