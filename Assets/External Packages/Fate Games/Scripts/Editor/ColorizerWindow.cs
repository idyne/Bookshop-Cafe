using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ColorizerWindow : EditorWindow
{
    private Color color;

    [MenuItem("Window/Fate")]
    public static void ShowWindow()
    {
        GetWindow<ColorizerWindow>("Colorizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Color the selected objects!", EditorStyles.boldLabel);

        color = EditorGUILayout.ColorField("Color", color);

        if (GUILayout.Button("Colorize"))
        {
            foreach(GameObject obj in Selection.gameObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if(renderer != null)
                {
                    Material material = renderer.material;
                    material.color = color;
                    renderer.material = material;
                }
            }
        }
    }
}
