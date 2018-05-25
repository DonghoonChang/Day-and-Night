using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {

    public string itemName = "New Item";
    public Texture2D itemIcon = null;
    public GameObject itemObject = null;
    public bool isUnique = false;
    public bool inConsumable = false;
    public bool isStackable = false;
    public bool destoryOnUse = false;
}
