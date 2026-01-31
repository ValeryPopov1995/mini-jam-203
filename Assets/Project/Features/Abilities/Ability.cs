using UnityEngine;

namespace Project.Features.Abilities
{
    public abstract class Ability : MonoBehaviour
    {
        [SerializeField] protected  string abilityName = "Basic Ability";
        [SerializeField] protected  float cooldown = 1f;

        public abstract void Activate();

        public float Cooldown => cooldown;
        public string AbilityName => abilityName;
    }
}