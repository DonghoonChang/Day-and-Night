using UnityEngine;

namespace MyGame.Inventory.Weapon
{
    public class Ammo : Item
    {
        [SerializeField]
        AmmoCard ammoCard;

        [SerializeField]
        int _quantity;

        #region Properties

        public override Sprite Icon
        {
            get
            {
                return ammoCard.icon;
            }
        }

        public override string Name
        {
            get
            {
                return ammoCard.name;
            }
        }


        public override string Description
        {
            get
            {
                return ammoCard.description;
            }
        }


        public AmmoType Type
        {
            get
            {
                return ammoCard.type;
            }
        }

        public int Quantity
        {
            get
            {
                return _quantity;
            }

            set
            {
                _quantity = Mathf.Max(0, value);
            }
        }

        #endregion
    }
}
