using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrowThorn : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField] private TileBase emptyTile, wallTile, fragileTile, thornTile, bridgeTile;
    private float nextGrowTime;
    const float growInterval = 0.8f;
    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
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
        List<Vector3Int> nextGrowCells = new List<Vector3Int>();
        BoundsInt bounds = tilemap.cellBounds;
        for(int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile != emptyTile) continue;
                if (!HasNeighbour4Dir(thornTile, x, y)) continue;
                if (!HasNeighbour8Dir(wallTile, x, y)
                    && !HasNeighbour8Dir(fragileTile, x, y)
                    && !HasNeighbourBelow(bridgeTile, x, y)) continue;
                nextGrowCells.Add(new Vector3Int(x, y, 0));
            }
        }
        foreach(Vector3Int pos in nextGrowCells)
        {
            tilemap.SetTile(pos, thornTile);
        }
    }

    private bool HasNeighbourBelow(TileBase tile, int x, int y)
    {
        if (tilemap.GetTile(new Vector3Int(x, y - 1, 0)) == tile) return true;
        return false;
    }
    private bool HasNeighbour4Dir(TileBase tile, int x, int y)
    {
        if (tilemap.GetTile(new Vector3Int(x - 1, y, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x + 1, y, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x, y - 1, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x, y + 1, 0)) == tile) return true;
        return false;
    }

    private bool HasNeighbour8Dir(TileBase tile, int x, int y)
    {
        if (tilemap.GetTile(new Vector3Int(x - 1, y, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x + 1, y, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x, y - 1, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x, y + 1, 0)) == tile) return true;

        // Diagonal
        if (tilemap.GetTile(new Vector3Int(x - 1, y - 1, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x + 1, y + 1, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x + 1, y - 1, 0)) == tile) return true;
        if (tilemap.GetTile(new Vector3Int(x - 1, y + 1, 0)) == tile) return true;

        return false;
    }
}
