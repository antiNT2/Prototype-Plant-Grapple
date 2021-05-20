using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class HiddenTilemap : MonoBehaviour
{
    Tilemap triggerTilemap;
    Vector3Int firstTileHiddenPosition;

    private void Start()
    {
        triggerTilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Vector3 playerPos = collision.transform.position;
            Vector3Int collidedHideableTileTilemapPosition = triggerTilemap.WorldToCell(playerPos);

            if (triggerTilemap.layoutGrid.cellSize.y < 1f) //dirty bugfix :/
                HideTile(collidedHideableTileTilemapPosition + Vector3Int.up);
            HideTile(collidedHideableTileTilemapPosition + Vector3Int.down);
            HideTile(collidedHideableTileTilemapPosition);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        print("jaaj");
        if (collision.tag == "Player")
        {
            if (firstTileHiddenPosition != Vector3Int.zero)
            {
                RevealTile(firstTileHiddenPosition + Vector3Int.up);
                firstTileHiddenPosition = Vector3Int.zero;
            }
        }
    }

    void HideTile(Vector3Int position)
    {
        if (triggerTilemap.HasTile(position) && triggerTilemap.GetTileFlags(position) != TileFlags.None)
        {
            triggerTilemap.SetTileFlags(position, TileFlags.None);
            DOTween.ToAlpha(() => triggerTilemap.GetColor(position), x => triggerTilemap.SetColor(position, x), 0f, 0.4f).SetEase(Ease.InQuad);

            if (firstTileHiddenPosition == Vector3Int.zero)
                firstTileHiddenPosition = position;

            HideTile(position + Vector3Int.left);
            HideTile(position + Vector3Int.right);
            HideTile(position + Vector3Int.up);
            HideTile(position + Vector3Int.down);
        }
    }

    void RevealTile(Vector3Int position)
    {
        if (triggerTilemap.HasTile(position) && triggerTilemap.GetTileFlags(position) == TileFlags.None)
        {
            triggerTilemap.SetTileFlags(position, TileFlags.LockTransform);
            DOTween.ToAlpha(() => triggerTilemap.GetColor(position), x => triggerTilemap.SetColor(position, x), 1f, 0.4f).SetEase(Ease.Linear);

            RevealTile(position + Vector3Int.left);
            RevealTile(position + Vector3Int.right);
            RevealTile(position + Vector3Int.up);
            RevealTile(position + Vector3Int.down);
        }
    }
}
