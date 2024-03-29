using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class PixelCharacter : MonoBehaviour
{
    private Tilemap levelTilemap;
    private Tilemap thornTilemap;

    [SerializeField] private TileTypes tileTypes;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float walkSpeed, gravity, jumpForce;
    [SerializeField] private Vector2 bulletSpawnPoint;
    [SerializeField] private Bullet myBullet;
    [SerializeField] private BoundsInt hitBox;

    private Vector2 velocity;
    private float xRemainder, yRemainder;
    private bool onFloor;
    private bool canDoubleJump;
    private float facing;

    private Sprite sprite;

    private const float ppu = 12.0f;

    public event Action exitLevel;
    public event Action died;

    private Animator animator;
    private const string idleAnimation = "CharacterIdle";
    private const string runAnimation = "CharacterRun";
    private const string jumpAnimation = "CharacterJump";
    private const string fallAnimation = "CharacterFall";
    private string state;

    private void Awake()
    {
        facing = 1.0f;
        spriteRenderer = GetComponent<SpriteRenderer>();
        sprite = spriteRenderer.sprite;
        animator = GetComponent<Animator>();
        state = idleAnimation;
    }
    void Start()
    {
        
        
    }

    private void OnEnable()
    {
        xRemainder = 0.0f;
        yRemainder = 0.0f;
        velocity = Vector2.zero;

        levelTilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        thornTilemap = GameObject.Find("ThornTilemap").GetComponent<Tilemap>();
        transform.position = GetEnterCoord();

        animator.CrossFade(idleAnimation, 0);
    }

    private void ChangeState(string animation)
    {
        if(state != animation)
        {
            animator.CrossFade(animation, 0);
            state = animation;
        }
    }

    void Update()
    {
        onFloor = false;
        if (OverlapTile(tileTypes.floorTiles, transform.position + Vector3.down / ppu))
        {
            onFloor = true;
            canDoubleJump = true;
        }
        
        velocity.x = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            velocity.x = -walkSpeed;
            facing = -1.0f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            velocity.x = walkSpeed;
            facing = 1.0f;
        }

        spriteRenderer.flipX = facing == -1.0f ? true : false;

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

        if (Input.GetKeyUp(KeyCode.UpArrow) && velocity.y > 0.0f)
        {
            velocity.y /= 2;
        }

        if(Input.GetKeyDown(KeyCode.DownArrow) && OverlapTile(tileTypes.exitTile, transform.position))
        {
            exitLevel?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            float flipX = spriteRenderer.flipX ? -1.0f : 1.0f;
            Vector3 spawnPoint = Vector3.zero;
            spawnPoint.x = Mathf.Round(bulletSpawnPoint.x * ppu) / ppu * flipX;
            spawnPoint.y = Mathf.Round(bulletSpawnPoint.y * ppu) / ppu;
            
            Bullet newBullet = Instantiate(myBullet, transform.position + spawnPoint, Quaternion.identity);
            newBullet.Velocity = new Vector2(facing, 0.0f);
            //myBullet.SetDirection(facing);
        }

        velocity.y += gravity;

        if (onFloor)
        {
            if (velocity.x == 0.0f) ChangeState(idleAnimation);
            else ChangeState(runAnimation);
        }
        else
        {
            if (velocity.y < 0.0f) ChangeState(fallAnimation); else ChangeState(jumpAnimation);
        }

        MoveX(velocity.x);
        MoveY(velocity.y);

        

        Bounds bounds = new Bounds(transform.position + (Vector3)hitBox.position / ppu, (Vector3)hitBox.size / ppu);
        if (BoundsOverlapTile(thornTilemap, tileTypes.thornTile, bounds))
        {
            died?.Invoke();
        }
    }
    
    private bool BoundsOverlapTile(Tilemap tilemap, TileBase tile, Bounds b)
    {
        if (tilemap == null) return false;

        Vector3Int coord1 = tilemap.WorldToCell(new Vector3(b.min.x, b.min.y, 0));
        Vector3Int coord2 = tilemap.WorldToCell(new Vector3(b.max.x, b.max.y, 0));

        BoundsInt area = new BoundsInt(coord1, coord2 - coord1 + Vector3Int.one);
        TileBase[] tiles = new TileBase[area.size.x * area.size.y];
        tilemap.GetTilesBlockNonAlloc(area, tiles);
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == tile) return true;
        }
        return false;
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
                if (!OverlapTile(tileTypes.solidTiles, transform.position + new Vector3(sign, 0, 0) / ppu))
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
                if (OverlapTile(tileTypes.solidTiles, transform.position + new Vector3(0, sign, 0) / ppu))
                {
                    velocity.y = 0.0f;
                    break;
                }
                else if(sign == -1 && OverlapTile(tileTypes.bridgeTile, transform.position + new Vector3(0, sign, 0) / ppu) && !OverlapTile(tileTypes.bridgeTile, transform.position))
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
        if (levelTilemap == null) return false;

        Vector3Int coord1 = levelTilemap.WorldToCell(position - new Vector3((sprite.rect.width / 2 ) / ppu, 0, 0));
        Vector3Int coord2 = levelTilemap.WorldToCell(position + new Vector3((sprite.rect.width / 2 - 1) / ppu, (sprite.rect.height - 1) / ppu, 0));
        BoundsInt area = new BoundsInt(coord1, coord2 - coord1 + Vector3Int.one);
        TileBase[] tiles = new TileBase[area.size.x * area.size.y];
        levelTilemap.GetTilesBlockNonAlloc(area, tiles);
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == tile) return true;
        }
        return false;
    }

    private bool OverlapTile(List<TileBase> tileList, Vector3 position)
    {
        if (levelTilemap == null) return false;

        Vector3Int coord1 = levelTilemap.WorldToCell(position - new Vector3((sprite.rect.width / 2) / ppu, 0, 0));
        Vector3Int coord2 = levelTilemap.WorldToCell(position + new Vector3((sprite.rect.width / 2 - 1) / ppu, (sprite.rect.height - 1) / ppu, 0));
        BoundsInt area = new BoundsInt(coord1, coord2 - coord1 + Vector3Int.one);
        TileBase[] tiles = new TileBase[area.size.x * area.size.y];
        levelTilemap.GetTilesBlockNonAlloc(area, tiles);
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tileList.Contains(tiles[i])) return true;
        }
        return false;
    }

    private Vector3 GetEnterCoord()
    {
        if (levelTilemap == null) return Vector3.zero;

        BoundsInt bounds = levelTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                TileBase tile = levelTilemap.GetTile(new Vector3Int(x, y, 0));
                if(tile == tileTypes.enterTile) return levelTilemap.CellToWorld(new Vector3Int(x,y,0));
                
            }
        }
        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        // Bullet spawn point
        float flip = spriteRenderer.flipX ? -1.0f : 1.0f;
        //float flip = 1;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + bulletSpawnPoint.x * flip, transform.position.y + bulletSpawnPoint.y, 0), 0.25f);

        // Hit box
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position + (Vector3)hitBox.position / ppu, (Vector3)hitBox.size / ppu);
    }
}
