using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniJam203.HandView
{
    public interface IHandView
    {
        UniTask GetCan(Color color);
        UniTask DropCan();
    }
}