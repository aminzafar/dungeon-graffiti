using System.Collections.Generic;
using UnityEngine;

public enum Direction { None, North, South, East, West }

public class DirectionUtility
{
    public static List<Direction> directions = new List<Direction>()
    {
        Direction.North,
        Direction.South,
        Direction.East,
        Direction.West
    };

    public static Dictionary<Direction, Vector3Int> directionOffsets = new Dictionary<Direction, Vector3Int>()
    {
        {Direction.North, Vector3Int.forward},
        {Direction.South, Vector3Int.back},
        {Direction.East, Vector3Int.right},
        {Direction.West, Vector3Int.left}
    };
}