using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class TileGenerator : MonoBehaviour {

    public MapParameters Paramaters;

    public Tilemap Tilemap;
//    public Tile DenseTrees, LightTrees, Grass, Sand, Water, DeepWater;

    public TileTexturePair[] tileList;
    private WFCGenerator _wfcGenerator;

    public int tilesPerFrame = 5;
    public int tilesToRevert = 5;
    

    [Serializable]
    public struct TileTexturePair {
        public string name;
        public Tile tile;
    }
 
    
    
    // Start is called before the first frame update
    void Start()
    {
        List<String> tileNames = new List<string>();
        foreach (TileTexturePair ttp in tileList)
        {
            tileNames.Add(ttp.name);
        }
        _wfcGenerator = new WFCGenerator(tileNames, Paramaters.Width, Paramaters.Height, tilesToRevert);
    }

    // Update is called once per frame
    void Update() {
       
        for (int i = 0; i < tilesPerFrame; i++)
            PlaceTiles();
    }

    private void PlaceTiles() {
        Vector2Int position;
        string type;
        if (_wfcGenerator.Step(out position, out type)) {
            Tile tile = Array.Find(tileList, x => x.name.Equals(type)).tile;
            Tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
            
        }
        else {
            Debug.Log("Failed to place tile.");
        }
    }
}