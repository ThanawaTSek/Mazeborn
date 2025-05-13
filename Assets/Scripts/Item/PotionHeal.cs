using UnityEngine;
using Unity.Netcode;

public class ItemHeal : NetworkBehaviour
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.TryGetComponent<HealthSystem>(out HealthSystem health))
        {
            health.Heal(healAmount);
            
            ulong targetClientId = collision.GetComponent<NetworkObject>().OwnerClientId;
            PlayPickupSoundClientRpc(targetClientId);
            
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null) netObj.Despawn();
            else Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void PlayPickupSoundClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    }
}