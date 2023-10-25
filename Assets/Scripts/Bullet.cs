using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bullet : MonoBehaviour
{
    private const float ppu = 12.0f;

    private Tilemap levelTilemap;
    private Tilemap thornTilemap;
    private Sprite sprite;

    [SerializeField] private TileTypes tileTypes;
    [SerializeField] private float speed;

    private Vector2 myVelocity;
    public Vector2 Velocity
    {
        get
        {
            return myVelocity;
        }
        set
        {
            myVelocity = value * speed;
        }
    }

    private float xRemainder, yRemainder;

    private void Awake()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        sprite = spriteRenderer.sprite;
    }
    private void OnEnable()
    {
        xRemainder = 0.0f;
        yRemainder = 0.0f;

        levelTilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        thornTilemap = GameObject.Find("ThornTilemap").GetComponent<Tilemap>();

    }
    void Update()
    {
        
        MoveX(myVelocity.x);
        MoveY(myVelocity.y);

        
    }

    private void MoveX(float amount)
    {
        xRemainder += amount;
        int move = Mathf.RoundToInt(xRemainder);
        if (move == 0) return;
        
        xRemainder -= move;
        int sign = Math.Sign(move);

        while (move != 0)
        {
            var direction = new Vector3(sign, 0, 0) / ppu;

            bool shouldStop = false;

            ManageOverlap(levelTilemap, transform.position + direction, out shouldStop);
            ManageOverlap(thornTilemap, transform.position + direction, out shouldStop);

            if (shouldStop) break;

            transform.Translate(direction);
            move -= sign;
        }
        
    }

    private void MoveY(float amount)
    {
        yRemainder += amount;
        int move = Mathf.RoundToInt(yRemainder);
        if (move != 0)
        {
            yRemainder -= move;
            int sign = Math.Sign(move);
            while (move != 0)
            {
                if (OverlapTile(tileTypes.solidTiles, transform.position + new Vector3(0, sign, 0) / ppu))
                {
                    Destroy(gameObject);
                    break;
                }
                else if (sign == -1 && OverlapTile(tileTypes.bridgeTile, transform.position + new Vector3(0, sign, 0) / ppu))
                {
                    Destroy(gameObject);
                    break;
                }
                else
                {
                    transform.Translate(new Vector3(0, sign, 0) / ppu);
                    move -= sign;
                }

            }
        }

    }

    private void ManageOverlap(Tilemap tilemap, Vector3 position, out bool stopMovement)
    {
        stopMovement = false;
        var tiles = new List<Vector3Int>();

        Vector3Int coord1 = tilemap.WorldToCell(position);
        Vector3Int coord2 = tilemap.WorldToCell(position + new Vector3((sprite.rect.width - 1) / ppu, (sprite.rect.height - 1) / ppu, 0));

        for(int x = coord1.x; x <= coord2.x; x++)
        {
            for(int y = coord1.y; y <= coord2.y; y++)
            {
                var coord = new Vector3Int(x, y, 0);
                var tile = tilemap.GetTile(coord);

                if (tile == tileTypes.wallTile)
                {
                    stopMovement = true;
                    Collide();
                }
                if (tileTypes.breakableTiles.Contains(tile))
                {
                    tilemap.SetTile(coord, null);
                    stopMovement = true;
                    Collide();
                }
                
            }
        }
    }
    private bool OverlapTile(TileBase tile, Vector3 position)
    {
        if (levelTilemap == null) return false;

        Vector3Int coord1 = levelTilemap.WorldToCell(position);
        Vector3Int coord2 = levelTilemap.WorldToCell(position + new Vector3((sprite.rect.width - 1) / ppu, (sprite.rect.height - 1) / ppu, 0));
        BoundsInt area = new BoundsInt(coord1, coord2 - coord1 + Vector3Int.one);
        TileBase[] tiles = new TileBase[area.size.x * area.size.y];
        levelTilemap.GetTilesBlockNonAlloc(area, tiles);
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == tile)
            {
                return true;
            }
        }
        return false;
    }

    private bool OverlapTile(List<TileBase> tileList, Vector3 position)
    {
        if (levelTilemap == null) return false;

        Vector3Int coord1 = levelTilemap.WorldToCell(position);
        Vector3Int coord2 = levelTilemap.WorldToCell(position + new Vector3((sprite.rect.width - 1) / ppu, (sprite.rect.height - 1) / ppu, 0));
        BoundsInt area = new BoundsInt(coord1, coord2 - coord1 + Vector3Int.one);
        TileBase[] tiles = new TileBase[area.size.x * area.size.y];
        levelTilemap.GetTilesBlockNonAlloc(area, tiles);
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tileList.Contains(tiles[i])) return true;
        }
        return false;
    }

    private void Collide()
    {
        Destroy(gameObject);
    }
}
