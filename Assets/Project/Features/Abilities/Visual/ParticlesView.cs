using NaughtyAttributes;
using UnityEngine;

public class ParticlesView : MonoBehaviour
{
    public void SetColor(Color newColor)
    {
        var particleSystems = GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.startColor = newColor;
            ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }

#if UNITY_EDITOR
    [Button] private void TestColor() => SetColor(Color.red);
#endif
}
