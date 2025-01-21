using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalArrow : MonoBehaviour
{
    public float amplitude = 1.0f; // Maximum distance moved
    public float frequency = 1.0f; // Speed of oscillation

    private Vector3 startPosition;

    void Start()
    {
        // Store the starting position of the GameObject
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // Calculate the new x position using a sine wave
        float newX = startPosition.x + Mathf.Sin(Time.time * frequency) * amplitude;

        // Update the local position of the GameObject
        transform.localPosition = new Vector3(newX, startPosition.y, startPosition.z);
    }
}
