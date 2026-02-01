using UnityEngine;

namespace Project.Features.Abilities
{
    [System.Serializable]
    public class ShootingAbility : Ability
    {
        [Header("Arrow Settings")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float arrowSpeed = 50f;
        [SerializeField] private float arrowLifeTime = 5f;
        public LayerMask hitLayers = -1; 
        
        [Header("Damage")]
        public float damage = 25f;
        
        [Header("Effects")]
        public GameObject hitEffectPrefab;
        
        public override void Activate()
        {
            if (arrowPrefab == null) 
            {
                Debug.LogWarning("Arrow prefab не назначен!");
                return;
            }
            
            Camera cam = Camera.main;
            
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            
            Vector3 shootDirection;
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hitLayers))
            {
                Vector3 targetPoint = hit.point;
                shootDirection = (targetPoint - cam.transform.position).normalized;
            }
            else
            {
                shootDirection = cam.transform.forward;
            }
            
            Vector3 spawnPosition = cam.transform.position + cam.transform.forward * 1.5f;
            Quaternion arrowRotation = Quaternion.LookRotation(shootDirection);
            
            GameObject arrowObj = Instantiate(arrowPrefab, spawnPosition, arrowRotation);
            Arrow arrowScript = arrowObj.GetComponent<Arrow>();
            
            if (arrowScript != null)
            {
                arrowScript.Initialize(this, shootDirection, arrowSpeed, arrowLifeTime);
            }
            else
            {
                Rigidbody rb = arrowObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = shootDirection * arrowSpeed;
                    Destroy(arrowObj, arrowLifeTime);
                }
            }
        }
    }
}
