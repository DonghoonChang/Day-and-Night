using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

    [SerializeField] [Range(1, 150)] private int _Damage;
    [SerializeField] [Range(1, 150)] private int _Force;

    public int BaseDamage
    {
        get
        {
            return _Damage;
        }
    }

    public int Force
    {
        get
        {
            return _Force;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
