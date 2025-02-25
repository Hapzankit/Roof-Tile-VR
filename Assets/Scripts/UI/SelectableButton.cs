using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableButton : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Button> buttons;
    public Color selectedColor = Color.red;  // Color for the selected button
    public Color normalColor = Color.white;  // Color for non-selected buttons
    public float exposureSelected = 10;

    public TileCasting tileCasting;

    void Start()
    {
        // Initialize each button with a click listener
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => SelectButton(button));
        }
        InvokeRepeating("FlashButtons", 0.5f, 0.5f);
    }

    void FlashButtons()
    {
        foreach (var button in buttons)
        {
            ColorBlock cb = button.colors;
            if (cb.normalColor == Color.white)
            {
                cb.normalColor = Color.green;
            }
            else
            {
                cb.normalColor = Color.white;
            }
            button.colors=cb;
        }
    }

    void Update()
    {

    }

    // Method to select a button and change colors appropriately
    public void SelectButton(Button selectedButton)
    {
        foreach (var button in buttons)
        {
            ColorBlock cb = button.colors;
            if (button == selectedButton)
            {
                // Change the color of the selected button
                cb.normalColor = selectedColor;
                cb.highlightedColor = selectedColor; // Optional: Set highlighted color as well
                float.TryParse(selectedButton.GetComponentInChildren<TMP_Text>().text.ToString().Replace("\"", "").Trim(), out exposureSelected);
            }
            else
            {
                // Reset the color of other buttons
                cb.normalColor = normalColor;
                cb.highlightedColor = normalColor; // Optional: Reset highlighted color as well
            }
            button.colors = cb; // Apply the color changes to the button
        }

        tileCasting.markerCube.GetComponent<WhiteboardMarker>().InstantiateLines();
    }
}
