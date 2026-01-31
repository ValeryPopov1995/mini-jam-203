using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniJam203.HandView
{
    public interface IHandView
    {
        UniTask PickCan(Color color);
        UniTask DropCan();
    }
}