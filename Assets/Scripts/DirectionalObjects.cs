using UnityEngine;

public enum ConnectionType { None, Small, Big }

[System.Serializable]
public class DirectionalObjects
{
    public Direction direction;
    public GameObject wall;
    public GameObject doorWall;
    
    public GameObject smallConnection;
    public GameObject bigConnection;

    public ConnectionType connectionType;

    public static Direction GetDirectionToTile(DungeonTile tile1, DungeonTile tile2)
    {
        if (tile2.GridCoordinates.x > tile1.GridCoordinates.x) return Direction.East;
        if (tile2.GridCoordinates.x < tile1.GridCoordinates.x) return Direction.West;
        if (tile2.GridCoordinates.z > tile1.GridCoordinates.z) return Direction.North;
        if (tile2.GridCoordinates.z < tile1.GridCoordinates.z) return Direction.South;
        return Direction.None;
    }

    public static Direction Clockwise(Direction direction)
    {
        if (direction == Direction.North) return Direction.East;
        if (direction == Direction.West) return Direction.North;
        if (direction == Direction.South) return Direction.West;
        if (direction == Direction.East) return Direction.South;
        return Direction.None;        
    }

    public static Direction CounterClockwise(Direction direction)
    {
        if (direction == Direction.North) return Direction.West;
        if (direction == Direction.West) return Direction.South;
        if (direction == Direction.South) return Direction.East;
        if (direction == Direction.East) return Direction.North;
        return Direction.None;        
    }
}