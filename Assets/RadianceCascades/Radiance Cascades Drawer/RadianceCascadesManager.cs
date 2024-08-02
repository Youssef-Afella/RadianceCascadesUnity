using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadianceCascadesManager : MonoBehaviour
{
    public RawImage target;
    public Material radianceCascadesMaterial;

    [Header("Parameters")]
    public int cascadesHeight = 1024;
    public int cascadeCount = 6;

    [Header("Compute Shaders")]
    public ComputeShader cascadeCompute;
    public ComputeShader cascadeMerger;

    [HideInInspector] public static RenderTexture sdfTexture;//Original SDF with the full resolution computed in the DrawingSDFGenerator script

    private RenderTexture sdfTextureResized;
    private RenderTexture[] cascadeTextures;//The array where we gonna hold the computed cascades, also we use the same array to merge them
    //We end up having all the merged cascades in cascade0

    private Vector2Int cascadeResolution;

    void Start()
    {
        cascadeTextures = new RenderTexture[0];
    }

    void Update()
    {
        if (sdfTexture == null)
            return;

        UpdateResolutionIfNeeded();
        UpdateCascadeArrayIfNeeded();
        Graphics.Blit(sdfTexture, sdfTextureResized);//copy the full resolution sdfTexture to the resized one

        //Computing cascades
        for (int i = 0; i < cascadeTextures.Length; i++)
        {
            cascadeCompute.SetInt("Level", i);
            cascadeCompute.SetTexture(0, "Result", cascadeTextures[i]);
            cascadeCompute.Dispatch(0, cascadeResolution.x / 8, cascadeResolution.y / 8, 1);
        }

        //Merging the cascades
        //it starts from the upper cascade to the lower one
        //we store the result in the lower cascade so the final merged texture is the cascade0
        for (int i = cascadeTextures.Length - 2; i >= 0; i--)
        {
            cascadeMerger.SetInt("lowerLevel", i);
            cascadeMerger.SetTexture(0, "upperCascade", cascadeTextures[i + 1]);
            cascadeMerger.SetTexture(0, "lowerCascade", cascadeTextures[i]);
            cascadeMerger.Dispatch(0, cascadeResolution.x / 8, cascadeResolution.y / 8, 1);
        }
    }

    int oldCascadeCount = -1;
    private void UpdateCascadeArrayIfNeeded() {
        if (cascadeCount == oldCascadeCount)
            return;

        //Release all the old RenderTextures from memory
        for (int i = 0; i < cascadeTextures.Length; i++)
        {
            cascadeTextures[i].Release();
        }

        //Reinitialing new RenderTextures
        cascadeTextures = new RenderTexture[cascadeCount];

        for (int i = 0; i < cascadeCount; i++)
        {
            cascadeTextures[i] = new RenderTexture(cascadeResolution.x, cascadeResolution.y, 0, RenderTextureFormat.ARGBFloat);
            cascadeTextures[i].enableRandomWrite = true;
            cascadeTextures[i].Create();
        }

        //Assign the cascade0 to the RawImage
        if (cascadeCount > 0)
        {
            target.texture = cascadeTextures[0];
        }

        oldCascadeCount = cascadeCount;
    }

    int oldHeight = 0;

    private void UpdateResolutionIfNeeded()
    {
        if (cascadesHeight == oldHeight)
            return;

        float aspect = (float)Screen.width / Screen.height;
        cascadeResolution = new Vector2Int((int)(cascadesHeight * aspect), cascadesHeight);

        radianceCascadesMaterial.SetVector("_Resolution", (Vector2)cascadeResolution);//Assign the Resolution to the Radiance Field shader

        //Release all the old RenderTextures from memory if exist
        for (int i = 0; i < cascadeTextures.Length; i++)
        {
            cascadeTextures[i].Release();
        }
        if (sdfTextureResized != null) {
            sdfTextureResized.Release();
        }

        //Reinitializing the RenderTextures with the new resolution
        sdfTextureResized = new RenderTexture(cascadeResolution.x, cascadeResolution.y, 0, RenderTextureFormat.ARGBFloat);
        sdfTextureResized.enableRandomWrite = true;
        sdfTextureResized.Create();

        cascadeTextures = new RenderTexture[cascadeCount];

        for (int i = 0; i < cascadeCount; i++)
        {
            cascadeTextures[i] = new RenderTexture(cascadeResolution.x, cascadeResolution.y, 0, RenderTextureFormat.ARGBFloat);
            cascadeTextures[i].enableRandomWrite = true;
            cascadeTextures[i].Create();
        }

        //Updating the values in the Compute Shaders
        cascadeCompute.SetTexture(0, "SDFTexture", sdfTextureResized);
        cascadeCompute.SetVector("Resolution", (Vector2)cascadeResolution);
        cascadeMerger.SetVector("Resolution", (Vector2)cascadeResolution);

        //Assign the cascade0 to the RawImage
        if (cascadeCount > 0)
        {
            target.texture = cascadeTextures[0];
        }

        oldHeight = cascadesHeight;
        oldCascadeCount = cascadeCount;
    }
}
