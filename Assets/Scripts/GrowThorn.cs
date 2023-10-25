using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrowThorn : MonoBehaviour
{
    private Tilemap levelTilemap, thornTilemap;
    [SerializeField] private TileTypes tileTypes;
    private float nextGrowTime;
    const float growInterval = 0.8f;
    private void Awake()
    {
        levelTilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        thornTilemap = GetComponent<Tilemap>();
        nextGrowTime = 0.0f;
    }

    private void Update()
    {
        

        if(Time.time >= nextGrowTime)
        {
            nextGrowTime = Time.time + growInterval;
            Grow();
        }
        
    }

    private void Grow()
    {
        List<Vector3Int> nextThornCells = new List<Vector3Int>();
        List<Vector3Int> nextStemCells = new List<Vector3Int>();

        BoundsInt bounds = thornTilemap.cellBounds;
        for(int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int thornCellPos = new Vector3Int(x, y, 0);
                TileBase tileOnThorn = thornTilemap.GetTile(thornCellPos);
                Vector3Int levelCellPos = levelTilemap.WorldToCell(thornTilemap.CellToWorld(thornCellPos));
                TileBase tileOnLevel = levelTilemap.GetTile(levelCellPos);

                if (tileOnThorn != null || tileOnLevel != tileTypes.emptyTile) continue;
                if (!HasNeighbour4Dir(thornTilemap, tileTypes.thornTile, thornCellPos)) continue;
                if (!HasNeighbour8Dir(tileTypes.wallTile, thornCellPos)
                    && !HasNeighbour8Dir(tileTypes.fragileTile, thornCellPos)
                    && !HasNeighbourBelow(tileTypes.bridgeTile, thornCellPos)) continue;
                nextThornCells.Add(new Vector3Int(x, y, 0));
            }
        }
        foreach(Vector3Int pos in nextThornCells)
        {
            thornTilemap.SetTile(pos, tileTypes.thornTile);
        }
    }

    private bool HasNeighbourBelow(TileBase tile, Vector3Int coord)
    {
        if (GetTileInLevelFromThornMap(coord + Vector3Int.down) == tile) return true;
        return false;
    }
    private bool HasNeighbour4Dir(Tilemap tilemap, TileBase tile, Vector3Int coord)
    {
        if (tilemap.GetTile(coord + Vector3Int.left) == tile) return true;
        if (tilemap.GetTile(coord + Vector3Int.right) == tile) return true;
        if (tilemap.GetTile(coord + Vector3Int.down) == tile) return true;
        if (tilemap.GetTile(coord + Vector3Int.up) == tile) return true;
        return false;
    }

    private bool HasNeighbour8Dir(TileBase tile, Vector3Int coord)
    {
        if (GetTileInLevelFromThornMap(coord + Vector3Int.left) == tile) return true;
        if (GetTileInLevelFromThornMap(coord + Vector3Int.right) == tile) return true;
        if (GetTileInLevelFromThornMap(coord + Vector3Int.down) == tile) return true;
        if (GetTileInLevelFromThornMap(coord + Vector3Int.up) == tile) return true;

        // Diagonal
        if (GetTileInLevelFromThornMap(coord + Vector3Int.left + Vector3Int.up) == tile) return true;
        if (GetTileInLevelFromThornMap(coord + Vector3Int.right + Vector3Int.up) == tile) return true;
        if (GetTileInLevelFromThornMap(coord + Vector3Int.left + Vector3Int.down) == tile) return true;
        if (GetTileInLevelFromThornMap(coord + Vector3Int.right + Vector3Int.down) == tile) return true;

        return false;
    }

    private TileBase GetTileInLevelFromThornMap(Vector3Int coord)
    {
        return levelTilemap.GetTile(levelTilemap.WorldToCell(thornTilemap.CellToWorld(coord)));
    }
}
