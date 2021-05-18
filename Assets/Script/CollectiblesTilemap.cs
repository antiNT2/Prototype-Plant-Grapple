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
                CoinsManager.instance.AddCoins(tileToCollect.coinGetAmount);
                SpawnParticleEffects(position, tileToCollect.collectibleGetParticlesPrefab);
                if (tileToCollect.collectibleGetSound != null)
                    CustomFunctions.PlaySound(tileToCollect.collectibleGetSound);
            }
        }
    }

    void SpawnParticleEffects(Vector3Int position, GameObject particlePrefab)
    {
        if (particlePrefab == null)
            return;

        GameObject spawnedEffect = Instantiate(particlePrefab);
        spawnedEffect.transform.position = triggerTilemap.CellToWorld(position);
        Destroy(spawnedEffect, 0.4f);
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
}
