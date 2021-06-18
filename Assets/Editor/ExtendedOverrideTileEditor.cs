using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(ExtendedOverrideTile))]
public class ExtendedOverrideTileEditor : AdvancedRuleOverrideTileEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
