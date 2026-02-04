using System;
using UnityEngine;
using UnityEngine.InputSystem;

// План (псевдокод) — подробно:
// 1) Перевести обработку ввода движения и взгляда с опроса в Update на реакцию на события InputAction.
// 2) Подписаться на _move.performed и _move.canceled, чтобы обновлять MoveDirection только по событиям.
//    - В обработчике performed: считать Vector2 из контекста, определить устройство через context.control.device,
//      применить deadzone для геймпада, присвоить MoveDirection (x,0,y).
//    - В обработчике canceled: очистить MoveDirection.
// 3) Подписаться на _look.performed и _look.canceled, чтобы обновлять LookDelta только по событиям.
//    - В обработчике performed: считать Vector2, определить isGamepad через context.control.device,
//      применять gamepad sensitivity * Time.deltaTime для геймпада или mouseSensitivity для мыши,
//      учитывать deadzone для геймпада, присвоить LookDelta.
//    - В обработчике canceled: очистить LookDelta.
// 4) Сохранить обработчики кнопок Jump/Sprint/Crouch как есть (performed/canceled).
// 5) В OnEnable кэшировать и Enable() все actions и подписаться на события (move, look, jump, sprint, crouch).
// 6) В OnDisable — отписаться от всех событий и Disable() actions, очистить ссылки.
// 7) Сохраняем LateUpdate для сброса одно-кадровых флагов (JumpPressed и т.д.) и обнуления LookDelta,
//    чтобы другие скрипты успевали прочитать значения в их Update.
// 8) Удалить Update() и методы, опрашивающие вход каждый кадр.
// 9) Обработчики должны быть безопасны при null и не полагаться на Update.
// 10) Убедиться, что isGamepad обновляется на каждом событии ввода и в случае отсутствия события fallback на Gamepad.current.

namespace MiniJam203.Player
{
    public class SimpleInputHandler : MonoBehaviour, IMoveInput
    {
        // IMoveInput implementation
        public Vector3 MoveDirection { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool SprintPressed { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool CrouchPressed { get; private set; }
        public bool CrouchHeld { get; private set; }

        // Events
        public event Action OnJumpPerformed;
        public event Action OnSprintPerformed;
        public event Action OnCrouchPerformed;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference crouchAction;

        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float gamepadLookSensitivity = 50f;
        [SerializeField] private float gamepadDeadzone = 0.1f;

        // Кэш локальных InputAction для удобства и производительности
        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _sprint;
        private InputAction _crouch;

        private bool isGamepad;

        private void OnEnable()
        {
            // Кэшируем actions
            _move = moveAction?.action;
            _look = lookAction?.action;
            _jump = jumpAction?.action;
            _sprint = sprintAction?.action;
            _crouch = crouchAction?.action;

            // Включаем actions
            _move?.Enable();
            _look?.Enable();
            _jump?.Enable();
            _sprint?.Enable();
            _crouch?.Enable();

            // Подписываемся на события
            if (_move != null)
            {
                _move.performed += HandleMovePerformed;
                _move.canceled += HandleMoveCanceled;
            }

            if (_look != null)
            {
                _look.performed += HandleLookPerformed;
                _look.canceled += HandleLookCanceled;
            }

            if (_jump != null)
            {
                _jump.performed += HandleJumpPerformed;
                _jump.canceled += HandleJumpCanceled;
            }

            if (_sprint != null)
            {
                _sprint.performed += HandleSprintPerformed;
                _sprint.canceled += HandleSprintCanceled;
            }

            if (_crouch != null)
            {
                _crouch.performed += HandleCrouchPerformed;
                _crouch.canceled += HandleCrouchCanceled;
            }
        }

        private void OnDisable()
        {
            // Отписываемся от всех событий
            if (_move != null)
            {
                _move.performed -= HandleMovePerformed;
                _move.canceled -= HandleMoveCanceled;
            }

            if (_look != null)
            {
                _look.performed -= HandleLookPerformed;
                _look.canceled -= HandleLookCanceled;
            }

            if (_jump != null)
            {
                _jump.performed -= HandleJumpPerformed;
                _jump.canceled -= HandleJumpCanceled;
            }

            if (_sprint != null)
            {
                _sprint.performed -= HandleSprintPerformed;
                _sprint.canceled -= HandleSprintCanceled;
            }

            if (_crouch != null)
            {
                _crouch.performed -= HandleCrouchPerformed;
                _crouch.canceled -= HandleCrouchCanceled;
            }

            _move?.Disable();
            _look?.Disable();
            _jump?.Disable();
            _sprint?.Disable();
            _crouch?.Disable();

            // Очистить ссылки
            _move = _look = _jump = _sprint = _crouch = null;
        }

        // Сбрасываем одно-кадровые Pressed-флаги в LateUpdate чтобы другие скрипты успели прочитать в Update
        private void LateUpdate()
        {
            JumpPressed = false;
            SprintPressed = false;
            CrouchPressed = false;

            // Сбрасываем LookDelta, чтобы не накапливать дельту между кадрами
            LookDelta = Vector2.zero;
        }

        // Обработчики для движения (событий, а не Update)
        private void HandleMovePerformed(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();

            // Определяем устройство по context (приоритет)
            InputDevice device = context.control?.device;
            if (device != null)
            {
                isGamepad = device is Gamepad;
            }
            else
            {
                isGamepad = Gamepad.current != null;
            }

            if (isGamepad && input.magnitude < gamepadDeadzone)
                input = Vector2.zero;

            MoveDirection = new Vector3(input.x, 0f, input.y);
        }

        private void HandleMoveCanceled(InputAction.CallbackContext context)
        {
            MoveDirection = Vector3.zero;
        }

        // Обработчики для взгляда (событий, а не Update)
        private void HandleLookPerformed(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();

            // Определяем устройство по context (приоритет)
            InputDevice device = context.control?.device;
            if (device != null)
            {
                isGamepad = device is Gamepad;
            }
            else
            {
                isGamepad = Gamepad.current != null;
            }

            if (isGamepad)
            {
                if (input.magnitude < gamepadDeadzone)
                    input = Vector2.zero;

                input *= gamepadLookSensitivity * Time.deltaTime;
            }
            else
            {
                input *= mouseSensitivity;
            }

            LookDelta = input;
        }

        private void HandleLookCanceled(InputAction.CallbackContext context)
        {
            LookDelta = Vector2.zero;
        }

        // Button Event Handlers
        private void HandleJumpPerformed(InputAction.CallbackContext context)
        {
            JumpPressed = true; // одно-кадровый
            JumpHeld = true;
            OnJumpPerformed?.Invoke();
        }

        private void HandleJumpCanceled(InputAction.CallbackContext context)
        {
            JumpPressed = false;
            JumpHeld = false;
        }

        private void HandleSprintPerformed(InputAction.CallbackContext context)
        {
            SprintPressed = true;
            SprintHeld = true;
            OnSprintPerformed?.Invoke();
        }

        private void HandleSprintCanceled(InputAction.CallbackContext context)
        {
            SprintPressed = false;
            SprintHeld = false;
        }

        private void HandleCrouchPerformed(InputAction.CallbackContext context)
        {
            CrouchPressed = true;
            CrouchHeld = true;
            OnCrouchPerformed?.Invoke();
        }

        private void HandleCrouchCanceled(InputAction.CallbackContext context)
        {
            CrouchPressed = false;
            CrouchHeld = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
