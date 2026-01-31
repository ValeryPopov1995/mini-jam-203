using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniJam203.Menu
{
    public class GameLoopManager : MonoBehaviour
    {
        public event System.Action OnStartedGame;

        [SerializeField] private HandView.HandView _menuHandL;
        [SerializeField] private HandView.HandView _menuHandR;
        [SerializeField] private InputActionReference _startAction;
        [SerializeField] private InputActionReference _quitAction;

        [SerializeField] private GameObject[] _disableOnStart;
        [SerializeField] private GameObject[] _enableOnStart;

        [SerializeField] private Player.PlayerHealth _player;
        [SerializeField] private Camera _playerCam;

        private bool _isStartedGame = false;

        private void Awake()
        {
            EnableMenu();
        }

        public async void StartNewGame()
        {
            if (!_isStartedGame)
            {
                _isStartedGame=true;
                DisableMenu();
                await _menuHandR.Drop();

                foreach(var item in _disableOnStart) item.SetActive(false);
                foreach(var item in _enableOnStart) item.SetActive(true);

                OnStartedGame?.Invoke();
            }
        }

        public void Quit()
        {

        }

        private void EnableMenu()
        {
            _startAction.action.Enable();
            _quitAction.action.Enable();

            _startAction.action.performed += InvokeStartGame;
            _quitAction.action.performed += InvokeQuit;
        }

        private void DisableMenu()
        {
            _startAction.action.performed -= InvokeStartGame;
            _quitAction.action.performed -= InvokeQuit;

            _startAction?.action.Disable();
            _quitAction?.action.Disable();
        }

        private void InvokeQuit(InputAction.CallbackContext context)
        {
            Quit();
        }

        private void InvokeStartGame(InputAction.CallbackContext context)
        {
            StartNewGame();
        }
    }
}