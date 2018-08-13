using UnityEngine;
using MyGame.GameManagement;
using MyGame.Object;

public class StartingInventory : MonoBehaviour {

    [SerializeField]
    Item[] items;

    void Start()
    {
        items = GetComponentsInChildren<Item>();

        foreach(Item item in items)
        {
            GameManager.Instance.Player.AddItem(item);
        }
    }
}
