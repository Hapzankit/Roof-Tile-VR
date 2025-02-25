using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingMaterial : MonoBehaviour
{
    // Start is called before the first frame update
    public float blinkInterval = 0.5f; // Time in seconds between blinks
    private Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        InvokeRepeating("ToggleVisibility", blinkInterval, blinkInterval);
    }

    void ToggleVisibility()
    {
        renderer.enabled = !renderer.enabled;
    }

    
}
