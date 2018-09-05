using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Object;
using TMPro;

namespace Game.UI {

    public class UIItemSlot : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        #region Setting

        static int ItemNameDisplaySpeed = 4;

        #endregion

        [SerializeField]
        UIInventoryPanel _inventoryPanel;

        [SerializeField]
        TextMeshProUGUI _itemText;

        [SerializeField]
        int _index;

        Item _item;
        Button _button;
        GameObject _itemProp;

        public Item Item
        {
            set
            {
                if (value != null)
                {
                    if (_itemProp != null)
                        Destroy(_itemProp);

                    _itemProp = Instantiate(value.gameObject, transform);
                    _itemProp.transform.localPosition = Vector3.zero;
                    _itemProp.transform.localRotation = Quaternion.LookRotation(Vector3.zero);
                    _itemProp.transform.localScale = value.GetComponent<Item>().scaleInGUI;
                    _itemProp.SetActive(true);

                    _item = value;
                }

                else
                {
                    _item = null;

                    if (_itemProp != null)
                    {
                        Destroy(_itemProp);
                        _itemProp = null;
                    }
                }
            }
        }

        public void Awake()
        {
            _itemText.text = "";

            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClickHandler);
        }

        private void OnClickHandler()
        {
            _inventoryPanel.OnButtonClicked(_index);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _inventoryPanel.OnButtonSelected(_index);

            StopAllCoroutines();
            StartCoroutine(ShowItemName());
            StartCoroutine(RotateProp());
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _itemText.text = "";

            StopAllCoroutines();
        }

        private IEnumerator ShowItemName()
        {
            if (_item != null)
            {
                int counter = 0;
                string itemName = _item.name;

                _itemText.text = "";

                for (int i = 0; i < itemName.Length;)
                {
                    if (counter % ItemNameDisplaySpeed == 0)
                    {
                        _itemText.text += _item.Name[i];

                        i++;
                    }

                    counter++;
                    yield return null;
                }
            }
        }

        private IEnumerator RotateProp()
        {
            if (_itemProp != null)
            {
                _itemProp.transform.localRotation = Quaternion.LookRotation(Vector3.zero);

                while (true)
                {
                    _itemProp.transform.Rotate(new Vector3(0f, 1f, 0f), 2f);
                    yield return null;

                }
            }
        }
    }
}

