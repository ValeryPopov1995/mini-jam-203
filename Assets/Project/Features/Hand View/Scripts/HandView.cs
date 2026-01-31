using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniJam203.HandView
{
    public class HandView : MonoBehaviour, IHandView
    {
        [SerializeField] private HandElement[] _hands;
        [SerializeField] private Animator _animator;
        private Color _color;
        private float _fill;

        public UniTask DropCan()
        {
            throw new System.NotImplementedException();
        }

        public async UniTask GetCan(Color color)
        {
            _color = color;
            _fill = 1;

            await UniTask.Delay(1);
        }
    }
}