using UnityEngine;
using Unity.Netcode;

public class ItemHeal : NetworkBehaviour
{
    [SerializeField] private int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.TryGetComponent<HealthSystem>(out HealthSystem health))
        {
            health.Heal(healAmount);
            
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null) netObj.Despawn();
            else Destroy(gameObject);
        }
    }
}