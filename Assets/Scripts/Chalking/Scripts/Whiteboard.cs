using System.Collections.Generic;
using RoofTileVR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048); // Resolution of the texture
    public List<BoxCollider> LineTraceCaller;
    public float initialMarkDistanceInches = 21.5f;
    public float subsequentMarkDistanceInches = 10f;
    float widthInInches;
    float lengthInInches;


    public TileCasting tileCasting;

    public int numberOfLinesOftileTobeMade = 0;


    public TMP_Text exposureMeasurements;
    public GameObject MeasuretextL;
    public GameObject MeasuretextR;

    public SelectableButton exposureButton;

    void Start()
    {
        tileCasting = FindObjectOfType<TileCasting>();
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

    }

    public void DrawMarks()
    {
        subsequentMarkDistanceInches = /*float.Parse(exposureDropdown.options[exposureDropdown.value].text)*/ exposureButton.exposureSelected;
        // Calculate the width in texture space
        float pixelsPerInch = textureSize.x / widthInInches;

        // Start drawing marks
        int n = 0;
        float currentWidthInches = initialMarkDistanceInches;
        while (currentWidthInches <= widthInInches)
        {
            int x = Mathf.RoundToInt(currentWidthInches * pixelsPerInch);

            // Draw a vertical line or mark
            DrawVerticalLine(x);
            SnapTheText(n);
            tileCasting.exposureConvertedToUnityfromInches.Add(exposureButton.exposureSelected);

            // Move to the next mark position
            currentWidthInches += subsequentMarkDistanceInches;
            n++;
        }
        print("number of lines" + n);
        numberOfLinesOftileTobeMade = n;
        // Apply texture changes
        texture.Apply();
        print("Draw Marks");
        tileCasting.BoardPosition = tileCasting.boardPositionsToSnap[0];
    }


    void SnapTheText(int num)
    {
        //from =text
        // Get the current world position of the child object
        TMP_Text instantiatedText = Instantiate(exposureMeasurements);

        Vector3 childWorldPosition = instantiatedText.transform.position;

        // Get the target position from the side edge
        Vector3 targetPosition = MeasuretextL.transform.position;

        // Calculate the new world position for the child in the local positive Y direction of the objectToCheck
        Vector3 localYDirection = MeasuretextL.transform.right; // This gets the local 'up' direction which corresponds to the local +Y axis

        // Combine the Y-direction offset with the original directional offset
        Vector3 combinedDirection = (localYDirection.normalized * num) + new Vector3(0.5f, 0, 0);
        Vector3 newChildWorldPosition = targetPosition + combinedDirection * exposureButton.exposureSelected * 0.0254f;

        // Calculate the required offset for the parent
        Vector3 offset = newChildWorldPosition - childWorldPosition;
        if (num == 0)
        {

            instantiatedText.text = 10 + " inches";
        }
        else
        {
            instantiatedText.text = exposureButton.exposureSelected + " inches";
        }
        // Apply the offset to the parent to snap it
        instantiatedText.transform.position += offset;
        SnapThePositions(num);
    }

    void SnapThePositions(int num)
    {
        Transform pos = Instantiate(tileCasting.BoardPosition);

        Vector3 childWorldPosition = pos.transform.position;

        // Get the target position from the side edge
        Vector3 targetPosition = MeasuretextL.transform.position;

        // Calculate the new world position for the child in the local positive Y direction of the objectToCheck
        Vector3 localYDirection = MeasuretextL.transform.right; // This gets the local 'up' direction which corresponds to the local +Y axis

        // Combine the Y-direction offset with the original directional offset
        Vector3 combinedDirection = (localYDirection.normalized * num) + new Vector3(0.5f, 0, 0);
        Vector3 newChildWorldPosition = targetPosition + combinedDirection * exposureButton.exposureSelected * 0.0254f;

        // Calculate the required offset for the parent
        Vector3 offset = newChildWorldPosition - childWorldPosition;
        // Apply the offset to the parent to snap it
        pos.transform.position += offset;
        pos.SetParent(this.transform);
        pos.transform.localPosition = new Vector3(pos.transform.localPosition.x, num * 0.005f + 0.0355f, pos.transform.localPosition.z);
        tileCasting.boardPositionsToSnap.Add(pos);

    }

    public void DrawVerticalLine(int x)
    {
        // x=x * textureSize.x / widthInInches
        int lineThickness = 5; // Thickness of the mark in pixels
        float pixelsPerInch = textureSize.x / widthInInches;
        Color markColor = Color.black; // Color of the mark

        for (int thickness = -lineThickness / 2; thickness < lineThickness / 2; thickness++)
        {
            int drawX = Mathf.Clamp(x + thickness, 0, (int)textureSize.x - 1);
            for (int y = 0; y < textureSize.y; y++)
            {
                texture.SetPixel(drawX, y, markColor);
            }
        }

        // float.TryParse(exposureDropdown.options[exposureDropdown.value].text, out float result);

    }
    public void DrawVerticalAtDistance(float dist)
    {
        float pixelsPerInch = textureSize.x / widthInInches;
        print(pixelsPerInch + "PIXEL PER INCH");
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
        tileCasting.GetDistanceAccordingToExposure(subsequentMarkDistanceInches);


    }









}
