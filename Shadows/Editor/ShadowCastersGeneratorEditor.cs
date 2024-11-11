using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(ShadowCaster2DTileMapComposite))]
public class ShadowCastersGeneratorEditor : Editor
{
    readonly GUIContent _sortingLayersLabel = new("Target Sorting Layers", "Apply Shadows to the specified sorting layers.");

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ShadowCaster2DTileMapComposite generator = (ShadowCaster2DTileMapComposite)target;
        EditorGUILayout.Space();

        // Cria o dropdown
        generator.selectedSortingLayers = EditorGUILayout.MaskField(_sortingLayersLabel, generator.selectedSortingLayers, GetSortingLayers());

        // Atualiza a lista de Sorting Layers e seus IDs
        generator.sortingLayerIDs = new int[GetSelectedSortingLayerIDs(generator.selectedSortingLayers).Count];

        int count = 0;
        foreach (int layerID in GetSelectedSortingLayerIDs(generator.selectedSortingLayers))
        {
            generator.sortingLayerIDs[count] = layerID;
            count++;
        }

        // Verifica se houve alguma alteração e atualiza
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
            }
        }

        return selectedLayerIDs;
    }

}
