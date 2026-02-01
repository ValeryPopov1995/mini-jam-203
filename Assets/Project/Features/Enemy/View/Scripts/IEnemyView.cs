using Cysharp.Threading.Tasks;

namespace MiniJam203
{
    public interface IEnemyView
    {
        UniTask Attack();
        UniTask GetDamage();
    }
}