using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int gridSize = 16;
    [SerializeField] private int tileCount = 64;
    [SerializeField] private float tileSpacing = 20f;
    [SerializeField] private float DelayBetweenTileGeneration = 0.1f;
    [SerializeField] private float randomSmallConnectionProbability = 0.4f;
    [SerializeField] private DungeonTile dungeonTilePrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Camera dungeonCamera;
    [SerializeField] private int cameraHeight = 250;

    private DungeonTile[,,] grid;
    private DungeonTile currentTile;
    private int currentTileCount;
    private int yIndexZero = 0;

    public int GridSize => gridSize;
    public DungeonTile[,,] Grid => grid;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        grid = new DungeonTile[gridSize, 1, gridSize];

        dungeonCamera.orthographic = true;
        dungeonCamera.orthographicSize = cameraHeight;
        dungeonCamera.transform.position = new Vector3((gridSize / 2f) * tileSpacing - 5, 1, (gridSize / 2f) * tileSpacing - 5);
        // using -5 here just to center the camera a bit better 

        StartCoroutine(GenerateDungeon());
    }

    private IEnumerator GenerateDungeon()
    {
        yield return StartCoroutine(GenerateTiles());

        yield return StartCoroutine(AddRandomSmallConnections(randomSmallConnectionProbability));

        yield return StartCoroutine(CreateBigSquareConnections());

        yield return StartCoroutine(CreateBigConnections());

        SpawnPlayer();

        dungeonCamera.gameObject.SetActive(false);
    }

    private IEnumerator GenerateTiles()
    {
        while (currentTileCount < tileCount)
        {
            yield return new WaitForSeconds(DelayBetweenTileGeneration);

            GenerateNextTile();
        }
    }

    private void GenerateNextTile()
    {
        if (currentTile == null)
        {
            Vector3Int gridCoordinates = new Vector3Int(gridSize/2, 0, gridSize/2);
            currentTile = GenerateTileAtGridPosition(gridCoordinates);
            return;
        }

        List<Direction> availableDirections = GetAvailableDirectionFromTile(currentTile);
        if(availableDirections.Count == 0)
        {
            currentTile = FirstTileWithAvailableNeighbours();
            availableDirections = GetAvailableDirectionFromTile(currentTile);
        }

        Direction direction = availableDirections[Random.Range(0, availableDirections.Count)];

        DungeonTile previousTile = currentTile;
        currentTile = GenerateTileAtGridPosition(currentTile.GridCoordinates + DirectionUtility.directionOffsets[direction]);
        
        previousTile.CreateSmallConnection(currentTile);
        currentTile.CreateSmallConnection(previousTile);
    }

    private IEnumerator AddRandomSmallConnections(float connectionChance)
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (grid[i, yIndexZero, j] == null)
                {
                    continue;
                }

                if (i < gridSize - 1)
                {
                    yield return StartCoroutine(TryAddSmallConnection(grid[i, yIndexZero, j], grid[i + 1, yIndexZero, j], connectionChance));
                }

                if (j < gridSize - 1)
                {
                    yield return StartCoroutine(TryAddSmallConnection(grid[i, yIndexZero, j], grid[i, yIndexZero, j + 1], connectionChance));
                }
            }
        }
    }

    private IEnumerator TryAddSmallConnection(DungeonTile tile1, DungeonTile tile2, float connectionChance)
    {
        if (tile1 == null || tile2 == null || tile1.HasSmallConnection(tile2))
        {
            yield break;
        }

        if (Random.value < connectionChance)
        {
            yield return new WaitForSeconds(DelayBetweenTileGeneration);

            tile1.CreateSmallConnection(tile2);
            tile2.CreateSmallConnection(tile1);
        }
    }

    private IEnumerator CreateBigConnections()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (grid[i, yIndexZero, j] == null || grid[i, yIndexZero, j].HasAnyBigConnection())
                {
                    continue;
                }

                if (i > 0 && i < gridSize - 1)
                {
                    DungeonTile[] tilesToConnect = new DungeonTile[] { grid[i-1, yIndexZero, j], grid[i, yIndexZero, j], grid[i+1, yIndexZero, j] };
                    yield return StartCoroutine(TryBigConnectTiles(tilesToConnect));
                }

                if (j > 0 && j < gridSize - 1)
                {
                    DungeonTile[] tilesToConnect = new DungeonTile[] { grid[i, yIndexZero, j-1], grid[i, yIndexZero, j], grid[i, yIndexZero, j+1] };
                    yield return StartCoroutine(TryBigConnectTiles(tilesToConnect));
                }
            }
        }
    }

    private IEnumerator TryBigConnectTiles(DungeonTile[] tilesToConnect)
    {
        if (!CanBigConnect(tilesToConnect))
        {
            yield break;
        }

        for (int i = 0; i < tilesToConnect.Length - 1; i++)
        {
            yield return new WaitForSeconds(DelayBetweenTileGeneration);

            tilesToConnect[i].CreateBigConnection(tilesToConnect[i + 1]);
            tilesToConnect[i + 1].CreateBigConnection(tilesToConnect[i]);
        }
    }

    private bool CanBigConnect(DungeonTile[] tiles)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null || tiles[i].HasAnyBigConnection())
            {
                return false;
            }

            if (i < tiles.Length - 1 && !tiles[i].HasSmallConnection(tiles[i + 1]))
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator CreateBigSquareConnections()
    {
        for (int i = 0; i < gridSize - 1; i++)
        {
            for (int j = 0; j < gridSize - 1; j++)
            {
                yield return StartCoroutine(TryBigConnectSquare(
                    grid[i, yIndexZero, j],
                    grid[i + 1, yIndexZero, j],
                    grid[i, yIndexZero, j + 1],
                    grid[i + 1, yIndexZero, j + 1]
                ));
            }
        }
    }

    private IEnumerator TryBigConnectSquare(DungeonTile topLeft, DungeonTile topRight, DungeonTile bottomLeft, DungeonTile bottomRight)
    {
        if (topLeft == null || topRight == null || bottomLeft == null || bottomRight == null)
        {
            yield break;
        }
        
        if (topLeft.HasAnyBigConnection() || topRight.HasAnyBigConnection() || bottomLeft.HasAnyBigConnection() || bottomRight.HasAnyBigConnection())
        {
            yield break;
        }

        if (!topLeft.HasSmallConnection(topRight) || !topRight.HasSmallConnection(bottomRight) || !bottomRight.HasSmallConnection(bottomLeft) || !bottomLeft.HasSmallConnection(topLeft))
        {
            yield break;
        }

        yield return new WaitForSeconds(DelayBetweenTileGeneration);

        TryBigConnectTiles(topLeft, topRight);
        TryBigConnectTiles(topRight, bottomRight);
        TryBigConnectTiles(bottomRight, bottomLeft);
        TryBigConnectTiles(bottomLeft, topLeft);
    }

    private void TryBigConnectTiles(DungeonTile tile1, DungeonTile tile2)
    {
        tile1.CreateBigConnection(tile2);
        tile2.CreateBigConnection(tile1);
    }

    private DungeonTile GenerateTileAtGridPosition(Vector3Int gridCoordinates)
    {
        Vector3 position = new Vector3(gridCoordinates.x * tileSpacing, yIndexZero, gridCoordinates.z * tileSpacing);

        DungeonTile tile = Instantiate(dungeonTilePrefab, position, Quaternion.identity, transform);

        tile.Initialize(gridCoordinates.x, gridCoordinates.z);

        grid[gridCoordinates.x, yIndexZero, gridCoordinates.z] = tile;

        currentTileCount++;
            
        return tile;
    }

    private List<Direction> GetAvailableDirectionFromTile(DungeonTile tile)
    {
        List<Direction> result = new List<Direction>();

        foreach (Direction direction in DirectionUtility.directions)
        {
            Vector3Int coordinates = tile.GridCoordinates + DirectionUtility.directionOffsets[direction];
        
            if (coordinates.x < 0 || coordinates.z < 0 || coordinates.x >= gridSize || coordinates.z >= gridSize)
            {
                continue;
            }

            if (grid[coordinates.x, yIndexZero, coordinates.z] == null)
            {
                result.Add(direction);
            }
        }

        return result;
    }

    private DungeonTile FirstTileWithAvailableNeighbours()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (grid[i, yIndexZero, j] != null && GetAvailableDirectionFromTile(grid[i, yIndexZero, j]).Count > 0)
                {
                    return grid[i, yIndexZero, j];
                }
            }
        }

        return null;
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3((gridSize / 2f) * tileSpacing - 5, 1, (gridSize / 2f) * tileSpacing - 5);
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity, transform);
    }

}
