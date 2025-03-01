using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // ใช้ Netcode

public class MazeGenerator : NetworkBehaviour
{
    public int width = 21, height = 21;
    public GameObject wallPrefab, floorPrefab, startPrefab, exitPrefab;
    private int[,] maze;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer) // ให้เซิร์ฟเวอร์สร้างแผนที่
        {
            GenerateMaze();
            SendMazeToClients();
        }
    }

    void GenerateMaze()
    {
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maze[x, y] = 1;

        int startX = Random.Range(1, width / 2) * 2;
        int startY = Random.Range(1, height / 2) * 2;
        CarvePath(startX, startY);
    }

    void CarvePath(int x, int y)
    {
        maze[x, y] = 0;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x * 2;
            int newY = y + dir.y * 2;

            if (newX > 0 && newX < width - 1 && newY > 0 && newY < height - 1 && maze[newX, newY] == 1)
            {
                maze[x + dir.x, y + dir.y] = 0;
                CarvePath(newX, newY);
            }
        }
    }

    void SendMazeToClients()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            SetMazeClientRpc(x, y, maze[x, y]);
    }

    [ClientRpc]
    void SetMazeClientRpc(int x, int y, int value)
    {
        maze[x, y] = value;
        Vector2 pos = new Vector2(x, y);
        Instantiate(value == 1 ? wallPrefab : floorPrefab, pos, Quaternion.identity);
    }
}