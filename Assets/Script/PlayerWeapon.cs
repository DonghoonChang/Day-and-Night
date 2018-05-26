using UnityEngine;

public class PlayerWeapon : MonoBehaviour {

    [SerializeField] [Range(1, 150)] private int _Damage;
    [SerializeField] [Range(1, 150)] private int _Concussion;
    [SerializeField] private float _FireRate;

    public int BaseDamage
    {
        get
        {
            return _Damage;
        }
    }

    public int Concussion
    {
        get
        {
            return _Concussion;
        }
    }

    public float FireRate
    {
        get
        {
            return 60f / _FireRate;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
