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

    float yaw = 0;
    float pitch = 0;

    public int Damage
    {
        get { return weapon.BaseDamage * GetDamageMultiplier(); }
    }

    void Awake()
    {
        cam = Camera.main;
        weapon = GetComponentInChildren<PlayerWeapon>();
    }

    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !(Cursor.lockState == CursorLockMode.Locked);
    }

    int GetDamageMultiplier()
    {
        return 1;
    }



}
