using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Extended OVERRIDE Rule Tile", menuName = "2D/Tiles/Extended OVERRIDE Rule Tile")]
//[CustomEditor(typeof(RuleOverrideTile), true)]
public class ExtendedOverrideTile : AdvancedRuleOverrideTile
{
    public string type;
    /*public override bool RuleMatch(int neighbor, TileBase other)
    {
        if (other is RuleOverrideTile)
            other = (other as RuleOverrideTile).m_InstanceTile;

        ExtendedRuleTile otherTile = other as ExtendedRuleTile;

        if (otherTile == null)
            return base.RuleMatch(neighbor, other);

        switch (neighbor)
        {
            case TilingRule.Neighbor.This: return type == otherTile.type;
            case TilingRule.Neighbor.NotThis: return type != otherTile.type;
        }
        return true;

    }*/
}
