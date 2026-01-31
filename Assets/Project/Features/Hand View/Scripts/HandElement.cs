using UnityEngine;
using UnityEngine.UI;

namespace MiniJam203.HandView
{
    public class HandElement : MonoBehaviour
    {
        public Color Color
        {
            get => _fill.color;
            set => _fill.color = value;
        }
        public float Fill
        {
            get => _fill.fillAmount;
            set => _fill.fillAmount = Mathf.Clamp01(value);
        }

        [SerializeField] private Image _fill;
    }
}