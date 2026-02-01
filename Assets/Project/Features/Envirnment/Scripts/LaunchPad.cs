using MiniJam203.Player;
using UnityEngine;

namespace MiniJam203.Environment
{
    public class LaunchPad : MonoBehaviour
    {
        [Header("Launch Settings")]
        [SerializeField] private float launchForce = 25f;
        [SerializeField] private Vector3 launchDirection = Vector3.up;
        [SerializeField] private float verticalMultiplier = 1.5f;
        [SerializeField] private float horizontalMultiplier = 1f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem launchEffect;
        [SerializeField] private AudioClip launchSound;

        [Header("Cooldown")]
        [SerializeField] private float cooldownTime = 0.5f;

        private AudioSource audioSource;
        private float lastLaunchTime;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && Time.time > lastLaunchTime + cooldownTime)
            {
                IPlayerMotor motor = other.GetComponent<IPlayerMotor>();
                if (motor != null)
                {
                    LaunchPlayer(motor);
                    lastLaunchTime = Time.time;
                }
            }
        }

        private void LaunchPlayer(IPlayerMotor motor)
        {
            Vector3 worldDirection = transform.TransformDirection(launchDirection).normalized;

            Vector3 launchVelocity = new Vector3(
                worldDirection.x * horizontalMultiplier,
                worldDirection.y * verticalMultiplier,
                worldDirection.z * horizontalMultiplier
            ) * launchForce;

            motor.SetVelocity(launchVelocity);

            PlayEffects();
        }

        private void PlayEffects()
        {
            if (launchEffect != null)
            {
                launchEffect.Play();
            }

            if (launchSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(launchSound);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 start = transform.position;
            Vector3 end = start + transform.TransformDirection(launchDirection) * 3f;

            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.3f);

            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(start, 0.5f);
        }
    }
}