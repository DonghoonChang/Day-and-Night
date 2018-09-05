using UnityEngine;
using Item = Game.Object.Item;


namespace Game.Enemy
{
    public class EnemyInventory : MonoBehaviour
    {
        public GameObject[] items;

        public void DropLoots()
        {
            foreach (GameObject item in items)
            {
                GameObject loot = Instantiate(item, null, true);
                Item itemScript = loot.transform.GetComponent<Item>();

                if (item == null)
                    continue;

                else
                {
                    itemScript.gameObject.SetActive(true);
                    itemScript.transform.SetParent(null);
                    itemScript.ToggleInventoryMode(false);

                    Debug.Log(itemScript.transform.name);
                    itemScript.transform.position = transform.position + transform.up;

                    Rigidbody lootRB = loot.transform.GetComponent<Rigidbody>();

                    if (lootRB != null)
                        lootRB.AddForce(transform.forward + transform.up, ForceMode.Impulse);
                }
            }
        }
    }

}
