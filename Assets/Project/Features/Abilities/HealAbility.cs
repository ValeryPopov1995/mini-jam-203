using MiniJam203.Player;
using Project.Features.Abilities;
using UnityEngine;

public class HealAbility : Ability
{
    private PlayerHealth _player;
    [SerializeField] private float _healPoints = 3;
    private void Start()
    {
        _player = FindAnyObjectByType<PlayerHealth>();
    }
    public override void Activate()
    {
        _player.Heal(_healPoints);
    }
}