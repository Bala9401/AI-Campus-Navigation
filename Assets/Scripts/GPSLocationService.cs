using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Android GPS location services for the AR Navigation app.
/// Requests permissions, starts GPS, and provides current lat/lon.
/// Attach this to any GameObject (e.g., Navigation Manager).
/// </summary>
public class GPSLocationService : MonoBehaviour
{
    // --- Public read-only GPS data ---
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public float Accuracy { get; private set; }
    public bool IsRunning { get; private set; }
    public string StatusMessage { get; private set; } = "Initializing...";

    // --- Settings ---
    [Tooltip("Desired accuracy in meters (lower = more precise, more battery)")]
    public float desiredAccuracy = 5f;

    [Tooltip("Minimum distance (meters) before updating position")]
    public float updateDistance = 2f;

    [Tooltip("Max seconds to wait for GPS to start")]
    public int maxWaitSeconds = 20;

    /// <summary>
    /// Call this to start GPS. Returns when GPS is ready or failed.
    /// </summary>
    public IEnumerator StartGPS()
    {
        Debug.Log("[GPS] Starting GPS Location Service...");

        // --- Step 1: Check if location service is supported ---
        if (!Input.location.isEnabledByUser)
        {
            StatusMessage = "GPS not enabled by user";
            Debug.LogWarning("[GPS] Location services not enabled by user. " +
                "Please enable GPS in device settings.");
            // On some Android devices this returns false even when GPS
            // is available, so we continue anyway
        }

        // --- Step 2: Request Android permission ---
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.FineLocation))
        {
            Debug.Log("[GPS] Requesting Fine Location permission...");
            UnityEngine.Android.Permission.RequestUserPermission(
                UnityEngine.Android.Permission.FineLocation);

            // Wait a moment for the permission dialog
            yield return new WaitForSeconds(2f);

            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                    UnityEngine.Android.Permission.FineLocation))
            {
                StatusMessage = "Location permission denied";
                Debug.LogError("[GPS] Fine Location permission denied!");
                yield break;
            }

            Debug.Log("[GPS] Fine Location permission granted.");
        }
#endif

        // --- Step 3: Start the location service ---
        Input.location.Start(desiredAccuracy, updateDistance);
        StatusMessage = "Starting GPS...";
        Debug.Log("[GPS] Waiting for GPS fix...");

        // --- Step 4: Wait for initialization ---
        int waited = 0;
        while (Input.location.status == LocationServiceStatus.Initializing
               && waited < maxWaitSeconds)
        {
            yield return new WaitForSeconds(1f);
            waited++;
            Debug.Log("[GPS] Waiting... (" + waited + "s)");
        }

        // --- Step 5: Check result ---
        if (waited >= maxWaitSeconds)
        {
            StatusMessage = "GPS timed out";
            Debug.LogError("[GPS] Timed out after " + maxWaitSeconds + " seconds.");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            StatusMessage = "GPS failed";
            Debug.LogError("[GPS] Location service failed to start.");
            yield break;
        }

        // --- Step 6: GPS is running ---
        IsRunning = true;
        UpdatePosition();

        StatusMessage = "GPS Active";
        Debug.Log("[GPS] GPS Active! Lat: " + Latitude + " Lon: " + Longitude
                  + " Accuracy: " + Accuracy + "m");
    }

    void Update()
    {
        if (IsRunning && Input.location.status == LocationServiceStatus.Running)
        {
            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        LocationInfo loc = Input.location.lastData;
        Latitude = loc.latitude;
        Longitude = loc.longitude;
        Accuracy = loc.horizontalAccuracy;
    }

    void OnDisable()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            Input.location.Stop();
            Debug.Log("[GPS] Location service stopped.");
        }
    }
}
