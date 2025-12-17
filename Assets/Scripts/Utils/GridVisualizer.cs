using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour {
    [Header("Visual Settings - Grid")]
    [SerializeField] private float pointSpawnDelay = 0.15f;
    [SerializeField] private float pointScale = 3.0f;
    [SerializeField] private Color pointColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color gridLineColor = Color.yellow;
    [SerializeField] private Material pointMaterial;

    [Header("Visual Settings - Target Ray")]
    [SerializeField] private Color rayColor = Color.red;
    [SerializeField] private float rayDuration = 1.0f; // How long the shot takes
    [SerializeField] private float rayWidth = 0.2f;

    private LineRenderer gridLine;   // Renamed from pathLine for clarity
    private LineRenderer targetLine; // New specific line for the ray

    // ---------------------- GRID LOGIC (Existing) ----------------------
    public void VisualizePath(List<Vector2> searchGrid) {
        // Use helper to create or get the line
        if (gridLine == null)
            gridLine = CreateLine("GridLineRenderer", gridLineColor, 0.5f);

        gridLine.positionCount = 0;
        StartCoroutine(AnimateGridGeneration(searchGrid));
    }

    // ---------------------- NEW TARGET RAY LOGIC ----------------------
    public void VisualizeTargetRay(Vector3 targetPosition) =>
        VisualizeTargetRay(targetPosition, Vector3.zero);

    public void VisualizeTargetRay(Vector3 targetPosition, Vector3 offset) {
        // 1. Setup the dedicated Target Line
        if (targetLine == null)
            targetLine = CreateLine("TargetLineRenderer", rayColor, rayWidth);

        targetLine.positionCount = 2;
        targetLine.enabled = true;

        // 2. Start the shooting animation
        StopCoroutine("AnimateRay"); // Stop overlap if called twice
        StartCoroutine(AnimateRay(targetPosition, offset));
    }

    private IEnumerator AnimateRay(Vector3 targetPos, Vector3 offset) {
        float timer = 0;

        while (timer < rayDuration) {
            timer += Time.deltaTime;
            float t = timer / rayDuration;

            // Ease out for a satisfying "Ziiip" effect
            // t = Mathf.Sin(t * Mathf.PI * 0.5f); 

            // 1. Keep the start attached to US (the moving drone)
            targetLine.SetPosition(0, transform.position + offset);

            // 2. Interpolate the tip of the laser from US to the TARGET
            Vector3 currentTipPos = Vector3.Lerp(transform.position + offset, targetPos, t);
            targetLine.SetPosition(1, currentTipPos);

            yield return null;
        }

        // Ensure it ends perfectly at the target
        targetLine.SetPosition(0, transform.position + offset);
        targetLine.SetPosition(1, targetPos);

        // Optional: If you want the line to disappear after reaching, uncomment below:
        // yield return new WaitForSeconds(0.5f);
        // targetLine.enabled = false; 
    }

    // ---------------------- HELPERS & ANIMATIONS ----------------------

    // Helper to keep code clean and reusable
    private LineRenderer CreateLine(string name, Color col, float width) {
        // Check if child object already exists to avoid duplicates
        Transform child = transform.Find(name);
        LineRenderer lr;

        if (child == null) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform);
            lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }
        else {
            lr = child.GetComponent<LineRenderer>();
        }

        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = col;
        lr.endColor = col;
        return lr;
    }

    private IEnumerator AnimateGridGeneration(List<Vector2> gridPoints) {
        int index = 0;
        foreach (Vector2 pnt in gridPoints) {
            Vector3 pos = new Vector3(pnt.x, 38, pnt.y);

            // Create Visual Node
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node.transform.position = pos;
            node.transform.localScale = Vector3.zero;

            Destroy(node.GetComponent<Collider>());
            var renderer = node.GetComponent<Renderer>();
            if (pointMaterial != null) renderer.material = pointMaterial;
            renderer.material.color = pointColor;

            // Update Line
            gridLine.positionCount = index + 1;
            gridLine.SetPosition(index, pos);

            // Animation
            StartCoroutine(PopAnimation(node.transform));
            index++;
            yield return new WaitForSeconds(pointSpawnDelay);
        }
    }

    private IEnumerator PopAnimation(Transform target) {
        float timer = 0;
        float duration = 0.3f;
        Vector3 finalScale = new Vector3(pointScale, pointScale, pointScale);

        while (timer < duration) {
            timer += Time.deltaTime;
            float t = timer / duration;
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            target.localScale = Vector3.Lerp(Vector3.zero, finalScale, scale);
            yield return null;
        }
        target.localScale = finalScale;
    }
}