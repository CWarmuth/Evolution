using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Object = System.Object;
using Random = Unity.Mathematics.Random;

public class WFCGenerator {
    private string rulesPath = "./Assets/Scripts/TileManager/Rules.tsv";
    
    
    public enum Direction {
        Up, Right, Down, Left
    }

//    public enum TileType {
//        None, DenseTrees, LightTrees, Grass, Sand, Water, DeepWater
//    }
    

    private Dictionary<int, TileDefinition> _tileDefinitions;
    private int[] _tileTypes;
    private List<string> tileNames;
    private FixedSizeQueue<Vector2Int> lastChoices;
    private Dictionary<int, string> idToTileName;

    private int width, height;

    public WFCGenerator(List<string> tileNames, int width, int height, int tilesToRevert) {
        _tileDefinitions = new Dictionary<int, TileDefinition>();
        lastChoices = new FixedSizeQueue<Vector2Int>(tilesToRevert);
        idToTileName = new Dictionary<int, string>();
        this.tileNames = tileNames;
        parseRules();
        this.width = width;
        this.height = height;
        _tileTypes = new int[width * height];
    }
 
    /**
     * Read the file stored at rulesPath and create the rules.
     */
    private void parseRules() {
        for (int i = 0; i < tileNames.Count; i++) {
            idToTileName.Add(i + 1, tileNames[i]);
        }
        
        StreamReader reader = File.OpenText(rulesPath);
        string line;
        line = reader.ReadLine(); // Skip first line
        if (line == null) {
            throw new InvalidDataException();
        }

        while ((line = reader.ReadLine()) != null) {
            string[] items = line.Split(' ');
            int primary = tileNames.FindIndex(x => x.Equals(items[0])) + 1;
            int secondary = tileNames.FindIndex(x => x.Equals(items[1])) + 1;
//            Enum.TryParse(items[0], out primary);
//            Enum.TryParse(items[1], out secondary);

            TileDefinition primaryTileDefinition;
            if (_tileDefinitions.ContainsKey(primary)) {
                primaryTileDefinition = _tileDefinitions[primary];
            }
            else {
                primaryTileDefinition = new TileDefinition(primary, 1);
                _tileDefinitions.Add(primary, primaryTileDefinition);
            }
            for (int i = 0; i < 4; i++) {
                if (items[2 + i].Equals("1")) {
                    Direction direction = (Direction) i;
                    Direction oppositeDirection;
                    switch (direction) {
                        case Direction.Up:
                            oppositeDirection = Direction.Down;
                            break;
                        case Direction.Right:
                            oppositeDirection = Direction.Left;
                            break;
                        case Direction.Down:
                            oppositeDirection = Direction.Up;
                            break;
                        case Direction.Left:
                            oppositeDirection = Direction.Right;
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                    
                    TileDefinition secondaryTileDefinition;
                    if (_tileDefinitions.ContainsKey(secondary)) {
                        secondaryTileDefinition = _tileDefinitions[secondary];
                    }
                    else {
                        secondaryTileDefinition = new TileDefinition(secondary, 1);
                        _tileDefinitions.Add(secondary, secondaryTileDefinition);
                    }
                    primaryTileDefinition.AllowTileInDirection(secondary, direction);
                    secondaryTileDefinition.AllowTileInDirection(primary, oppositeDirection);
                }
            }
        }
    }

    public bool Step(out Vector2Int position, out string tileType) {
        List<int> tileTypes = null;
        int index = -1;

        while (tileTypes is null || tileTypes.Count == 0) {
            UndoTiles();
            index = PickNextTiles(out tileTypes);
            if (index == -1) {
                position = Vector2Int.zero;
                tileType = null;
                return false;
            }
        }
        int random = UnityEngine.Random.Range(0, tileTypes.Count);
        int tt = tileTypes[random];
        _tileTypes[index] = tt;
        
        position = new Vector2Int(index % height, index / width);
        lastChoices.Enqueue(position);
        tileType = idToTileName[tt];
        return true;
    }

    private void UndoTiles() {
        for (int i = 0; i < lastChoices.Count(); i++) {
            Vector2Int position = lastChoices.Dequeue();
            _tileTypes[position.x + position.y * width] = -1;
        }
    }

    /*
     * Picks the next tile to collapse. Returns a Tuple of the index of the tile and a list of possible
     * TileTypes.
     */
    private int PickNextTiles(out List<int> tileTypes) {
        Tuple<int, int, List<int>> minWeight = new Tuple<int, int, List<int>>(-1, -1, new List<int>());

        for (int i = 0; i < width * height; i++) {
            if (_tileTypes[i] == 0) {
                List<int> potentialTileTypes = CellCanContain(i);
                int weight = potentialTileTypes.Count;
                if (weight == 1) {
                    minWeight = new Tuple<int, int, List<int>>(weight, i, potentialTileTypes);
                    break;
                }
                if (weight < minWeight.Item1 || minWeight.Item1 == -1) {
                    minWeight = new Tuple<int, int, List<int>>(weight, i, potentialTileTypes);
                }
            }
        }
        
        tileTypes = minWeight.Item3;
        return minWeight.Item2;
    }

    // Returns a list of possible tile types for a given index
    private List<int> CellCanContain(int index) {
        int up = index - width;
        int right = index + 1;
        int down = index + width;
        int left = index - 1;

        up = index >= width ? up : index; // Ensure Up is not on the first row
        right = index % width != width - 1 ? right : index; // Ensure Right is not in right column
        down = index < width * (height - 1) ? down : index; // Ensure Up is not on the last row
        left = index % width != 0 ? left : index;// Ensure Left is not in left column

        

//        Debug.Log("This cell at index " + index + " with value " + "can contain ");
        HashSet<int> possibleTypes = new HashSet<int>();
        
        foreach (int thisTileType in idToTileName.Keys) {
            bool tileIsAllowed = true;
            foreach (int i in Enum.GetValues(typeof(Direction))) {
                if (!tileIsAllowed) {
                    continue;
                }
                Direction d = (Direction) i;
                int directionIndex = -1;
                switch (d) {
                    case Direction.Down:
                        directionIndex = down;
                        break;
                    case Direction.Right:
                        directionIndex = right;
                        break;
                    case Direction.Left:
                        directionIndex = left;
                        break;
                    case Direction.Up:
                        directionIndex = up;
                        break;
                }
                
                

                int tileTypeInDirection = _tileTypes[directionIndex];
                TileDefinition td = _tileDefinitions[thisTileType];
                if (!td.CanPlace(tileTypeInDirection, d)) {
                    tileIsAllowed = false;
                }
            
            }

            if (tileIsAllowed) {
                possibleTypes.Add(thisTileType);
            }
            else {
                
            }
        }
        
        
        
        List<int> finalTypes = new List<int>();
        
        foreach (int tt in possibleTypes)
        {
            TileDefinition td = _tileDefinitions[tt];
            finalTypes.AddRange(Enumerable.Repeat(tt, td.GetFrequency()));
        }

        return finalTypes;
    }
}

// Stores the rules and frequencies of each tile
public class TileDefinition {
    private readonly int _type;
    private Dictionary<int, bool4> _allowedDirections;
    private readonly int frequencyHint;

