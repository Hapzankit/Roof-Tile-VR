using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class WhiteboardMarker : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 15;
    [SerializeField] GameObject markerBall;
    // public GameObject Scale;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;

    public Whiteboard whiteboard;

    public bool isLineMade = false;


    private LineRenderer lineRenderer;
    private bool isObjectPicked = false;

    Transform pickedObject;
    public Transform secondObject;
    public Transform thirdObject;
    void Start()
    {
        pickedObject = this.transform;
        // _renderer = _tip.GetComponent<Renderer>();
        // _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        // _tipHeight = _tip.localScale.y;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // Draw();
        // if (Time.frameCount % 3 == 0) // Apply every 3 frames
        // {
        // DrawBroad();
        // }


        // if (isObjectPicked && pickedObject != null)
        // {
        // Update LineRenderer positions in real-time

        // Check if picked object is near the third object
        if (Vector3.Distance(pickedObject.position, thirdObject.position) < 0.05f)
        {
            Destroy(pickedObject.gameObject);
            isObjectPicked = false;
            lineRenderer.enabled = false; // Hide the line
            whiteboard.DrawVerticalAtDistance(11.5f);
            lineRenderer.SetPosition(0, thirdObject.position);
            lineRenderer.SetPosition(1, secondObject.position);
        }
        else
        {
            lineRenderer.SetPosition(0, pickedObject.position);
            lineRenderer.SetPosition(1, secondObject.position);

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


    // void OnTriggerEnter(Collider other)
    // {
    //     // Pick up the object on trigger
    //     if (!isObjectPicked && other.CompareTag("Pickup"))
    //     {
    //         pickedObject = other.transform;
    //         pickedObject.SetParent(transform); // Parent to player or hand
    //         isObjectPicked = true;
    //         lineRenderer.enabled = true;
    //     }
    // }

    // void OnTriggerExit(Collider other)
    // {
    //     // Release the object when it leaves the player's hand area
    //     if (isObjectPicked && other.transform == pickedObject)
    //     {
    //         pickedObject.SetParent(null);
    //         isObjectPicked = false;
    //     }
    // }


    // private void DrawBroad()
    // {
    //     int numRays = 4; // Number of raycasts to cast in a circular pattern
    //     float radius = 0.0002f; // Radius of the broad area

    //     // Cast multiple raycasts in a circular pattern
    //     for (int i = 0; i < numRays; i++)
    //     {
    //         float angle = (i / (float)numRays) * Mathf.PI * 2;
    //         Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

    //         Vector3 rayOrigin = _tip.position + offset;

    //         if (Physics.Raycast(rayOrigin, transform.up, out _touch, _tipHeight))
    //         {
    //             if (_touch.transform.CompareTag("Whiteboard"))
    //             {
    //                 if (_whiteboard == null)
    //                 {
    //                     _whiteboard = _touch.transform.GetComponent<Whiteboard>();
    //                 }

    //                 _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

    //                 var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
    //                 var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

    //                 if (y >= 0 && y < _whiteboard.textureSize.y && x >= 0 && x < _whiteboard.textureSize.x)
    //                 {
    //                     _whiteboard.texture.SetPixels(x, y, _penSize, _penSize, _colors);
    //                 }
    //             }
    //             // if (_touch.transform.CompareTag("TraceCaller"))
    //             // {
    //             //     isLineMade = whiteboard.isFirstLineMade();

    //             //     if (isLineMade)
    //             //     {
    //             //         whiteboard.DrawMarks();
    //             //         // Destroy(Scale);
    //             //         Destroy(this.gameObject);
    //             //     }
    //             //     _touch.transform.gameObject.GetComponent<MeshRenderer>().enabled = true;
    //             // }
    //         }
    //     }

    //     _whiteboard?.texture.Apply(); // Apply the texture update
    // }




}
