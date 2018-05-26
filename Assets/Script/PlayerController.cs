using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    /*
     * Controls the basic game logic - stats, inventory
     */
    [SerializeField] [Range(0, 200)] private int health = 200;

    Camera cam;
    PlayerWeapon weapon;
    public GameObject bulletMark;
    CharacterController characterCtrl;
    LayerMask noIgnoreRaycastLayer =  ~(1 << 2);

    float yaw = 0;
    float pitch = 0;

    public int Damage
    {
        get { return weapon.BaseDamage; }
    }

    void Awake()
    {
        cam = Camera.main;
        weapon = GetComponentInChildren<PlayerWeapon>();
        characterCtrl = GetComponent<CharacterController>();
    }
    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !(Cursor.lockState == CursorLockMode.Locked);
    }

    void Update()
    {

    }




}
