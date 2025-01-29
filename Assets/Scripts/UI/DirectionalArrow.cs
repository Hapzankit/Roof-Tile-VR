using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalArrow : MonoBehaviour
{

    public enum ArrowDirection
    {
        X, Y, Z
    }
    public float amplitude = 1.0f; // Maximum distance moved
    public float frequency = 1.0f; // Speed of oscillation

    private Vector3 startPosition;
    public ArrowDirection arrowDirection;



    void Start()
    {
        // Store the starting position of the GameObject
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // Calculate the new x position using a sine wave

        switch (arrowDirection)
        {
            case ArrowDirection.X:
                float newX = startPosition.x + Mathf.Sin(Time.time * frequency) * amplitude;
                transform.localPosition = new Vector3(newX, startPosition.y, startPosition.z);
                break;
            case ArrowDirection.Y:
                float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
                transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
                break;
            case ArrowDirection.Z:
                float newZ = startPosition.z + Mathf.Sin(Time.time * frequency) * amplitude;
                transform.localPosition = new Vector3(startPosition.x, startPosition.y, newZ);
                break;

        }
        // Update the local position of the GameObject
    }
}
