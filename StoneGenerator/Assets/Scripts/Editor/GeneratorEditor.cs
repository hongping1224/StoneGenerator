using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Generator))]
public class GeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Generator t = (Generator)target;
        if (GUILayout.Button("NewSeed"))
        {
            t.RandomSeed();
            t.Regenerate(t.seed);
        }
        if (GUILayout.Button("Refresh"))
        {
            t.Regenerate(t.seed);
        }
    }

}
