using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniJam203.ComboView
{
    public interface IComboView
    {
        UniTask AddColor(Color color);
        UniTask ResetColors();
    }
}