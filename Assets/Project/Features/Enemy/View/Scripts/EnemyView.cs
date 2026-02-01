using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace MiniJam203
{
    public class EnemyView : MonoBehaviour, IEnemyView, IDisposable
    {
        private class AnimationData
        {
            private readonly float _frameDuration;
            private readonly GameObject[] _frames;

            public AnimationData(Transform framesParent, float duration)
            {
                _frameDuration = duration / framesParent.childCount;

                _frames = new GameObject[framesParent.childCount];
                for (var i = 0; i < framesParent.childCount; i++)
                {
                    _frames[i] = framesParent.GetChild(i).gameObject;
                    _frames[i].SetActive(false);
                }
            }

            public async UniTask PlayCircle()
            {
                for (var i = 0; i < _frames.Length; i++)
                {
                    try
                    {
                        _frames[i].SetActive(true);
                    }
                    catch { }

                    await UniTask.Delay(TimeSpan.FromSeconds(_frameDuration));

                    try
                    {
                        _frames[i].SetActive(false);
                    }
                    catch { }
                }
            }

            public void Stop()
            {
                for (var i = 0; i < _frames.Length; i++)
                    _frames[i].SetActive(false);
            }
        }

        [SerializeField] private float _moveCircleDuration = 1;
        [SerializeField] private float _attackDuration = 1;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Transform _moveFrameParent;
        [SerializeField] private Transform _attackFrameParent;

        private AnimationData _move, _attack;
        private bool _moveCircle;

        private void Awake()
        {
            _move = new(_moveFrameParent, _moveCircleDuration);
            _attack = new(_attackFrameParent, _attackDuration);
        }

        private async void Start()
        {
            _moveCircle = true;
            while (_moveCircle)
                await _move.PlayCircle();
        }

        [Button]
        public async UniTask Attack()
        {
            _moveFrameParent.gameObject.SetActive(false);
            _move.Stop();
            await _attack.PlayCircle();
            _moveFrameParent.gameObject.SetActive(true);
        }

        public async UniTask GetDamage()
        {
            await _sprite.material.DOColor(Color.red, .5f).AsyncWaitForCompletion();
            await _sprite.material.DOColor(Color.white, .5f).AsyncWaitForCompletion();
        }

        public void Dispose()
        {
            _moveCircle = false;
        }
    }
}