using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Main navigation controller.
/// Fetches route from Spring Boot API, uses GPS for positioning,
/// creates AR markers with labels, draws route lines with arrows.
/// </summary>
public class NavigationManager : MonoBehaviour
{
    // --- Inspector fields ---
    [Header("Prefab")]
    [Tooltip("Assign the NavigationMarker prefab here")]
    public GameObject markerPrefab;

    [Header("API Settings")]
    [Tooltip("Spring Boot API base URL")]
    public string apiBaseUrl = "http://10.0.2.105:8080/api/navigation";

    [Tooltip("Start node ID")]
    public int startNodeId = 1;

    [Tooltip("End node ID")]
    public int endNodeId = 5;

    [Header("Route Visualization")]
    [Tooltip("Color of the route line")]
    public Color routeLineColor = new Color(0f, 0.8f, 1f, 0.8f);

    [Tooltip("Width of the route line")]
    public float routeLineWidth = 0.05f;

    [Tooltip("Color of direction arrows")]
    public Color arrowColor = new Color(1f, 0.9f, 0.2f, 0.9f);

    // --- Private fields ---
    private GPSLocationService gpsService;
    private GPSCoordinateConverter gpsConverter;
    private NavigationUI navigationUI;
    private LineRenderer routeLine;

    private List<GameObject> spawnedMarkers = new List<GameObject>();
    private List<GameObject> spawnedArrows = new List<GameObject>();
    private bool markersSpawned = false;

    // Fallback reference coordinates (Coimbatore area)
    // Used only if GPS is unavailable (e.g., in editor testing)
    private double fallbackLatitude = 11.0168;
    private double fallbackLongitude = 76.9557;

    void Start()
    {
        Debug.Log("[Nav] NavigationManager Started");

        // --- Validate marker prefab ---
        if (markerPrefab == null)
        {
            Debug.LogError("[Nav] ERROR: markerPrefab is not assigned! " +
                "Please assign NavigationMarker prefab in Inspector.");
            return;
        }

        // --- Setup GPS service ---
        gpsService = gameObject.AddComponent<GPSLocationService>();

        // --- Setup UI ---
        navigationUI = gameObject.AddComponent<NavigationUI>();

        // --- Setup route line ---
        SetupRouteLine();

        // --- Start the navigation flow ---
        StartCoroutine(NavigationFlow());
    }

    void SetupRouteLine()
    {
        routeLine = gameObject.AddComponent<LineRenderer>();
        routeLine.startWidth = routeLineWidth;
        routeLine.endWidth = routeLineWidth;
        routeLine.positionCount = 0;
        routeLine.useWorldSpace = true;

        // Create a simple unlit material for the line
        routeLine.material = new Material(
            Shader.Find("Sprites/Default"));
        routeLine.startColor = routeLineColor;
        routeLine.endColor = routeLineColor;
    }

    /// <summary>
    /// Main navigation flow:
    /// 1. Start GPS
    /// 2. Fetch route from API
    /// 3. Place markers and draw route
    /// </summary>
    IEnumerator NavigationFlow()
    {
        // --- Step 1: Start GPS ---
        UpdateUI("Starting GPS...", "Initializing...", 0, "Starting...");

        yield return StartCoroutine(gpsService.StartGPS());

        if (gpsService.IsRunning)
        {
            Debug.Log("[Nav] GPS ready: Lat=" + gpsService.Latitude
                      + " Lon=" + gpsService.Longitude);

            gpsConverter = new GPSCoordinateConverter(
                gpsService.Latitude, gpsService.Longitude);

            UpdateUI("—", "Active (" + gpsService.Accuracy + "m)",
                     0, "Fetching route...");
        }
        else
        {
            Debug.LogWarning("[Nav] GPS not available. Using fallback " +
                "reference coordinates for testing.");

            gpsConverter = new GPSCoordinateConverter(
                fallbackLatitude, fallbackLongitude);

            UpdateUI("—", gpsService.StatusMessage,
                     0, "GPS fallback mode");
        }

        // --- Step 2: Fetch route from API ---
        yield return StartCoroutine(GetPath());
    }

