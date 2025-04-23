using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MazeGenerator : NetworkBehaviour
{
    public int width = 21, height = 21;
    public GameObject wallPrefab, floorPrefab, startPrefab, exitPrefab;

    private int[,] maze;
    private Vector2Int startPos, exitPos;
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private Dictionary<Vector2Int, GameObject> tileObjects = new Dictionary<Vector2Int, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateMaze();
            SetExit();
            RemoveDeadEnds();
            SendMazeToClients();
        }
    }

    void GenerateMaze()
    {
        maze = new int[width, height];

        // 1. ทุกตำแหน่งเป็นกำแพง
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maze[x, y] = 1;

        // 2. เคลียร์ห้อง 3x3 กลาง
        startPos = new Vector2Int(width / 2, height / 2);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int x = startPos.x + dx;
                int y = startPos.y + dy;
                if (x >= 0 && x < width && y >= 0 && y < height)
                    maze[x, y] = 0;
            }
        }

        // ✅ 3. ขุดทางเดินต่อจากกลาง
        CarvePath(startPos.x, startPos.y);

        // 4. วาง startPrefab ตรงกลาง
        Vector3 startWorldPos = ToWorldPosition(startPos);
        GameObject startObj = Instantiate(startPrefab, startWorldPos, Quaternion.identity);
        startObj.transform.localScale = Vector3.one;

        NetworkObject netObj = startObj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn();
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

    void SetExit()
    {
        int exitY = Random.Range(2, height - 3);
        exitPos = new Vector2Int(width - 3, exitY);
        maze[exitPos.x, exitPos.y] = 0;
        maze[exitPos.x - 1, exitPos.y] = 0;

        DestroyIfExists(exitPos);
        DestroyIfExists(new Vector2Int(exitPos.x - 1, exitPos.y));

        Vector3 exitWorldPos = ToWorldPosition(exitPos);
        GameObject exitObj = Instantiate(exitPrefab, exitWorldPos, Quaternion.identity);
        exitObj.transform.localScale = Vector3.one;

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
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = ToWorldPosition(gridPos);

                GameObject prefabToSpawn = (maze[x, y] == 1) ? wallPrefab : floorPrefab;
                GameObject tile = Instantiate(prefabToSpawn, worldPos, Quaternion.identity);
                tile.transform.localScale = Vector3.one;

                tileObjects[gridPos] = tile;

                NetworkObject netObj = tile.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                    netObj.Spawn();
            }
        }
    }

    void DestroyIfExists(Vector2Int pos)
    {
        if (tileObjects.ContainsKey(pos))
        {
            Destroy(tileObjects[pos]);
            tileObjects.Remove(pos);
        }
    }

    Vector3 ToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x - width / 2f, gridPos.y - height / 2f, 0);
    }

    public Vector3 GetStartWorldPosition()
    {
        return ToWorldPosition(startPos);
    }

    bool IsInsideMaze(int x, int y)
    {
        return x > 1 && x < width - 2 && y > 1 && y < height - 2;
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
}