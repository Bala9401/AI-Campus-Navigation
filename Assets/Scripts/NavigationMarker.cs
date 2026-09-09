using UnityEngine;
using TMPro;

/// <summary>
/// Attached to each navigation marker prefab instance.
/// Stores node data, creates a floating text label, and
/// differentiates start/waypoint/end markers by color.
/// </summary>
public class NavigationMarker : MonoBehaviour
{
    // --- Node information (set by NavigationManager) ---
    [HideInInspector] public int nodeId;
    [HideInInspector] public string nodeName;
    [HideInInspector] public int floorId;
    [HideInInspector] public double latitude;
    [HideInInspector] public double longitude;

    // --- Marker type (set by NavigationManager) ---
    // 0 = start, 1 = waypoint, 2 = destination
    [HideInInspector] public int markerType = 1;

    // The floating label child object
    private GameObject labelObject;

    // Colors for different marker types
    private static readonly Color startColor = new Color(0.2f, 0.9f, 0.3f);       // Green
    private static readonly Color waypointColor = new Color(0.3f, 0.7f, 1.0f);     // Blue
    private static readonly Color destinationColor = new Color(1.0f, 0.3f, 0.3f);  // Red

    /// <summary>
    /// Call this after setting nodeName and markerType to build the label.
    /// We build it via code so no prefab changes are needed.
    /// </summary>
    public void Initialize()
    {
        // --- Set marker color based on type ---
        Color markerColor = waypointColor;
        string typeTag = "●";

        switch (markerType)
        {
            case 0:
                markerColor = startColor;
                typeTag = "▶ START";
                break;
            case 1:
                markerColor = waypointColor;
                typeTag = "●";
                break;
            case 2:
                markerColor = destinationColor;
                typeTag = "★ END";
                break;
        }

        // Apply color to the sphere renderer
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Create a new material instance to avoid shared material issues
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = markerColor;

            // Make it slightly emissive so it's visible in AR
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", markerColor * 0.5f);

            rend.material = mat;
        }

        // --- Create floating text label above the marker ---
        CreateLabel(markerColor, typeTag);

        Debug.Log("[Marker] Initialized: " + nodeName
                  + " (Type: " + markerType + ")");
    }

    void CreateLabel(Color color, string typeTag)
    {
        // Create a child GameObject for the label
        labelObject = new GameObject("Label_" + nodeName);
        labelObject.transform.SetParent(transform);

        // Position the label above the sphere
        // The sphere has scale 0.2, so radius ~0.1 in world space
        labelObject.transform.localPosition = new Vector3(0, 1.5f, 0);
        labelObject.transform.localScale = new Vector3(5f, 5f, 5f);

        // Add TextMeshPro component for 3D world-space text
        TextMeshPro tmp = labelObject.AddComponent<TextMeshPro>();
        tmp.text = nodeName + "\n<size=60%>" + typeTag + "</size>";
        tmp.fontSize = 3;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        // Make the text render on top of everything
        tmp.sortingOrder = 100;

        // Auto-size the rect transform
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(4f, 2f);
        }
    }

    void Update()
    {
        // Make the marker and label always face the camera (billboard)
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Billboard the label toward the camera
            if (labelObject != null)
            {
                labelObject.transform.LookAt(cam.transform);
                labelObject.transform.Rotate(0, 180, 0);
            }
        }
    }
}