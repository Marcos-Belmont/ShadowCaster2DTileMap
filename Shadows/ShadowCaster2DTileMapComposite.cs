using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
[RequireComponent(typeof(CompositeShadowCaster2D))]
[RequireComponent(typeof(CompositeCollider2D))]
public class ShadowCaster2DTileMapComposite : MonoBehaviour
{
    #region PARAMETROS INSPECTOR
    [Space]
    [Tooltip("Specifies how to draw the shadow used with the ShadowCaster2D.")]
    public CastingOption castingOption;
    [HideInInspector] public enum CastingOption
    {
        SelfShadow,
        CastShadow,
        CastAndSelfShadow,
        NoShadow
    }

    //Aplica a logica de sorting layers.
    [HideInInspector] public int selectedSortingLayers = 0;
    [HideInInspector] public int[] sortingLayerIDs;

    [Space]
    [Tooltip("Used to define the reduction of the shadow detection format.")]
    [Range(0f, 0.5f)]
    public float shapeReduction = 0;
    #endregion

    private CompositeCollider2D _compositeCollider;
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;

    static readonly FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo shapePathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo sortingLayersField = typeof(ShadowCaster2D).GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance);

    public void Generate()
    {
        DestroyAllShadows();

        #region ELEMENTOS TILEMAP
        _compositeCollider = GetComponent<CompositeCollider2D>();
        tilemap = GetComponent <Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        #endregion

        for (int i = 0; i < _compositeCollider.pathCount; i++)
        {
            Vector2[] pathVertices = new Vector2[_compositeCollider.GetPathPointCount(i)];
            _compositeCollider.GetPath(i, pathVertices);
            GameObject shadowCaster = new GameObject("shadow_caster_" + i);
            shadowCaster.transform.parent = gameObject.transform;
            ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();

            #region APLICANDO CASTING OPTION
            switch (castingOption)
            {
                case CastingOption.SelfShadow:
                    shadowCasterComponent.selfShadows = true;
                    shadowCasterComponent.castsShadows = false;
                    break;
                case CastingOption.CastShadow:
                    shadowCasterComponent.selfShadows = false;
                    shadowCasterComponent.castsShadows = true;
                    break;
                case CastingOption.CastAndSelfShadow:
                    shadowCasterComponent.selfShadows = true;
                    shadowCasterComponent.castsShadows = true;
                    break;
                default:
                    shadowCasterComponent.selfShadows = false;
                    shadowCasterComponent.castsShadows = false;
                    break;
            }
            #endregion

            #region REPOSICIONANDO AS SOMBRAS
            Vector3[] testPath = new Vector3[pathVertices.Length];
            for (ushort j = 0; j < pathVertices.Length; j++)
            {
                float distance = Vector2.Distance(pathVertices[j], pathVertices[j < pathVertices.Length - 1 ? j + 1 : 0]) +
                    Vector2.Distance(pathVertices[j], pathVertices[j > 0 ? j - 1 : pathVertices.Length - 1]);

                if (Vector2.Distance(new Vector2(pathVertices[j].x + shapeReduction, pathVertices[j].y + shapeReduction), pathVertices[j < pathVertices.Length - 1 ? j + 1 : 0]) +
                    Vector2.Distance(new Vector2(pathVertices[j].x + shapeReduction, pathVertices[j].y + shapeReduction), pathVertices[j > 0 ? j - 1 : pathVertices.Length - 1]) < distance)
                {
                    testPath[j] = new Vector3(pathVertices[j].x + shapeReduction, pathVertices[j].y + shapeReduction);

                    if (!(tilemap.GetTile(tilemap.WorldToCell(testPath[j])) != null && tilemap.GetComponent<TilemapRenderer>().sortingLayerID == tilemapRenderer.sortingLayerID))
                        testPath[j] = new Vector3(pathVertices[j].x - shapeReduction, pathVertices[j].y - shapeReduction);
                }
                else if (Vector2.Distance(new Vector2(pathVertices[j].x + shapeReduction, pathVertices[j].y - shapeReduction), pathVertices[j < pathVertices.Length - 1 ? j + 1 : 0]) +
                    Vector2.Distance(new Vector2(pathVertices[j].x + shapeReduction, pathVertices[j].y - shapeReduction), pathVertices[j > 0 ? j - 1 : pathVertices.Length - 1]) < distance)
                {
                    testPath[j] = new Vector3(pathVertices[j].x + shapeReduction, pathVertices[j].y - shapeReduction);

                    if (!(tilemap.GetTile(tilemap.WorldToCell(testPath[j])) != null && tilemap.GetComponent<TilemapRenderer>().sortingLayerID == tilemapRenderer.sortingLayerID))
                        testPath[j] = new Vector3(pathVertices[j].x - shapeReduction, pathVertices[j].y + shapeReduction);
                }
                else if (Vector2.Distance(new Vector2(pathVertices[j].x - shapeReduction, pathVertices[j].y + shapeReduction), pathVertices[j < pathVertices.Length - 1 ? j + 1 : 0]) +
                    Vector2.Distance(new Vector2(pathVertices[j].x - shapeReduction, pathVertices[j].y + shapeReduction), pathVertices[j > 0 ? j - 1 : pathVertices.Length - 1]) < distance)
                {
                    testPath[j] = new Vector3(pathVertices[j].x - shapeReduction, pathVertices[j].y + shapeReduction);

                    if (!(tilemap.GetTile(tilemap.WorldToCell(testPath[j])) != null && tilemap.GetComponent<TilemapRenderer>().sortingLayerID == tilemapRenderer.sortingLayerID))
                        testPath[j] = new Vector3(pathVertices[j].x + shapeReduction, pathVertices[j].y - shapeReduction);
                }
                else // Tudo subtraindo
                {
                    testPath[j] = new Vector3(pathVertices[j].x - shapeReduction, pathVertices[j].y - shapeReduction);

                    if (!(tilemap.GetTile(tilemap.WorldToCell(testPath[j])) != null && tilemap.GetComponent<TilemapRenderer>().sortingLayerID == tilemapRenderer.sortingLayerID))
                        testPath[j] = new Vector3(pathVertices[j].x + shapeReduction, pathVertices[j].y + shapeReduction);
                }
            }
            #endregion

            #region APLICANDO CONFIGURACOES
            shapePathField.SetValue(shadowCasterComponent, testPath);
            shapePathHashField.SetValue(shadowCasterComponent, Random.Range(int.MinValue, int.MaxValue));
            sortingLayersField.SetValue(shadowCasterComponent, selectedSortingLayers == 0 ? null : sortingLayerIDs);
            #endregion
        }

    }
    public void DestroyAllShadows()
    {

        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            DestroyImmediate(child.gameObject);
        }

    }
}
