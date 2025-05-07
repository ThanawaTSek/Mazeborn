using UnityEngine;
using Unity.Netcode;

public class FlashlightItem : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.TryGetComponent(out PlayerLightController lightController))
        {
            lightController.IncreaseLightServerRpc();
            NetworkObject.Despawn();
        }
    }
}