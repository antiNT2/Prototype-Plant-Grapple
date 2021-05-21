using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CollectiblesTilemap : MonoBehaviour
{
    Tilemap triggerTilemap;

    private void Start()
    {
        triggerTilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Vector3 playerPos = collision.transform.position;
            Vector3Int collidedCollectibleTilemapPosition = triggerTilemap.WorldToCell(playerPos);

            if (triggerTilemap.layoutGrid.cellSize.y < 1f) //dirty bugfix :/
                CollectCollectible(collidedCollectibleTilemapPosition + Vector3Int.up);
            CollectCollectible(collidedCollectibleTilemapPosition + Vector3Int.down);
            CollectCollectible(collidedCollectibleTilemapPosition);
        }
    }

    void CollectCollectible(Vector3Int position)
    {
        if (triggerTilemap.HasTile(position))
        {
            CollectibleTile tileToCollect = GetCollectibleTile(triggerTilemap.GetTile(position));
            triggerTilemap.SetTile(position, null);

            if (tileToCollect != null)
            {
                tileToCollect.DoCollectEffect(triggerTilemap.CellToWorld(position));
            }
        }
    }

    public CollectibleTile GetCollectibleTile(TileBase _tile)
    {
        foreach (var item in CustomFunctions.instance.allCollectibleTiles)
        {
            if (_tile == item.tile)
                return item;
        }

        return null;
    }
}

[System.Serializable]
public class CollectibleTile
{
    public TileBase tile;
    public int coinGetAmount;
    public GameObject collectibleGetParticlesPrefab;
    public AudioClip collectibleGetSound;

    public void DoCollectEffect(Vector3 position)
    {
        CoinsManager.instance.AddCoins(this.coinGetAmount);
        CustomFunctions.SpawnParticleEffects(position, this.collectibleGetParticlesPrefab);
        if (this.collectibleGetSound != null)
            CustomFunctions.PlaySound(this.collectibleGetSound);
    }
}
