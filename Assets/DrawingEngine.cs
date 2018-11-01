using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingEngine : MonoBehaviour {


    public GameObject drawingObject;

    public List<LineRenderer> renderers;
    public int drawingIndex = -1;

	
    public void initializeNewLine(Vector3 position)
    {
        GameObject newDrawingRenderer = Instantiate(drawingObject);
        newDrawingRenderer.transform.parent = transform;

        LineRenderer renderer = newDrawingRenderer.GetComponent<LineRenderer>();
        renderer.startWidth = 0.01f;
        renderer.endWidth = 0.01f;
        renderer.positionCount = 0;

        renderers.Add(renderer);
        drawingIndex++;
    }

    public void DrawPoint(Vector3 position)
    {
        renderers[drawingIndex].positionCount++;

        renderers[drawingIndex].SetPosition(renderers[drawingIndex].positionCount -1, position);
    }
   
}
