using UnityEngine;

namespace Game.Object.Weapon
{
    public class Ammo : Item
    {
        [SerializeField]
        AmmoCard ammoCard;

        [SerializeField]
        protected int _count;

        #region Properties

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

        public virtual int Count
        {
            get
            {
                return _count;
            }

            set
            {
                _count = Mathf.Max(0, value);
            }
        }

        #endregion

    }
}
