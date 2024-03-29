using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class TileTypes : ScriptableObject
{
    public List<TileBase> solidTiles;
    public List<TileBase> breakableTiles;
    public List<TileBase> floorTiles;
    public List<TileBase> bridgeTiles;
    public List<TileBase> thornTiles;

    public TileBase emptyTile, wallTile, fragileTile, enterTile, exitTile, thornTile, stemTile, bridgeTile;
}
