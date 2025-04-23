using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class BodyPlayer:NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            
            PlayerName.Value = userData.userName;
           
            Debug.Log(PlayerName.Value);
        }
    }
}
