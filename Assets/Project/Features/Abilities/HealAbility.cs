using MiniJam203.Player;
using Project.Features.Abilities;
using UnityEngine;

public class HealAbility : Ability
{
    [SerializeField] private PlayerHealth _player;
    [SerializeField] private float _healPoints = 3;
    public override void Activate()
    {
        _player.Heal(_healPoints);
    }
}