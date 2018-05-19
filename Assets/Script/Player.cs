using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [Range(0, 200)]
    public int health = 200;

    [Range(1f, 10f)]
    public float speed_walk = 5f;

    [Range(5f, 20f)]
    public float speed_run = 10f;

    [Range(35f, 150f)]
    public float mass = 85f;


    void Awake()
    {
        
    }
    // Use this for initialization
    void Start () {
        Debug.Log("Here");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {
        if (Input.GetButton("Forward"))
        {
            transform.Translate(Vector3.forward * speed_walk * Time.deltaTime);
        }
        if (Input.GetButton("Backward"))
        {
            transform.Translate(Vector3.back * speed_walk * Time.deltaTime);
        }
        if (Input.GetButton("Left"))
        {
            transform.Translate(Vector3.left * speed_walk * Time.deltaTime);
        }

        if (Input.GetButton("Right"))
        {
            transform.Translate(Vector3.right * speed_walk * Time.deltaTime);
        }
    }
}
