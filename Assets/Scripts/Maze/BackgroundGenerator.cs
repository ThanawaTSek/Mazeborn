using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class BackgroundGenerator : NetworkBehaviour
{
    public GameObject backgroundTilePrefab;
    public int width = 20;
    public int height = 20;
    public float tileSize = 1f;

    private static bool hasSpawned = false;

    private void Start()
    {
        // ทำงานแค่ตอน Scene "Gameplay"
        if (SceneManager.GetActiveScene().name == "Gameplay" && IsServer && !hasSpawned)
        {
            GenerateBackground();
            hasSpawned = true;
        }
    }

    void GenerateBackground()
    {
        GameObject container = new GameObject("BackgroundContainer");

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Vector3 pos = new Vector3(x * tileSize, y * tileSize, 10f);

            GameObject tile = Instantiate(backgroundTilePrefab, pos, Quaternion.identity, container.transform);
            tile.GetComponent<NetworkObject>().Spawn(true);
        }

        Debug.Log("Background created by Server");
    }
}