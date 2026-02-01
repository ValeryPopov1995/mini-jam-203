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
        private string _container;
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

        public async UniTask Pick(string container, Color color, float volume01 = 1)
        {
            _container = container;
            _color = color;
            _fill = volume01;

            foreach (var hand in _hands)
            {
                hand.Color = color;
                hand.Fill = _fill;
            }

            await Anim("pick " + _container);
        }

        public async UniTask Drop()
        {
            await Anim("drop " + _container);
        }

        [Button]
        public async UniTask Drink()
        {
            await Anim("drink " + _container);
        }

        public void SetVolume(float volume01)
        {
            _fill = volume01;
            foreach (var hand in _hands)
                hand.Fill = _fill;
        }

        public async UniTask Anim(string animation)
        {
            if (!_animDatas.Any(data => data.AnimName == animation)) return;

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
        [Button] private void TestPickCan() => _ = Pick("can", Color.blue, .5f);
        [SerializeField] private string _testAnim = "menu drop";
        [Button] private void TestAnim() => _ = Anim(_testAnim);
#endif
    }
}