using Project.Features.Abilities;
using System.Collections;
using UnityEngine;

public class Refill : MonoBehaviour
{
    [Header("Refill Amount")]
    [SerializeField] private int refillAmountLeft = 3;    
    [SerializeField] private int refillAmountRight = 3;   

    [Header("Dispenser Mode")]
    [SerializeField] private bool isDispenser = false;     
    [SerializeField] private float dispenserCooldown = 5f; 

    [Header("Auto Destroy")]
    [SerializeField] private bool destroyAfterUse = true;  

    private AbilityManager abilityManager;
    private float lastDispenseTime;
    private bool isOnCooldown;

    private void Start()
    {
        abilityManager = FindAnyObjectByType<AbilityManager>();
        if (abilityManager == null)
            Debug.LogError("Refill: AbilityManager не найден!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanRefill()) return;

        RefillVessels();
        UseRefill();
    }

    private bool CanRefill()
    {
        if (!isDispenser && isOnCooldown) return false;
        return true;
    }

    private void RefillVessels()
    {
        if (abilityManager == null) return;

        if (abilityManager.leftVessel != null)
        {
            if (refillAmountLeft == 0)
                abilityManager.leftVessel.currentAmount = abilityManager.leftVessel.maxCapacity;
            else
                abilityManager.leftVessel.currentAmount = Mathf.Min(
                    abilityManager.leftVessel.maxCapacity,
                    abilityManager.leftVessel.currentAmount + refillAmountLeft
                );
            abilityManager.leftVessel.dropped = false;
        }

        if (abilityManager.rightVessel != null)
        {
            if (refillAmountRight == 0)
                abilityManager.rightVessel.currentAmount = abilityManager.rightVessel.maxCapacity;
            else
                abilityManager.rightVessel.currentAmount = Mathf.Min(
                    abilityManager.rightVessel.maxCapacity,
                    abilityManager.rightVessel.currentAmount + refillAmountRight
                );
            abilityManager.rightVessel.dropped = false;
        }
    }

    private void UseRefill()
    {
        if (isDispenser)
        {
            isOnCooldown = true;
            StartCoroutine(DispenserCooldown());
        }
        else if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DispenserCooldown()
    {
        yield return new WaitForSeconds(dispenserCooldown);
        isOnCooldown = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
