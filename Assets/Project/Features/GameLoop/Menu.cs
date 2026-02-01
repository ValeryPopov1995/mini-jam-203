using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniJam203.Menu
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private GameLoopManager _manager;

        [SerializeField] private HandView.HandView _menuHandL;
        [SerializeField] private HandView.HandView _menuHandR;

        [SerializeField] private InputActionReference _startAction;
        [SerializeField] private InputActionReference _quitAction;

        [SerializeField] private string _animShowMenu = "menu show";
        [SerializeField] private string _animHideMenu = "menu drop";

        private async void OnEnable()
        {
            await UniTask.Delay(500);
            _ = _menuHandL.Anim(_animShowMenu);
            _ = _menuHandR.Anim(_animShowMenu);

            _startAction.action.Enable();
            _quitAction.action.Enable();

            _startAction.action.performed += InvokeStartGame;
            _quitAction.action.performed += InvokeQuit;
        }

        private void OnDisable()
        {
            _startAction.action.performed -= InvokeStartGame;
            _quitAction.action.performed -= InvokeQuit;

            _startAction?.action.Disable();
            _quitAction?.action.Disable();
        }

        private async void InvokeQuit(InputAction.CallbackContext context)
        {
            await _menuHandL.Anim(_animHideMenu);
            _manager.Quit();
        }

        private async void InvokeStartGame(InputAction.CallbackContext context)
        {
            await _menuHandR.Anim(_animHideMenu);
            _manager.StartNewGame();
            enabled = false;

        }
    }
}