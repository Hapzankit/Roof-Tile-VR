using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureTag : MonoBehaviour
{
    public Material redmaterial;


    public void ChangeMaterial()
    {
        Renderer renderer = GetComponent<Renderer>(); // Get the Renderer component
        if (renderer != null)
        {
            renderer.material = redmaterial; // Apply the new material
        }
        else
        {
            Debug.LogError("Renderer not found on the GameObject");
        }
    }
    // Start is called before the first frame update

}
