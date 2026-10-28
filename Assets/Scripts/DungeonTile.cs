using System.Collections.Generic;
using UnityEngine;

public class DungeonTile : MonoBehaviour
{
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject ceiling;

    [SerializeField] private GameObject northEastCorner;
    [SerializeField] private GameObject northWestCorner;
    [SerializeField] private GameObject southEastCorner;
    [SerializeField] private GameObject southWestCorner;

    [SerializeField] private GameObject[] northEastWalls;
    [SerializeField] private GameObject[] northWestWalls;
    [SerializeField] private GameObject[] southEastWalls;
    [SerializeField] private GameObject[] southWestWalls;

    [SerializeField] private List<DirectionalObjects> directionalObjects;

    private Dictionary<Direction, DirectionalObjects> directionalObjectDictionary;
    private List<DungeonTile> smallConnections = new();
    private List<DungeonTile> bigConnections = new();
    private Vector3Int gridCoordinates;

    public Vector3Int GridCoordinates => gridCoordinates;

    public void Initialize(int gridX, int gridZ)
    {
        gridCoordinates = new Vector3Int(gridX, 0, gridZ);

        directionalObjectDictionary = new Dictionary<Direction, DirectionalObjects>();

        foreach (DirectionalObjects objects in directionalObjects)
        {
            directionalObjectDictionary.Add(objects.direction, objects);
        }
    }

    public void CreateSmallConnection(DungeonTile tile)
    {
        if (smallConnections.Contains(tile))
        {
            return;
        }

        Direction direction = DirectionalObjects.GetDirectionToTile(this, tile);
        directionalObjectDictionary[direction].doorWall.SetActive(false);
        directionalObjectDictionary[direction].smallConnection.SetActive(true);
        directionalObjectDictionary[direction].connectionType = ConnectionType.Small;

        smallConnections.Add(tile);
    }

    public void CreateBigConnection(DungeonTile tile)
    {
        if (bigConnections.Contains(tile))
        {
            return;
        }

        Direction direction = DirectionalObjects.GetDirectionToTile(this, tile);
        directionalObjectDictionary[direction].smallConnection.SetActive(false);
        directionalObjectDictionary[direction].wall.SetActive(false);
        directionalObjectDictionary[direction].bigConnection.SetActive(true);
        directionalObjectDictionary[direction].connectionType = ConnectionType.Big;

        bigConnections.Add(tile);
        ApplyCorners();
    }

    public bool HasSmallConnection(DungeonTile tile)
    {
        return smallConnections.Contains(tile);
    }

    public bool HasAnySmallConnection()
    {
        return smallConnections.Count > 0;
    }

    public bool HasBigConnection(DungeonTile tile)
    {
        return bigConnections.Contains(tile);
    }

    public bool HasAnyBigConnection()
    {
        return bigConnections.Count > 0;
    }

    private void ApplyCorners()
    {
        if (bigConnections.Count < 2)
        {
            return;
        }

        bool north = bigConnections.Find(tile => tile.GridCoordinates.z > gridCoordinates.z) != null;
        bool south = bigConnections.Find(tile => tile.GridCoordinates.z < gridCoordinates.z) != null;

        bool east = bigConnections.Find(tile => tile.GridCoordinates.x > gridCoordinates.x) != null;
        bool west = bigConnections.Find(tile => tile.GridCoordinates.x < gridCoordinates.x) != null;

        if (north && east) 
        {
            northEastCorner.SetActive(true);
            foreach (GameObject wall in northEastWalls) wall.SetActive(false);
        }

        if (north && west)     
        {
            northWestCorner.SetActive(true);
            foreach (GameObject wall in northWestWalls) wall.SetActive(false);   
        }

        if (south && east)
        { 
            southEastCorner.SetActive(true);
            foreach (GameObject wall in southEastWalls) wall.SetActive(false);
        }

        if (south && west)
        {
            southWestCorner.SetActive(true);
            foreach (GameObject wall in southWestWalls) wall.SetActive(false);    
        }     
    }
}
