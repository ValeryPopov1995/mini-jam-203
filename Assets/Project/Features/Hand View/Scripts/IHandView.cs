using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniJam203.HandView
{
    public interface IHandView
    {
        UniTask Pick(string container, Color color, float volume01 = 1);
        UniTask Drop();
        UniTask Drink();
        void SetVolume(float volume01);
    }
}