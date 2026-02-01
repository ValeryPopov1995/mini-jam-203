using UnityEngine;

namespace Project.Features.Abilities
{
    public class Vessel : MonoBehaviour
    {
        [SerializeField] private int maxCapacity = 3;
        public int currentAmount = 3;
        
        public bool dropped;
        public bool CanDrink => currentAmount > 0; 
        
        public void Sip()
        {
            if (!CanDrink) return;
            currentAmount--;
        }
        
        public void EmptyAndDrop()
        {
            currentAmount = 0;
            dropped = true;
        }
    }
}