    /// <summary>
    /// Fetch navigation path from Spring Boot backend.
    /// </summary>
    IEnumerator GetPath()
    {
        // Build the URL
        string url = apiBaseUrl + "?start=" + startNodeId + "&end=" + endNodeId;

        Debug.Log("[Nav] Sending request to: " + url);
        UpdateUI("—", navigationUI.gpsStatus, 0, "Sending request...");

        UnityWebRequest request = UnityWebRequest.Get(url);

        // Set timeout
        request.timeout = 15;

        yield return request.SendWebRequest();

        Debug.Log("[Nav] Request finished. Result: " + request.result);

        // --- Handle errors ---
        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorMsg = "API Error: " + request.error;
            Debug.LogError("[Nav] " + errorMsg);
            UpdateUI("—", navigationUI.gpsStatus, 0, errorMsg);
            yield break;
        }

        // --- Parse JSON ---
        string json = request.downloadHandler.text;
        Debug.Log("[Nav] JSON received: " + json);

        if (string.IsNullOrEmpty(json) || json == "[]")
        {
            Debug.LogWarning("[Nav] Empty node list received from API.");
            UpdateUI("—", navigationUI.gpsStatus, 0, "No route found");
            yield break;
        }

        // Wrap JSON array for Unity's JsonUtility
        string wrappedJson = "{\"nodes\":" + json + "}";

