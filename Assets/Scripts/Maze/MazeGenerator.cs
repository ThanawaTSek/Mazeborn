using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MazeGenerator : NetworkBehaviour
{
    public int width = 21, height = 21;
    public GameObject wallPrefab, floorPrefab, startPrefab, exitPrefab;
    public float scaleFactor = 1;

    private int[,] maze;
    private Vector2Int startPos, exitPos;
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    // เก็บ tile ทั้งหมดที่สร้างแล้ว (สำหรับลบกำแพงได้)
    private Dictionary<Vector2Int, GameObject> tileObjects = new Dictionary<Vector2Int, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateMaze();
            SetStartRoom();
            SetExit();
            RemoveDeadEnds();
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

        CreateSpawnRoom();
    }

    void CreateSpawnRoom()
    {
        int roomSize = 3;
        int centerX = width / 2;
        int centerY = height / 2;

        for (int x = -roomSize / 2; x <= roomSize / 2; x++)
        for (int y = -roomSize / 2; y <= roomSize / 2; y++)
            maze[centerX + x, centerY + y] = 0;
    }

    void CarvePath(int startX, int startY)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));
        maze[startX, startY] = 0;

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            Shuffle(directions);
            bool carved = false;

            foreach (Vector2Int dir in directions)
            {
                int newX = current.x + dir.x * 2;
                int newY = current.y + dir.y * 2;

                if (IsInsideMaze(newX, newY) && maze[newX, newY] == 1)
                {
                    maze[current.x + dir.x, current.y + dir.y] = 0;
                    maze[newX, newY] = 0;
                    stack.Push(new Vector2Int(newX, newY));
                    carved = true;
                    break;
                }
            }

            if (!carved)
                stack.Pop();
        }
    }

    void SetStartRoom()
    {
        int roomSize = 3;
        startPos = new Vector2Int(1, 1);

        for (int x = 0; x < roomSize; x++)
        {
            for (int y = 0; y < roomSize; y++)
            {
                int posX = startPos.x + x;
                int posY = startPos.y + y;
                maze[posX, posY] = 0;
            }
        }

        Vector3 startRoomPosition = new Vector3(
            (startPos.x + roomSize / 2f) * scaleFactor - (width / 2f * scaleFactor),
            (startPos.y + roomSize / 2f) * scaleFactor - (height / 2f * scaleFactor),
            0
        );

        GameObject startObj = Instantiate(startPrefab, startRoomPosition, Quaternion.identity);
        startObj.transform.localScale = new Vector3(roomSize * scaleFactor, roomSize * scaleFactor, 1);
    }

    void SetExit()
    {
        int exitY = Random.Range(2, height - 3);
        exitPos = new Vector2Int(width - 3, exitY);
        maze[exitPos.x, exitPos.y] = 0;
        
        maze[exitPos.x - 1, exitPos.y] = 0;
        
        if (tileObjects.ContainsKey(exitPos))
        {
            Destroy(tileObjects[exitPos]);
            tileObjects.Remove(exitPos);
        }
        if (tileObjects.ContainsKey(new Vector2Int(exitPos.x - 1, exitPos.y)))
        {
            Destroy(tileObjects[new Vector2Int(exitPos.x - 1, exitPos.y)]);
            tileObjects.Remove(new Vector2Int(exitPos.x - 1, exitPos.y));
        }
        
        Vector3 exitWorldPos = new Vector3(
            (exitPos.x - width / 2f) * scaleFactor,
            (exitPos.y - height / 2f) * scaleFactor,
            0
        );

        GameObject exitObj = Instantiate(exitPrefab, exitWorldPos, Quaternion.identity);
        exitObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        NetworkObject netObj = exitObj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn();
    }


    void RemoveDeadEnds()
    {
        for (int x = 2; x < width - 2; x++)
        {
            for (int y = 2; y < height - 2; y++)
            {
                if (maze[x, y] == 0 && CountAdjacentWalls(x, y) >= 3)
                {
                    maze[x, y] = 1;
                }
            }
        }
    }

    int CountAdjacentWalls(int x, int y)
    {
        int count = 0;
        foreach (Vector2Int dir in directions)
        {
            int checkX = x + dir.x;
            int checkY = y + dir.y;
            if (maze[checkX, checkY] == 1)
                count++;
        }
        return count;
    }

    void SendMazeToClients()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float offsetX = width / 2f;
                float offsetY = height / 2f;
                Vector2 pos = new Vector2((x - offsetX) * scaleFactor, (y - offsetY) * scaleFactor);

                GameObject prefabToSpawn = (maze[x, y] == 1) ? wallPrefab : floorPrefab;
                GameObject tile = Instantiate(prefabToSpawn, pos, Quaternion.identity);
                tile.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

                tileObjects[new Vector2Int(x, y)] = tile;

                NetworkObject netObj = tile.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                {
                    netObj.Spawn();
                }
            }
        }
    }

    void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = Random.Range(i, array.Length);
            Vector2Int temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }

    bool IsInsideMaze(int x, int y)
    {
        return x > 1 && x < width - 2 && y > 1 && y < height - 2;
    }
}