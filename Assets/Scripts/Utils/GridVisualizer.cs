using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float pointSpawnDelay = 0.15f; // Speed of animation
    [SerializeField] private float pointScale = 3.0f;
    [SerializeField] private Color pointColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color lineColor = Color.yellow;
    [SerializeField] private Material pointMaterial; // Assign a simple URP/Standard Lit material

    private LineRenderer pathLine;

    // Call this instead of your loop
    public void VisualizePath(List<Vector2> searchGrid)
    {
        // 1. Setup the Line Renderer
        if (pathLine == null)
        {
            pathLine = gameObject.AddComponent<LineRenderer>();
            pathLine.startWidth = 0.5f;
            pathLine.endWidth = 0.5f;
            pathLine.material = new Material(Shader.Find("Sprites/Default")); // Simple shader
            pathLine.startColor = lineColor;
            pathLine.endColor = lineColor;
        }

        pathLine.positionCount = 0;

        // 2. Start the animation
        StartCoroutine(AnimateGridGeneration(searchGrid));
    }

    private IEnumerator AnimateGridGeneration(List<Vector2> gridPoints)
    {
        int index = 0;

        foreach (Vector2 pnt in gridPoints)
        {
            Vector3 pos = new Vector3(pnt.x, 38, pnt.y);

            // A. Create the visual node (Sphere looks cleaner than Cube)
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node.transform.position = pos;
            node.transform.localScale = Vector3.zero; // Start invisible for pop effect

            // Clean up the node
            Destroy(node.GetComponent<Collider>()); // Optimization
            var renderer = node.GetComponent<Renderer>();
            if (pointMaterial != null) renderer.material = pointMaterial;
            renderer.material.color = pointColor;

            // B. Update the Line Renderer
            pathLine.positionCount = index + 1;
            pathLine.SetPosition(index, pos);

            // C. "Pop" Animation (Scale up elasticity)
            StartCoroutine(PopAnimation(node.transform));

            index++;

            // Wait before showing the next one
            yield return new WaitForSeconds(pointSpawnDelay);
        }
    }

    private IEnumerator PopAnimation(Transform target)
    {
        float timer = 0;
        float duration = 0.3f;
        Vector3 finalScale = new Vector3(pointScale, pointScale, pointScale);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            // Elastic ease-out curve
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            target.localScale = Vector3.Lerp(Vector3.zero, finalScale, scale);
            yield return null;
        }
        target.localScale = finalScale;
    }
}