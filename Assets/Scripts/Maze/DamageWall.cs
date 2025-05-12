using UnityEngine;
using Unity.Netcode;

public class DamageWall : NetworkBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.collider.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            HealthSystem health = netObj.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }
    }
}