using UnityEngine;

/// <summary>
/// Simple AR navigation HUD displayed using OnGUI.
/// Shows destination, GPS status, route info, and current step.
/// No Canvas required — uses Unity's immediate mode GUI.
/// </summary>
public class NavigationUI : MonoBehaviour
{
    // These are set by NavigationManager
    [HideInInspector] public string destinationName = "—";
    [HideInInspector] public string gpsStatus = "Initializing...";
    [HideInInspector] public int totalNodes = 0;
    [HideInInspector] public string navigationStatus = "Loading...";

    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;
    private GUIStyle statusStyle;
    private bool stylesInitialized = false;

    void InitStyles()
    {
        if (stylesInitialized) return;

        // Semi-transparent dark background
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2,
            new Color(0.05f, 0.05f, 0.12f, 0.85f));
        boxStyle.padding = new RectOffset(20, 20, 14, 14);

        // White body text
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = ScaledFont(24);
        labelStyle.normal.textColor = Color.white;
        labelStyle.wordWrap = true;

        // Cyan header
        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = ScaledFont(30);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.3f, 0.85f, 1f);

        // Status line — color changes based on state
        statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = ScaledFont(22);
        statusStyle.fontStyle = FontStyle.Bold;
        statusStyle.wordWrap = true;

        stylesInitialized = true;
    }

    /// <summary>
    /// Scale font sizes based on screen DPI for consistent mobile display.
    /// </summary>
    int ScaledFont(int baseSize)
    {
        float dpiScale = Screen.dpi > 0 ? Screen.dpi / 160f : 1f;
        return Mathf.Max(baseSize, (int)(baseSize * dpiScale * 0.6f));
    }

    void OnGUI()
    {
        InitStyles();

        float panelWidth = Screen.width * 0.92f;
        float lineH = ScaledFont(24) + 16;
        float panelHeight = lineH * 5 + 30;
        float x = (Screen.width - panelWidth) / 2f;
        float y = Screen.height - panelHeight - 20;

        // --- Background panel ---
        GUI.Box(new Rect(x, y, panelWidth, panelHeight), "", boxStyle);

        float innerX = x + 22;
        float innerY = y + 12;

        // --- Header ---
        GUI.Label(new Rect(innerX, innerY, panelWidth - 44, lineH),
            "🧭  AR Campus Navigation", headerStyle);
        innerY += lineH + 2;

        // --- Destination ---
        GUI.Label(new Rect(innerX, innerY, panelWidth - 44, lineH),
            "📍 Destination:  " + destinationName, labelStyle);
        innerY += lineH;

        // --- GPS Status ---
        string gpsIcon = gpsStatus.Contains("Active") ? "🛰️" : "⚠️";
        GUI.Label(new Rect(innerX, innerY, panelWidth - 44, lineH),
            gpsIcon + " GPS:  " + gpsStatus, labelStyle);
        innerY += lineH;

        // --- Route info ---
        GUI.Label(new Rect(innerX, innerY, panelWidth - 44, lineH),
            "🗺️ Route:  " + totalNodes + " Nodes", labelStyle);
        innerY += lineH;

        // --- Status with dynamic color ---
        Color statusColor;
        if (navigationStatus.Contains("ready"))
            statusColor = new Color(0.3f, 1f, 0.4f); // Green
        else if (navigationStatus.Contains("Error") || navigationStatus.Contains("failed"))
            statusColor = new Color(1f, 0.4f, 0.3f); // Red
        else
            statusColor = new Color(1f, 0.9f, 0.3f); // Yellow

        statusStyle.normal.textColor = statusColor;
        GUI.Label(new Rect(innerX, innerY, panelWidth - 44, lineH),
            "⚡ Status:  " + navigationStatus, statusStyle);
    }

    /// <summary>
    /// Helper to create a solid color texture for GUI backgrounds.
    /// </summary>
    Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
