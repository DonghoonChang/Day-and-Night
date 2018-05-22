using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

    [SerializeField] [Range(1, 150)] private int m_Damage = 15;

    public int BaseDamage
    {
        get
        {
            return m_Damage;
        }
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
