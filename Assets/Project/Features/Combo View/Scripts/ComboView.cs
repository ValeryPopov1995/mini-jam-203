using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam203.ComboView
{
    public class ComboView : MonoBehaviour, IComboView
    {
        private ComboViewElement[] _elements;

        private void Awake()
        {
            _elements = GetComponentsInChildren<ComboViewElement>();
            foreach (var element in _elements)
                element.ResetColor();
        }

        public async UniTask AddColor(Color color)
        {
            if (_elements.Any(e => e.Color == default))
                SetColorToNext(color);
            else
                OverflowColor(color);

            await UniTask.Yield();

            void SetColorToNext(Color color)
            {
                for (int i = 0; i < _elements.Length; i++)
                {
                    if (_elements[i].Color == default)
                    {
                        _elements[i].Color = color;
                        break;
                    }
                }
            }

            void OverflowColor(Color color)
            {
                for (int i = 0; i < _elements.Length - 1; i++)
                    _elements[i].Color = _elements[i + 1].Color;
                _elements.Last().Color = color;
            }
        }

        public async UniTask ResetColors()
        {
            foreach (var element in _elements)
                element.Color = default;
            await UniTask.Yield();
        }

#if UNITY_EDITOR
        [Button]
        private void TestAddColor()
        {
            Color[] colors = new Color[]
            {
                Color.red, Color.green, Color.blue,
            };
            var color = colors[Random.Range(0, colors.Length)];
            _ = AddColor(color);
        }
        [Button] private void TestResetColor() => _ = ResetColors();
#endif
    }
}