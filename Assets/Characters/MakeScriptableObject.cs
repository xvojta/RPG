using UnityEngine;
using System.Collections;
using UnityEditor;
using BLINK.Controller;

public class MakeScriptableObject
{
    [MenuItem("Assets/Create/My Scriptable Object")]
    public static void CreateMyAsset()
    {
        FPSInputController asset = ScriptableObject.CreateInstance<FPSInputController>();

        AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}