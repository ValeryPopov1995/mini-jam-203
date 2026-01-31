using UnityEngine;

namespace Project.Features.Abilities
{
    [System.Serializable]
    public class ArrowAbility : Ability
    {
        [Header("Arrow Settings")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float arrowSpeed = 50f;
        [SerializeField] private float arrowLifeTime = 5f;
        public LayerMask hitLayers = -1;  // Что может поражать
        
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
            
            // 🎯 Raycast от центра экрана (перекрестие прицела)
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            
            Vector3 shootDirection;
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hitLayers))
            {
                // Точно в точку попадания
                Vector3 targetPoint = hit.point;
                shootDirection = (targetPoint - cam.transform.position).normalized;
                Debug.Log($"🎯 Цель: {hit.collider.name}");
            }
            else
            {
                // Если ничего не попало — прямо вперёд
                shootDirection = cam.transform.forward;
                Debug.Log("➡️ Стрела вперёд");
            }
            
            // Спавн чуть впереди камеры
            Vector3 spawnPosition = cam.transform.position + cam.transform.forward * 1.5f;
            Quaternion arrowRotation = Quaternion.LookRotation(shootDirection);
            
            // Создаём стрелу
            GameObject arrowObj = Instantiate(arrowPrefab, spawnPosition, arrowRotation);
            Arrow arrowScript = arrowObj.GetComponent<Arrow>();
            
            if (arrowScript != null)
            {
                arrowScript.Initialize(this, shootDirection, arrowSpeed, arrowLifeTime);
            }
            else
            {
                // Fallback с Rigidbody
                Rigidbody rb = arrowObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = shootDirection * arrowSpeed;
                    Destroy(arrowObj, arrowLifeTime);
                }
            }
            
            Debug.Log($"<color=orange>{this}</color>: Стрела выстрелена!");
        }
    }
}
