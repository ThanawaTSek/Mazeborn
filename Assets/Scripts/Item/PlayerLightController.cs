using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Netcode;

public class PlayerLightController : NetworkBehaviour
{
    [SerializeField] private Light2D flashlight;

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseLightServerRpc()
    {
        IncreaseLightClientRpc();
    }

    [ClientRpc]
    private void IncreaseLightClientRpc()
    {
        if (flashlight != null && flashlight.lightType == Light2D.LightType.Point)
        {
            flashlight.pointLightOuterRadius += 0.5f;
        }
    }
}