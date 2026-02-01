using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [Tooltip("Transform дочернего объекта, который содержит модель/Animator.")]
    [SerializeField] Transform visualTransform;

    [Tooltip("Animator на Visual (может быть null, тогда только позиция/поворот).")]
    [SerializeField] Animator visualAnimator;

    // Поворот визуала к цели
    public void FaceVisualTowards(Transform target)
    {
        if (visualTransform == null || target == null) return;

        Vector3 dir = (target.position - visualTransform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;

        visualTransform.rotation = Quaternion.Slerp(
            visualTransform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 10f
        );
    }

    public void SetMoving(bool value)
    {
        if (visualAnimator != null)
            visualAnimator.SetBool("IsMoving", value);
    }

    public void PlayAttack()
    {
        if (visualAnimator != null)
            visualAnimator.SetTrigger("Attack");
    }

    public void PlayDeath()
    {
        if (visualAnimator != null)
            visualAnimator.SetTrigger("Die");
    }
}