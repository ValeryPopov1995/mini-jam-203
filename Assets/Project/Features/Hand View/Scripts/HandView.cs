using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Linq;
using UnityEngine;

namespace MiniJam203.HandView
{
    public class HandView : MonoBehaviour, IHandView
    {
        [Serializable]
        private struct AnimData
        {
            public string AnimName;
            public float Duration;
            public AudioClip Clip;
            public HandElement[] Animation;
        }

        [SerializeField] private AudioSource _source;
        [SerializeField] private AnimData[] _animDatas;
        private HandElement[] _hands;

        private Color _color;
        private float _fill;

        private void Awake()
        {
            _hands = GetComponentsInChildren<HandElement>();
            HideHands();
        }

        [Button]
        public async UniTask DropCan()
        {
            await Anim("drop can");
        }

        public async UniTask PickCan(Color color)
        {
            ResetValues(color);
            await Anim("pick can");
        }

        private void ResetValues(Color color)
        {
            _color = color;
            _fill = 1;
            foreach (var hand in _hands)
            {
                hand.Color = color;
                hand.Fill = _fill;
            }
        }

        private async UniTask Anim(string animation)
        {
            HideHands();
            var data = _animDatas.First(data => data.AnimName == animation);
            var frameDuration = data.Duration / data.Animation.Length;
            if (data.Clip) _source.PlayOneShot(data.Clip);

            foreach (var hand in data.Animation)
            {
                hand.gameObject.SetActive(true);
                await UniTask.Delay(TimeSpan.FromSeconds(frameDuration));
                hand.gameObject.SetActive(false);
            }

            var lastHand = data.Animation.Last();
            lastHand.gameObject.SetActive(true);
        }

        private void HideHands()
        {
            foreach (var hand in _hands)
                hand.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        [Button] private void TestPickCan() => _ = PickCan(Color.blue);
#endif
    }
}