using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MazeGenerator : NetworkBehaviour
{
    public int width = 21, height = 21;
    public GameObject wallPrefab, floorPrefab, startPrefab, exitPrefab, bearTrapPrefab;
    
    [Header("Background")]
    public GameObject backgroundPrefab;
    
    [Header("Items")]
    public GameObject itemHealPrefab;

    private int[,] maze;
    private Vector2Int startPos, exitPos;
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private Dictionary<Vector2Int, GameObject> tileObjects = new Dictionary<Vector2Int, GameObject>();

    private List<Vector3> trapWorldPositions = new List<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnBackground();
            GenerateMaze();
            SetExit();
            RemoveDeadEnds();
            SendMazeToClients();
            SpawnBearTraps(10);
            SpawnHealItems(10);
        }
    }
    
    void Awake()
    {
        Debug.Log("Awake - BearTrapPrefab: " + bearTrapPrefab);
    }


    void GenerateMaze()
    {
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maze[x, y] = 1;

        // ห้องกลาง 3x3
        startPos = new Vector2Int(width / 2, height / 2);
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            int x = startPos.x + dx;
            int y = startPos.y + dy;
            if (x >= 0 && x < width && y >= 0 && y < height)
                maze[x, y] = 0;
        }

        CarvePath(startPos.x, startPos.y);

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
        for (int y = 2; y < height - 2; y++)
            if (maze[x, y] == 0 && CountAdjacentWalls(x, y) >= 3)
                maze[x, y] = 1;
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

    void SpawnBearTraps(int count = 10)
    {
        if (bearTrapPrefab == null)
        {
            Debug.LogError("[MazeGenerator] BearTrap prefab is NULL! Please assign it in the Inspector.");
            return;
        }

        trapWorldPositions.Clear();
        List<Vector2Int> floorPositions = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (maze[x, y] == 0 && pos != startPos && pos != exitPos)
                floorPositions.Add(pos);
        }

        ShuffleList(floorPositions);

        for (int i = 0; i < Mathf.Min(count, floorPositions.Count); i++)
        {
            Vector2Int pos = floorPositions[i];
            Vector2 randomOffset = new Vector2(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.2f, 0.2f)
            );
            Vector3 worldPos = ToWorldPosition(pos) + (Vector3)randomOffset;

            GameObject trap = Instantiate(bearTrapPrefab, worldPos, Quaternion.identity);
            trap.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            trapWorldPositions.Add(worldPos);

            NetworkObject netObj = trap.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
                netObj.Spawn();
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

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
    
    void SpawnHealItems(int count = 10)
    {
        if (itemHealPrefab == null)
        {
            Debug.LogError("[MazeGenerator] HealItem prefab is NULL! Please assign it in the Inspector.");
            return;
        }

        HashSet<Vector2Int> trapGridPositions = new HashSet<Vector2Int>();
        foreach (Vector3 worldPos in trapWorldPositions)
        {
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(worldPos.x + width / 2f),
                Mathf.RoundToInt(worldPos.y + height / 2f)
            );
            trapGridPositions.Add(gridPos);
        }

        List<Vector2Int> validHealPositions = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (maze[x, y] == 0 && pos != startPos && pos != exitPos && !trapGridPositions.Contains(pos))
                validHealPositions.Add(pos);
        }

        ShuffleList(validHealPositions);

        for (int i = 0; i < Mathf.Min(count, validHealPositions.Count); i++)
        {
            Vector2Int pos = validHealPositions[i];
    
            Vector2 randomOffset = new Vector2(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.2f, 0.2f)
            );
    
            Vector3 worldPos = ToWorldPosition(pos) + (Vector3)randomOffset;

            GameObject healItem = Instantiate(itemHealPrefab, worldPos, Quaternion.identity);
            healItem.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            NetworkObject netObj = healItem.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
                netObj.Spawn();
        }
    }
    
    void SpawnBackground()
    {
        if (backgroundPrefab == null)
        {
            Debug.LogError("[MazeGenerator] BG prefab is NULL! Please assign it in the Inspector.");
            return;
        }
        
        float bgWidth = width + 20f;
        float bgHeight = height + 20f;
        
        Vector3 bgCenterPosition = new Vector3(0, 0, 5);

        GameObject bg = Instantiate(backgroundPrefab, bgCenterPosition, Quaternion.identity);
        bg.transform.localScale = new Vector3(bgWidth, bgHeight, 1);

        NetworkObject netObj = bg.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (trapWorldPositions != null)
        {
            foreach (var pos in trapWorldPositions)
            {
                Gizmos.DrawWireCube(pos, new Vector3(1, 1, 0));
            }
        }
    }
}
