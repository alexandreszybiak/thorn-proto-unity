using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class Bullet : MonoBehaviour
{
    private const float ppu = 12.0f;

    private Tilemap tilemap;
    private Sprite sprite;

    [SerializeField] private TileBase emptyTile, wallTile, fragileTile, thornTile, bridgeTile;
    [SerializeField] private float speed;

    private Vector2 velocity;
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
        velocity = Vector2.right * speed;

        tilemap = FindObjectOfType<Tilemap>();

    }
    void Update()
    {
        Vector3Int coord = tilemap.WorldToCell(transform.position);
        TileBase prout = tilemap.GetTile(coord);
        
        

        if (tilemap.GetTile(coord) == thornTile)
        {
            tilemap.SetTile(coord, emptyTile);
            Destroy(gameObject);
        }

        MoveX(velocity.x);
        MoveY(velocity.y);
    }

    private void MoveX(float amount)
    {
        xRemainder += amount;
        int move = Mathf.RoundToInt(xRemainder);
        if (move != 0)
        {
            xRemainder -= move;
            int sign = Math.Sign(move);
            while (move != 0)
            {
                var direction = new Vector3(sign, 0, 0) / ppu;

                if (OverlapTile(wallTile, transform.position + direction))
                {
                    Destroy(gameObject);
                    break;
                }
                else
                {
                    transform.Translate(direction);
                    move -= sign;
                }

            }
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
                if (OverlapTile(wallTile, transform.position + new Vector3(0, sign, 0) / ppu))
                {
                    Destroy(gameObject);
                    break;
                }
                else if (sign == -1 && OverlapTile(bridgeTile, transform.position + new Vector3(0, sign, 0) / ppu))
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

    private bool OverlapTile(TileBase tile, Vector3 position)
    {
        if (tilemap == null) return false;

        Vector3Int coord1 = tilemap.WorldToCell(position);
        Vector3Int coord2 = tilemap.WorldToCell(position + new Vector3((sprite.rect.width - 1) / ppu, (sprite.rect.height - 1) / ppu, 0));
        BoundsInt area = new BoundsInt(coord1, coord2 - coord1 + Vector3Int.one);
        TileBase[] tiles = new TileBase[area.size.x * area.size.y];
        tilemap.GetTilesBlockNonAlloc(area, tiles);
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == tile)
            {
                return true;
            }
        }
        return false;
    }
}
