using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class WhiteboardMarker : MonoBehaviour
{

    public Whiteboard whiteboard;

    public bool isLineMade = false;


    private LineRenderer lineRenderer;
    private bool isObjectPicked = false;

    Transform pickedObject;
    public Transform secondObject;
    public Transform thirdObject;


    public Transform secondObjectShake;
    public Transform thirdObjectShake;
    public bool isLineDrawnForStarter = false;
    public bool isExposureButtonActive = false;
    public GameObject markerBody;




    public float DrawLineAtDistance = 11.5f;
    void Start()
    {
        pickedObject = this.transform;


        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.positionCount = 2;
    }
    bool isSecondLine = false;
    public void ChangeObjects()
    {
        secondObjectShake.transform.gameObject.SetActive(true);
        thirdObjectShake.transform.gameObject.SetActive(true);
        markerBody.SetActive(false);
        isExposureButtonActive = true;
        DrawLineAtDistance = 21.5f;
        // StartCoroutine(whiteboard.tileCasting.aODPanel.WriteTextForTime(6, Color.red, "Select Exposure to place shakes"));
        whiteboard.tileCasting.EnableDisableTileGrab(false, "First select the exposure!", 2, Color.red);
    }

    public void InstantiateLines()
    {
        isObjectPicked = false;
        whiteboard.DrawMarks();
        lineRenderer.enabled = false; // Hide the line
        whiteboard.DrawVerticalAtDistance(DrawLineAtDistance);
        whiteboard.tileCasting.WriteOnHandMenu("Now start placing shakes over starter tiles from bottom right.");
        // StartCoroutine(whiteboard.tileCasting.aODPanel.WriteTextForTime(3, Color.red, "Select Exposure to place shakes"));
        pickedObject.gameObject.SetActive(false);
        secondObjectShake.gameObject.SetActive(false);
        thirdObjectShake.gameObject.SetActive(false);
        // isLineDrawnAfterExposure = true;
        whiteboard.tileCasting.EnableDisableTileGrab(true, "", 3, Color.white);
        // Destroy(this);
    }

    void Update()
    {




        if (isSecondLine)
        {
            lineRenderer.enabled = true;
            if (Vector3.Distance(pickedObject.position, thirdObjectShake.position) < 0.05f)
            {


                isObjectPicked = false;
                whiteboard.DrawMarks();
                lineRenderer.enabled = false; // Hide the line
                whiteboard.DrawVerticalAtDistance(DrawLineAtDistance);
                lineRenderer.SetPosition(0, thirdObjectShake.position);
                lineRenderer.SetPosition(1, secondObjectShake.position);

                whiteboard.tileCasting.WriteOnHandMenu("Now start placing normal shakes over starter tiles from bottom right");
                pickedObject.gameObject.SetActive(false);
                secondObjectShake.gameObject.SetActive(false);
                thirdObjectShake.gameObject.SetActive(false);



            }
            else
            {

                lineRenderer.SetPosition(0, pickedObject.position);
                lineRenderer.SetPosition(1, secondObjectShake.position);
            }
        }

        else
        {
            if (Vector3.Distance(pickedObject.position, thirdObject.position) < 0.05f)
            {
                isLineDrawnForStarter = true;
                isObjectPicked = false;
                // whiteboard.DrawMarks();
                lineRenderer.enabled = false; // Hide the line
                whiteboard.DrawVerticalAtDistance(DrawLineAtDistance);
                lineRenderer.SetPosition(0, thirdObject.position);
                lineRenderer.SetPosition(1, secondObject.position);
                this.transform.position = new Vector3(secondObjectShake.position.x, secondObjectShake.position.y, secondObjectShake.position.z);
                whiteboard.tileCasting.WriteOnHandMenu("Now Place Starter tiles on correct place (starting from bottom right) until its border turns green");
                // StartCoroutine(whiteboard.tileCasting.aODPanel.WriteTextForTime(3, Color.green, "Chalk line drawn!"));
                pickedObject.gameObject.SetActive(false);
                secondObject.gameObject.SetActive(false);
                thirdObject.gameObject.SetActive(false);


                // ChangeObjects();

            }
            else
            {
                lineRenderer.SetPosition(0, pickedObject.position);
                lineRenderer.SetPosition(1, secondObject.position);
            }
        }
        // }
        // else
        // {
        //     lineRenderer.enabled = false; // Disable line if no object is picked
        // }
    }

    public void OnMarkerpicked()
    {
        this.transform.rotation = Quaternion.Euler(0, 90, 120);
    }







}
