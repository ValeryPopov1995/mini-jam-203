using MiniJam203.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Project.Features.Abilities
{
    public class AbilityManager : MonoBehaviour
    {
        [Header("Potions")]
        public Vessel leftVessel;
        public Vessel rightVessel;

        [SerializeField] private InputAction leftGulp;
        [SerializeField] private InputAction rightGulp;

        [SerializeField] private float drinkWindow = .5f;

        [SerializeField] private AbilityMixRatio[] abilities;

        [SerializeField] private int maxTotalGulps = 3;
        public UnityEvent onOverload;
        public float overloadDamage;
        private int leftCount = 0;
        private int rightCount = 0;
        private float drinkingSartTime;
        private bool isDrinkingWindowActive = false;

        private PlayerHealth _player;
        private void OnEnable()
        {
            leftGulp?.Enable();
            rightGulp?.Enable();

            leftGulp.performed += OnLeftGulp;
            rightGulp.performed += OnRightGulp;

            _player = FindAnyObjectByType<PlayerHealth>();
            onOverload.AddListener(() => _player.TakeDamage(overloadDamage, gameObject));
        }

        private void OnDisable()
        {
            leftGulp.performed -= OnLeftGulp;
            rightGulp.performed -= OnRightGulp;

            leftGulp?.Disable();
            rightGulp?.Disable();
        }

        private void Update()
        {
            if (isDrinkingWindowActive && Time.time > drinkingSartTime + drinkWindow)
            {
                ProcessMix();
            }
        }

        private void OnLeftGulp(InputAction.CallbackContext context)
        {
            if (leftVessel.Dropped) return;
            if (!leftVessel.CanDrink)
            {
                leftVessel.EmptyAndDrop();
                return;
            }

            leftVessel.Sip();
            leftCount++;
            StartDrinkingWindow();
        }

        private void OnRightGulp(InputAction.CallbackContext context)
        {
            if (rightVessel.Dropped) return;
            if (!rightVessel.CanDrink)
            {
                rightVessel.EmptyAndDrop();
                return;
            }

            rightVessel.Sip();
            rightCount++;
            StartDrinkingWindow();
        }

        private void StartDrinkingWindow()
        {
            if (!isDrinkingWindowActive)
            {
                isDrinkingWindowActive = true;
                drinkingSartTime = Time.time;
            }
        }

        private void ProcessMix()
        {
            int totalSwings = leftCount + rightCount;

            Debug.Log($"Mix complete: L={leftCount}, R={rightCount}, Total={totalSwings}");

            if (totalSwings > maxTotalGulps)
            {
                Debug.LogWarning("💥 OVERLOAD!");
                onOverload?.Invoke();
                ResetMix();
                return;
            }

            foreach (var config in abilities)
            {
                if (config.leftCount == leftCount && config.rightCount == rightCount)
                {
                    config.ability.Activate();
                    break;
                }
            }

            ResetMix();
        }

        private void ResetMix()
        {
            leftCount = 0;
            rightCount = 0;
            isDrinkingWindowActive = false;
        }

        [System.Serializable]
        public class AbilityMixRatio
        {
            public int leftCount;
            public int rightCount;
            [SerializeReference] public Ability ability;
        }
    }
}
