using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PixelCharacter : MonoBehaviour
{
    private Tilemap tilemap;

    [SerializeField] private TileBase wallTile, enterTile, exitTile, thornTile, bridgeTile;
    [SerializeField] private float walkSpeed, gravity, jumpForce;

    private Vector2 velocity;
    private float xRemainder, yRemainder;
    private bool onFloor;
    private bool canDoubleJump;
    private Sprite sprite;

    private const float ppu = 12.0f;

    public event Action exitLevel;
    public event Action died;
    private void Awake()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        sprite = spriteRenderer.sprite;
    }
    void Start()
    {
        
        
    }

    private void OnEnable()
    {
        xRemainder = 0.0f;
        yRemainder = 0.0f;
        velocity = Vector2.zero;

        tilemap = FindObjectOfType<Tilemap>();
        transform.position = GetEnterCoord();
    }

    void Update()
    {
        if (OverlapTile(wallTile, transform.position + new Vector3(0, -1, 0) / ppu) || OverlapTile(bridgeTile, transform.position + new Vector3(0, -1, 0) / ppu))
        {
            onFloor = true;
        }

        velocity.x = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow)) velocity.x = -walkSpeed;

        if (Input.GetKey(KeyCode.RightArrow)) velocity.x = walkSpeed;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (onFloor)
            {
                velocity.y = jumpForce;
            }
        
            else if (canDoubleJump)
            {
                velocity.y = jumpForce;
                canDoubleJump = false;
            }
        }

        if(Input.GetKeyDown(KeyCode.DownArrow) && OverlapTile(exitTile, transform.position))
        {
            exitLevel?.Invoke();
        }

        if(!canDoubleJump && OverlapTile(wallTile, transform.position + new Vector3(0, -1, 0) / ppu))
        {
            canDoubleJump = true;
        }

        velocity.y += gravity;
        //velocity.y = Mathf.Clamp(velocity.y, 0.0f, )

        MoveX(velocity.x);
        MoveY(velocity.y);

        if(OverlapTile(thornTile, transform.position))
        {
            died?.Invoke();
        }
    }

    private void MoveX(float amount)
    {
        xRemainder += amount;
        int move = Mathf.RoundToInt(xRemainder);
        if (move != 0)
        {
            xRemainder -= move;
            int sign = Math.Sign(move);
            while(move != 0)
            {
                if (!OverlapTile(wallTile, transform.position + new Vector3(sign, 0, 0) / ppu))
                {
                    transform.Translate(new Vector3(sign, 0, 0) / ppu);
                    move -= sign;
                }
                else
                {
                    break;
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
                    velocity.y = 0.0f;
                    break;
                }
                else if(sign == -1 && OverlapTile(bridgeTile, transform.position + new Vector3(0, sign, 0) / ppu) && !OverlapTile(bridgeTile, transform.position))
                {
                    velocity.y = 0.0f;
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
            if (tiles[i] == tile) return true;
        }
        return false;
    }

    private Vector3 GetEnterCoord()
    {
        if (tilemap == null) return Vector3.zero;

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if(tile == enterTile) return tilemap.CellToWorld(new Vector3Int(x,y,0));
                
            }
        }
        return Vector3.zero;
    }
}
