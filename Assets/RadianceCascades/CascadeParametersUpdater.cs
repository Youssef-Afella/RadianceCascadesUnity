using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CascadeParametersUpdater : MonoBehaviour
{
    public RadianceCascadesManager radianceCascadesManager;
    public DrawingSDFGenerator drawingSDFGenerator;

    public Text cascadeCountText;
    public Text cascadeHeightText;

    public void UpdateBurshColor(Button button) {
        Color color = button.transform.GetChild(0).GetComponent<Image>().color;
        drawingSDFGenerator.brushColor = color;
    }

    public void UpdateBrushSize(float size) {
        drawingSDFGenerator.brushSize = size;
    }

    public void UpdateCascadeCount(float input) {
        radianceCascadesManager.cascadeCount = (int) Mathf.Round(input * 8);
        cascadeCountText.text = "Cascade Count : "+ (int)Mathf.Round(input * 8);
    }

    public void UpdateCascadeHeight(float input)
    {
        radianceCascadesManager.cascadesHeight = 256 + (int)Mathf.Round(input * 8) * 128;
        cascadeHeightText.text = "Cascade Height : " + ((int)Mathf.Round(input * 8) * 128 + 256);
    }

    public void ClearDrawing() {
        drawingSDFGenerator.ClearDrawing();
    }
}
