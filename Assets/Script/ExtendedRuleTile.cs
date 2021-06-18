using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Extended Rule Tile", menuName = "2D/Tiles/Extended Rule Tile")]
public class ExtendedRuleTile : RuleTile
{

    public string type;
    public override bool RuleMatch(int neighbor, TileBase other)
    {
        string detectedType = "";

        if (other is RuleOverrideTile)
            other = (other as RuleOverrideTile).m_InstanceTile;

        if (other is ExtendedOverrideTile)
            detectedType = (other as ExtendedOverrideTile).type;

        ExtendedRuleTile otherTile = other as ExtendedRuleTile;

        if (otherTile == null)
            return base.RuleMatch(neighbor, other);

        if (detectedType == "")
            detectedType = otherTile.type;

        switch (neighbor)
        {
            case TilingRule.Neighbor.This: return type == detectedType;
            case TilingRule.Neighbor.NotThis: return type != detectedType;
        }
        return true;

    }
}