        NodeList nodeList;
        try
        {
            nodeList = JsonUtility.FromJson<NodeList>(wrappedJson);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Nav] JSON parse error: " + e.Message);
            UpdateUI("—", navigationUI.gpsStatus, 0, "Invalid JSON");
            yield break;
        }

        if (nodeList == null || nodeList.nodes == null
            || nodeList.nodes.Length == 0)
        {
            Debug.LogWarning("[Nav] No nodes in parsed data.");
            UpdateUI("—", navigationUI.gpsStatus, 0, "Empty route");
            yield break;
        }

        Debug.Log("[Nav] Parsed " + nodeList.nodes.Length + " nodes.");

        // --- Step 3: Place markers and draw route ---
        PlaceMarkers(nodeList.nodes);
    }

    /// <summary>
    /// Place AR markers at GPS-converted positions and draw route line.
    /// </summary>
    void PlaceMarkers(NodeData[] nodes)
    {
        // Prevent duplicate markers
        if (markersSpawned)
        {
            Debug.Log("[Nav] Markers already spawned. Clearing old markers.");
            ClearMarkers();
        }

        // Get the last node name as destination
        string destinationName = nodes[nodes.Length - 1].nodeName;

        List<Vector3> routePositions = new List<Vector3>();

        for (int i = 0; i < nodes.Length; i++)
        {
            NodeData node = nodes[i];

            // Convert GPS to Unity position
            Vector3 position = gpsConverter.GPSToUnityPosition(
                node.latitude, node.longitude, node.floorId);

            Debug.Log("[Nav] Node " + (i + 1) + "/" + nodes.Length
                      + ": " + node.nodeName
                      + " → Position: " + position
                      + " (Lat:" + node.latitude
                      + " Lon:" + node.longitude
                      + " Floor:" + node.floorId + ")");

            // Instantiate marker
            GameObject marker = Instantiate(
                markerPrefab, position, Quaternion.identity);

            marker.name = "Marker_" + node.nodeName;

            // Store node data and initialize appearance
            NavigationMarker markerScript =
                marker.GetComponent<NavigationMarker>();

            // If the prefab doesn't have NavigationMarker, add it now
            if (markerScript == null)
            {
                markerScript = marker.AddComponent<NavigationMarker>();
                Debug.Log("[Nav] Added NavigationMarker script to marker at runtime.");
            }

            markerScript.nodeId = node.nodeId;
            markerScript.nodeName = node.nodeName;
            markerScript.floorId = node.floorId;
            markerScript.latitude = node.latitude;
            markerScript.longitude = node.longitude;

            // Determine marker type: start, waypoint, or destination
            if (i == 0)
                markerScript.markerType = 0; // Start
            else if (i == nodes.Length - 1)
                markerScript.markerType = 2; // Destination
            else
                markerScript.markerType = 1; // Waypoint

            // Build the label and apply colors
            markerScript.Initialize();

            spawnedMarkers.Add(marker);
            routePositions.Add(position);
        }

        // --- Draw route line ---
        DrawRouteLine(routePositions);

        // --- Place direction arrows between consecutive nodes ---
        PlaceDirectionArrows(routePositions);

        markersSpawned = true;

        // Update UI
        UpdateUI(
            destinationName,
            gpsService.IsRunning
                ? "Active (" + gpsService.Accuracy + "m)"
                : gpsService.StatusMessage,
            nodes.Length,
            "Route ready"
        );

        Debug.Log("[Nav] ✓ Navigation ready! " + nodes.Length
                  + " markers placed. Destination: " + destinationName);
    }

    /// <summary>
    /// Draw a line connecting all route nodes in order.
    /// </summary>
    void DrawRouteLine(List<Vector3> positions)
    {
        if (routeLine == null || positions.Count < 2)
            return;

        routeLine.positionCount = positions.Count;
        routeLine.SetPositions(positions.ToArray());

        Debug.Log("[Nav] Route line drawn with "
                  + positions.Count + " points.");
    }

    /// <summary>
    /// Place small arrow indicators between consecutive route nodes
    /// showing the direction of travel.
    /// </summary>
    void PlaceDirectionArrows(List<Vector3> positions)
    {
        if (positions.Count < 2) return;

        for (int i = 0; i < positions.Count - 1; i++)
        {
            Vector3 from = positions[i];
            Vector3 to = positions[i + 1];
            Vector3 midpoint = (from + to) / 2f;
            Vector3 direction = (to - from).normalized;
            float distance = Vector3.Distance(from, to);

            // Skip if nodes are too close together
            if (distance < 0.5f) continue;

            // Create arrow at midpoint
            GameObject arrow = CreateArrowIndicator(
                midpoint, direction, i + 1);

            spawnedArrows.Add(arrow);

            // For longer segments, add additional arrows
            if (distance > 5f)
            {
                Vector3 quarterPoint = Vector3.Lerp(from, to, 0.25f);
                Vector3 threeQuarterPoint = Vector3.Lerp(from, to, 0.75f);

                spawnedArrows.Add(
                    CreateArrowIndicator(quarterPoint, direction, i + 1));
                spawnedArrows.Add(
                    CreateArrowIndicator(threeQuarterPoint, direction, i + 1));
            }
        }

        Debug.Log("[Nav] Placed " + spawnedArrows.Count + " direction arrows.");
    }

    /// <summary>
    /// Create a single arrow indicator (elongated cube pointing in direction).
    /// </summary>
    GameObject CreateArrowIndicator(Vector3 position, Vector3 direction, int index)
    {
        // Use a stretched cube as a simple arrow/chevron
        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.name = "Arrow_" + index;
        arrow.transform.position = position;
        arrow.transform.localScale = new Vector3(0.08f, 0.02f, 0.15f);

        // Point the arrow in the direction of travel
        if (direction != Vector3.zero)
        {
            arrow.transform.rotation = Quaternion.LookRotation(direction);
        }

        // Apply arrow color
        Renderer rend = arrow.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = arrowColor;
            rend.material = mat;
        }

        // Remove collider — arrows are visual only
        Collider col = arrow.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        return arrow;
    }

    /// <summary>
    /// Remove all previously spawned markers, arrows,
    /// and clear the route line.
    /// </summary>
    void ClearMarkers()
    {
        foreach (GameObject marker in spawnedMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        spawnedMarkers.Clear();

        foreach (GameObject arrow in spawnedArrows)
        {
            if (arrow != null)
                Destroy(arrow);
        }
        spawnedArrows.Clear();

        if (routeLine != null)
            routeLine.positionCount = 0;

        markersSpawned = false;
        Debug.Log("[Nav] Cleared all markers, arrows, and route line.");
    }

    /// <summary>
    /// Update the navigation UI display.
    /// </summary>
    void UpdateUI(string destination, string gps,
                  int nodes, string status)
    {
        if (navigationUI == null) return;
        navigationUI.destinationName = destination;
        navigationUI.gpsStatus = gps;
        navigationUI.totalNodes = nodes;
        navigationUI.navigationStatus = status;
    }
}

[System.Serializable]
public class NodeList
{
    public NodeData[] nodes;
}