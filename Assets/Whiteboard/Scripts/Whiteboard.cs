using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048); // Resolution of the texture
    public List<BoxCollider> LineTraceCaller;
    public float initialMarkDistanceInches = 11.5f;
    public float subsequentMarkDistanceInches = 10f;
    float widthInInches;
    float lengthInInches;

    public TMP_Dropdown exposureDropdown;

    void Start()
    {
        // Initialize texture
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        r.material.mainTexture = texture;

        if (this.transform == null)
        {
            Debug.LogError("Plane Transform is not assigned!");
            return;
        }

        // Get local scale of the plane
        Vector3 scale = this.transform.localScale;

        // Unity's default plane is 10x10 units, scaled by its local scale
        float planeWidthUnits = 10 * scale.x; // Width in Unity units
        float planeLengthUnits = 10 * scale.z; // Length in Unity units

        // Convert to inches (1 unit = 1 meter, 1 meter = 39.37 inches)
        widthInInches = planeWidthUnits * 39.37f;
        lengthInInches = planeLengthUnits * 39.37f;

        Debug.Log($"Plane Dimensions in Inches: Width = {widthInInches:F2} inches, Length = {lengthInInches:F2} inches");

        // Draw marks
        // DrawMarks();
    }

    public void DrawMarks()
    {
        subsequentMarkDistanceInches = float.Parse(exposureDropdown.options[exposureDropdown.value].text);
        // Calculate the width in texture space
        float pixelsPerInch = textureSize.x / widthInInches;

        // Start drawing marks
        float currentWidthInches = initialMarkDistanceInches;
        while (currentWidthInches <= widthInInches)
        {
            int x = Mathf.RoundToInt(currentWidthInches * pixelsPerInch);

            // Draw a vertical line or mark
            DrawVerticalLine(x);

            // Move to the next mark position
            currentWidthInches += subsequentMarkDistanceInches;
        }

        // Apply texture changes
        texture.Apply();
    }

    public void DrawVerticalLine(int x)
    {
        // x=x * textureSize.x / widthInInches
        int lineThickness = 5; // Thickness of the mark in pixels
        Color markColor = Color.black; // Color of the mark

        for (int thickness = -lineThickness / 2; thickness < lineThickness / 2; thickness++)
        {
            int drawX = Mathf.Clamp(x + thickness, 0, (int)textureSize.x - 1);
            for (int y = 0; y < textureSize.y; y++)
            {
                texture.SetPixel(drawX, y, markColor);
            }
        }
    }
    public void DrawVerticalAtDistance(float dist)
    {
        float pixelsPerInch = textureSize.x / widthInInches;
        int x = Mathf.RoundToInt(dist * pixelsPerInch);

        // x=x * textureSize.x / widthInInches
        int lineThickness = 5; // Thickness of the mark in pixels
        Color markColor = Color.black; // Color of the mark

        for (int thickness = -lineThickness / 2; thickness < lineThickness / 2; thickness++)
        {
            int drawX = Mathf.Clamp(x + thickness, 0, (int)textureSize.x - 1);
            for (int y = 0; y < textureSize.y; y++)
            {
                texture.SetPixel(drawX, y, markColor);
            }
        }
        texture.Apply();
    }

    public bool isFirstLineMade()
    {
        int i = 0;
        foreach (var caller in LineTraceCaller)
        {
            if (caller.GetComponent<MeshRenderer>().enabled)
            {
                print(i + " Number of callers");
                i++;
            }
        }

        return i == LineTraceCaller.Count;
    }
}