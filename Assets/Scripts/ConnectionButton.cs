using Unity.Netcode;
using UnityEngine;


public class ConnectionButton : MonoBehaviour
{
    public void SatartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
