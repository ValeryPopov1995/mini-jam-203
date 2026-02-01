using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam203.ComboView
{
    public class ComboViewElement : MonoBehaviour
    {
        public Color Color
        {
            get => _color.color;
            set
            {
                _color.color = value;
                _canvasGroup.DOFade(_color.color == default ? 0 : 1, _fadeDuration);
            }
        }

        [SerializeField] private Image _color;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = .5f;

        internal void ResetColor()
        {
            _color.color = default;
            _canvasGroup.alpha = 0;
        }
    }
}