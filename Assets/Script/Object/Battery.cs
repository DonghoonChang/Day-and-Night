using UnityEngine;

namespace Game.Object
{
    public class Battery : Item
    {
        public float amount = 500f; // In seconds

        public override string Name {
            get {
                return "Battery";

            }
        }

        public override string Description {
            get
            {
                return "Refills Flashlight";
            }
        }
    }
}
