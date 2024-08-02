using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingSDFGenerator : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public ComputeShader sdfCompute;

    public RawImage target;
    public Color brushColor = Color.red;
    public float brushSize = 0.01f;

    private RenderTexture sdfTexture;

    void Start()
    {
        sdfTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        sdfTexture.enableRandomWrite = true;
        sdfTexture.Create();

        RenderTexture.active = sdfTexture;
        GL.Clear(true, true, new Color(0, 0, 0, 1));
        RenderTexture.active = null;

        sdfCompute.SetVector("Resolution", new Vector2(Screen.width, Screen.height));
        sdfCompute.SetTexture(0, "Result", sdfTexture);

        target.texture = sdfTexture;

        //Assign the texture to the RadianceCascadeManager
        RadianceCascadesManager.sdfTexture = sdfTexture;
    }

    Vector2 oldPos;

    public void OnPointerDown(PointerEventData eventData)
    {

        Vector2 resolution = new Vector2(Screen.width, Screen.height);
        Vector2 position = eventData.position / Mathf.Max(resolution.x, resolution.y);

        oldPos = position;

        DrawBrush(position, position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 resolution = new Vector2(Screen.width, Screen.height);
        Vector2 position = eventData.position / Mathf.Max(resolution.x, resolution.y);

        DrawBrush(oldPos, position);

        oldPos = position;
    }

    private void DrawBrush(Vector2 startPos, Vector2 endPos) {
        sdfCompute.SetFloat("brushSize", brushSize);
        sdfCompute.SetVector("brushColor", brushColor);

        sdfCompute.SetVector("startPos", startPos);
        sdfCompute.SetVector("endPos", endPos);

        sdfCompute.Dispatch(0, Screen.width / 8, Screen.height / 8, 1);
    }

    public void ClearDrawing() {
        RenderTexture.active = sdfTexture;
        GL.Clear(true, true, new Color(0, 0, 0, 1));
        RenderTexture.active = null;
    }

}