    public TileDefinition(int type, int frequency) {
        _type = type;
        _allowedDirections = new Dictionary<int, bool4>();
        frequencyHint = frequency;
    }

    public int GetFrequency() {
        return frequencyHint;
    }

    public bool CanPlace(int tileType, WFCGenerator.Direction relativeDirection) {
        if (tileType == 0) {
            return true;
        }
        if (!_allowedDirections.ContainsKey(tileType)) {
            return false;
        }
        
        return _allowedDirections[tileType][(int) relativeDirection];
    }

    public void AllowTileInDirection(int tileType, WFCGenerator.Direction relativeDirection) {
        if (!_allowedDirections.ContainsKey(tileType)) {
            _allowedDirections.Add(tileType, new bool4());
        }
        bool4 allowable = _allowedDirections[tileType];
        allowable[(int) relativeDirection] = true;
        _allowedDirections[tileType] = allowable;
    }

    public override bool Equals(Object obj) {
        if ((obj == null) || this.GetType() != obj.GetType()) {
            return false;
        }
        else {
            TileDefinition td = (TileDefinition) obj;
            return td._type == _type;
        }
    }

    protected bool Equals(TileDefinition other) {
        return string.Equals(_type, other._type) && frequencyHint == other.frequencyHint;
    }

    public override int GetHashCode() {
        unchecked {
            return ((_type.GetHashCode()) * 397) ^ frequencyHint;
        }
    }
}

internal class FixedSizeQueue<T>
{
    private readonly int _maxSize;
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly object _queueLockObj = new object();
 
    internal FixedSizeQueue(int maxSize)
    {
        _maxSize = maxSize;
    }
 
    internal void Enqueue(T item)
    {
        lock (_queueLockObj)
        {
            if (_queue.Count == _maxSize)
            {
                _queue.Dequeue();
            }
 
            _queue.Enqueue(item);
        }
    }
 
    internal bool Contains(T item)
    {
        lock (_queueLockObj)
        {
            return _queue.Contains(item);
        }
    }

    internal T Dequeue() {
        lock (_queueLockObj) {
            return _queue.Dequeue();
        }
    }

    internal int Count() {
        lock (_queueLockObj) {
            return _queue.Count;
        }
    }
}