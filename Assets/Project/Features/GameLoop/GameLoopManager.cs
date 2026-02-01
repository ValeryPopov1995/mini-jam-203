using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniJam203.Menu
{
    public class GameLoopManager : MonoBehaviour
    {
        public event System.Action OnStartedGame;

        [SerializeField] private Menu _menu;
        [SerializeField] private Image _fade;

        [SerializeField] private GameObject[] _disableOnStart;
        [SerializeField] private GameObject[] _enableOnStart;

        [SerializeField] private Player.PlayerHealth _player;
        [SerializeField] private Player.SimpleInputHandler _input;

        private bool _isStartedGame = false;

        private void Awake()
        {
            _ = _fade.DOFade(0, 1).AsyncWaitForCompletion();
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

        private async void ShowDiedMenu(GameObject @object)
        {
            _player.OnDied -= ShowDiedMenu;
            await _fade.DOFade(1, 1).AsyncWaitForCompletion();
            SceneManager.LoadScene(0);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}