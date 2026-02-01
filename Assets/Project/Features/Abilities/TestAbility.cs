using UnityEngine;

namespace Project.Features.Abilities
{
    public class TestAbility : Ability
    {
        public override void Activate()
        {
            Debug.Log($"<color=yellow>{abilityName}</color>: activated! 🌀");;
        }
    }
}
