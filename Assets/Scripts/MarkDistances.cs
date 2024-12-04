using UnityEngine;

public class MarkDistances : MonoBehaviour
{
    public GameObject markerPrefab; // Assign a prefab for the markers (e.g., a small sphere)
    public Vector3 direction = Vector3.forward; // Direction in which the marks will be placed
    public float[] distancesInInches = { 8f, 9f, 10f }; // Distances in inches

    void Start()
    {
        if (markerPrefab == null)
        {
            Debug.LogError("Please assign a marker prefab.");
            return;
        }

        foreach (float distance in distancesInInches)
        {
            float distanceInMeters = distance * 0.0254f; // Convert inches to meters
            Vector3 position = transform.position + direction.normalized * distanceInMeters;

            // Instantiate the marker at the calculated position
            Instantiate(markerPrefab, position, Quaternion.identity);
        }
    }
    
}
