using UnityEngine;

namespace MiniJam203.Menu
{
    public class GameLoopManager : MonoBehaviour
    {
        public event System.Action OnStartedGame;

        [SerializeField] private Menu _menu;

        [SerializeField] private GameObject[] _disableOnStart;
        [SerializeField] private GameObject[] _enableOnStart;

        [SerializeField] private Player.PlayerHealth _player;
        [SerializeField] private Player.SimpleInputHandler _input;

        private bool _isStartedGame = false;

        private void Awake()
        {
            _menu.gameObject.SetActive(true);
            _input.enabled = false;
        }

        public async void StartNewGame()
        {
            if (!_isStartedGame)
            {
                _isStartedGame = true;

                _input.enabled = true;

                _player.OnDied += ShowDiedMenu;

                foreach (var item in _disableOnStart) item.SetActive(false);
                foreach (var item in _enableOnStart) item.SetActive(true);

                OnStartedGame?.Invoke();
            }
        }

        private void ShowDiedMenu(GameObject @object)
        {
            _player.OnDied -= ShowDiedMenu;
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}