using UnityEngine;
using UnityEngine.UI;

namespace MiniJam203.HandView
{
    public class HandElement : MonoBehaviour
    {
        public Color Color
        {
            get => _fill ? _fill.color : Color.white;
            set
            {
                if (_fill) _fill.color = value;
            }
        }
        public float Fill
        {
            get => _fill ? _fill.fillAmount : default;
            set
            {
                if (_fill) _fill.fillAmount = Mathf.Clamp01(value);
            }
        }

        [SerializeField] private Image _fill;
    }
}