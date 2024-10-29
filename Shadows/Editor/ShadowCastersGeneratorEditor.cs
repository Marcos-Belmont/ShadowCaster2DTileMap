using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(ShadowCaster2DTileMap))]
public class ShadowCastersGeneratorEditor : Editor
{
    const string EDITOR_SHADOW_CASTER = "EditorShadowCaster";
    internal static int selectedLayerMask = 0;
    ShadowCaster2DTileMap generator;
    readonly GUIContent sortingLayersLabel = new("Target Sorting Layers", "Apply Shadows to the specified sorting layers.");

    private void OnEnable()
    {
        selectedLayerMask = PlayerPrefs.HasKey(EDITOR_SHADOW_CASTER) ? PlayerPrefs.GetInt(EDITOR_SHADOW_CASTER) : 0;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        generator = (ShadowCaster2DTileMap)target;
        EditorGUILayout.Space();

        // Cria o dropdown
        selectedLayerMask = EditorGUILayout.MaskField(sortingLayersLabel, selectedLayerMask, GetSortingLayers());

        generator.isNull = true;

        // Atualiza a lista de Sorting Layers e seus IDs
        generator.sortingLayerIDs = new int[GetSelectedSortingLayerIDs(selectedLayerMask).Count];

        int count = 0;
        foreach (int layerID in GetSelectedSortingLayerIDs(selectedLayerMask))
        {
            generator.sortingLayerIDs[count] = layerID;
            generator.countZeros = layerID == 0 && generator.countZeros < 2 ? (byte)(generator.countZeros+1) : generator.countZeros;
            count++;
        }

        // Verifica se houve alguma alteração
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.Space();

        #region BOTOES DE GERACAO
        if (GUILayout.Button("Generate"))
        {

            generator.Generate();

        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Destroy All Shadows"))
        {

            generator.DestroyAllShadows();

        }
        #endregion

        PlayerPrefs.SetInt("EditorShadowCaster", selectedLayerMask);
    }

    private string[] GetSortingLayers()
    {
        return SortingLayer.layers.Select(layer => layer.name).ToArray();
    }

    private List<int> GetSelectedSortingLayerIDs(int layerMask)
    {
        List<int> selectedLayerIDs = new List<int>();
        SortingLayer[] layers = SortingLayer.layers;

        for (int i = 0; i < layers.Length; i++)
        {
            if ((layerMask & (1 << i)) != 0)
            {
                selectedLayerIDs.Add(layers[i].id);
                generator.isNull = false;
            }
        }

        return selectedLayerIDs;
    }

}