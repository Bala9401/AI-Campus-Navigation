using UnityEngine;

/// <summary>
/// Converts GPS latitude/longitude to Unity local X/Z coordinates
/// using the user's current GPS position as the origin.
/// Uses local ENU (East-North-Up) approximation suitable for campus scale.
/// </summary>
public class GPSCoordinateConverter
{
    // The GPS position treated as Unity world origin (0,0,0)
    private double originLatitude;
    private double originLongitude;

    // Pre-calculated conversion factors
    private double metersPerDegreeLat;
    private double metersPerDegreeLon;

    /// <summary>
    /// Initialize the converter with the user's current GPS position.
    /// This position becomes Unity's (0, 0, 0).
    /// </summary>
    public GPSCoordinateConverter(double userLatitude, double userLongitude)
    {
        originLatitude = userLatitude;
        originLongitude = userLongitude;

        // 1 degree of latitude ≈ 111,320 meters (constant everywhere)
        metersPerDegreeLat = 111320.0;

        // 1 degree of longitude varies by latitude
        // At equator ≈ 111,320m, shrinks toward poles
        metersPerDegreeLon = 111320.0 *
            Mathf.Cos((float)originLatitude * Mathf.Deg2Rad);

        Debug.Log("[GPSConverter] Origin set: Lat=" + originLatitude
                  + " Lon=" + originLongitude);
        Debug.Log("[GPSConverter] Meters/DegLat=" + metersPerDegreeLat
                  + " Meters/DegLon=" + metersPerDegreeLon);
    }

    /// <summary>
    /// Convert a GPS coordinate to Unity local position.
    /// X = East/West (longitude difference)
    /// Y = Height based on floor
    /// Z = North/South (latitude difference)
    /// </summary>
    public Vector3 GPSToUnityPosition(
        double latitude,
        double longitude,
        int floorId)
    {
        // East-West distance (X axis)
        double x = (longitude - originLongitude) * metersPerDegreeLon;

        // North-South distance (Z axis)
        double z = (latitude - originLatitude) * metersPerDegreeLat;

        // Height: each floor is ~3 meters, ground floor at 0.2m
        float y = (floorId - 1) * 3.0f + 0.2f;

        return new Vector3((float)x, y, (float)z);
    }

    /// <summary>
    /// Get the distance in meters between two GPS points.
    /// Useful for proximity checks.
    /// </summary>
    public float DistanceBetween(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        double dx = (lon2 - lon1) * metersPerDegreeLon;
        double dz = (lat2 - lat1) * metersPerDegreeLat;
        return (float)System.Math.Sqrt(dx * dx + dz * dz);
    }
}
