using MiniJam203.HandView;
using MiniJam203.Menu;
using UnityEngine;

namespace Project.Features.Abilities
{
    public class Vessel : MonoBehaviour
    {
        public int maxCapacity = 5;
        public int CurrentAmount
        {
            get => _currentAmount;
            set
            {
                if (_currentAmount == 0 && value > 0) _ = _hand.Pick("can", Color);
                else if (_currentAmount > value) _ = _hand.Drink();
                //else if (value == 0) _ = _hand.Drop();

                _currentAmount = value;
            }
        }
        private int _currentAmount;

        public Color Color = Color.red;

        public bool Dropped => !CanDrink;
        public bool CanDrink => CurrentAmount > 0;


        [SerializeField] private HandView _hand;
        private GameLoopManager _manager;

        private void Awake()
        {
            _manager = FindFirstObjectByType<GameLoopManager>();
            _manager.OnStartedGame += PickCanAtStart;
        }

        private void OnDestroy()
        {
            _manager.OnStartedGame -= PickCanAtStart;
        }

        private void PickCanAtStart()
        {
            CurrentAmount = maxCapacity;
        }

        public void Sip()
        {
            if (!CanDrink) return;
            CurrentAmount--;
        }
    }
}
