using UnityEngine;
using Unity.Netcode;

public class FlashlightItem : NetworkBehaviour
{
    [SerializeField] private AudioClip pickupSound;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.TryGetComponent(out PlayerLightController lightController))
        {
            lightController.IncreaseLightServerRpc();
            
            PlayPickupSoundClientRpc(collision.GetComponent<NetworkObject>().OwnerClientId);
            
            NetworkObject.Despawn();
        }
    }
    
    [ClientRpc]
    private void PlayPickupSoundClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;
        
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    }
}