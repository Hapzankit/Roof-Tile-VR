using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoofTileVR
{
    public class TileCasting1 : MonoBehaviour
    {
        public Transform rightController; // Reference to the right controller transform
        public GameObject placeholderPrefab; // The placeholder object to show
        public GameObject prefabToPlace; // The prefab to place when the trigger is pressed
        public float boxCastSize = 0.1f; // Size of the box cast
        public LayerMask layerMask; // Layer mask to define what to hit
        public List<GameObject> objectsToCheck;

        [SerializeField] private GameObject currentPlaceholder;
        [SerializeField] private TextMeshProUGUI logTMP;
        [SerializeField] private GameObject triggerTMP;
        [SerializeField] private InputAction RightTriggerAction;
        [SerializeField] private InputActionReference RightTriggerRef;
        [SerializeField] private InputActionReference PrimaryButtonRef;
        [SerializeField] GameObject indicator;
        [SerializeField] TMP_Text distanceErrorText;
        [SerializeField] TMP_Text distanceOverlapText;
        private System.Nullable<Vector3> prevHitPoint = null;
        private Vector3 prevPos;
        private Vector3 prevRot;

        [SerializeField] private float thresholdDistance;
        public bool isCastingActive = false;

        public static Action<string> tilePlacedMsg;
        private void OnEnable()
        {
            //RightTriggerAction += TriggerPressed();
            RightTriggerRef.action.performed += RightTriggerPressed;
            RightTriggerRef.action.Enable();

            PrimaryButtonRef.action.performed += OnPrimaryButtonPressed;
            PrimaryButtonRef.action.Enable();

        }

        private void OnDisable()
        {
            RightTriggerRef.action.performed -= RightTriggerPressed;
            RightTriggerRef.action.Disable();

            PrimaryButtonRef.action.performed -= OnPrimaryButtonPressed;
            PrimaryButtonRef.action.Disable();
        }

        private void EnableIA(InputActionReference inputActionReference, System.Action<InputAction.CallbackContext> callBackFunction)
        {
            inputActionReference.action.performed += callBackFunction;
            inputActionReference.action.Enable();
        }

        private void DisableIA(InputActionReference inputActionReference, System.Action<InputAction.CallbackContext> callBackFunction)
        {
            inputActionReference.action.Reset();
            inputActionReference.action.Enable();
        }

        public void RightTriggerPressed(InputAction.CallbackContext obj)
        {
            if (currentPlaceholder.activeSelf)
            {
                PlaceObject(prevPos, prevRot);
                tilePlacedMsg?.Invoke("Tile placed success");
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //currentPlaceholder = Instantiate(placeholderPrefab);
            currentPlaceholder.SetActive(false);
            triggerTMP.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (isCastingActive == false) return;

            // Perform the box casting
            Vector3 origin = rightController.position;
            Vector3 direction = rightController.forward;
            Vector3 halfExtents = new Vector3(boxCastSize, boxCastSize, boxCastSize);

            if (Physics.BoxCast(origin, halfExtents, direction, out RaycastHit hit, Quaternion.identity, Mathf.Infinity, layerMask))
            {
                if (prevHitPoint == null)
                {
                    prevHitPoint = hit.point;
                }
                else
                {

                }

                // Show the placeholder at the hit position
                SetLog($"Hit detected: {hit.point}");
                prevPos = hit.point;
                prevRot = hit.normal;

                currentPlaceholder.transform.position = hit.point;
                currentPlaceholder.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                currentPlaceholder.SetActive(true);

            }
            else
            {
                // Hide the placeholder if no hit
                currentPlaceholder.SetActive(false);
                SetLog($"No Object detected in front {rightController.name}");
            }
        }

        public float proximityThreshold = 0.5f; // Set your desired proximity threshold



        private Vector3[] GetCorners(GameObject obj)
        {
            BoxCollider collider = obj.GetComponent<BoxCollider>();
            Vector3[] corners = new Vector3[8];
            Vector3 center = obj.transform.position;
            Vector3 extents = collider.size * 0.5f;
            Quaternion rotation = obj.transform.rotation;

            corners[0] = center + rotation * new Vector3(-extents.x, -extents.y, -extents.z);
            corners[1] = center + rotation * new Vector3(extents.x, -extents.y, -extents.z);
            corners[2] = center + rotation * new Vector3(extents.x, -extents.y, extents.z);
            corners[3] = center + rotation * new Vector3(-extents.x, -extents.y, extents.z);
            corners[4] = center + rotation * new Vector3(-extents.x, extents.y, -extents.z);
            corners[5] = center + rotation * new Vector3(extents.x, extents.y, -extents.z);
            corners[6] = center + rotation * new Vector3(extents.x, extents.y, extents.z);
            corners[7] = center + rotation * new Vector3(-extents.x, extents.y, extents.z);

            return corners;
        }

        private void CheckProximity(GameObject placedObject)
        {
            foreach (GameObject obj in objectsToCheck)
            {
                if (obj != placedObject) // Skip the object itself
                {
                    float distance = Vector3.Distance(placedObject.transform.position, obj.transform.position);
                    Debug.Log("Distance between " + placedObject.name + " and " + obj.name + ": " + distance);

                    // Calculate edge distances
                    Vector3[] corners1 = GetCorners(placedObject);
                    Vector3[] corners2 = GetCorners(obj);
                    for (int i = 0; i < 12; i++)
                    {
                        Vector3 point1Start = corners1[EdgePairs[i, 0]];
                        Vector3 point1End = corners1[EdgePairs[i, 1]];
                        Vector3 point2Start = corners2[EdgePairs[i, 0]];
                        Vector3 point2End = corners2[EdgePairs[i, 1]];

                        float edgeDistance = Vector3.Distance(point1Start, point2Start); // Simplified; consider the actual edge distance calculation
                        Debug.Log($"Edge {i} distance: {edgeDistance}");
                    }
                }
            }
        }

        static readonly int[,] EdgePairs = new int[,]
        {
            {0, 1}, {1, 2}, {2, 3}, {3, 0}, // Bottom face edges
            {4, 5}, {5, 6}, {6, 7}, {7, 4}, // Top face edges
            {0, 4}, {1, 5}, {2, 6}, {3, 7}  // Side edges connecting top and bottom
        };




        Vector3 errorpoint;
        private float CalculateGap(GameObject objectA, GameObject objectB)
        {
            Collider colliderA = objectA.GetComponent<Collider>();
            Collider colliderB = objectB.GetComponent<Collider>();

            if (colliderA != null && colliderB != null)
            {
                // Check if the colliders are overlapping
                Vector3 direction;
                float penetrationDepth;

                // Check for penetration to determine if the colliders are overlapping
                bool isOverlapping = Physics.ComputePenetration(
                    colliderA, colliderA.transform.position, colliderA.transform.rotation,
                    colliderB, colliderB.transform.position, colliderB.transform.rotation,
                    out direction, out penetrationDepth
                );

                // Calculate the closest points between the colliders
                Vector3 closestPointA = colliderA.ClosestPoint(colliderB.transform.position);
                Vector3 closestPointB = colliderB.ClosestPoint(colliderA.transform.position);

                // Always calculate the distance between the closest points
                float distance = Vector3.Distance(closestPointA, closestPointB);

                // If there's an overlap, consider the distance as negative
                if (isOverlapping)
                {
                    distance = -distance;
                }

                return distance;

            }
            else
            {
                Debug.LogError("Both GameObjects need Colliders to calculate the gap.");
                return -1f; // Return a negative value to indicate an error
            }
        }
        int i = 0;
        void PlaceObject(Vector3 position, Vector3 normal)
        {
            // Instantiate the prefab at the hit position with rotation based on the normal
            GameObject placedObject = Instantiate(prefabToPlace, position, Quaternion.FromToRotation(Vector3.up, normal));
            objectsToCheck.Add(placedObject);
            placedObject.name = "Tile" + i;
            CheckProximity(placedObject);
            placedObject.GetComponentInChildren<TMP_Text>().text = placedObject.name;
            i++;
            // Optional: You can add more logic here, like snapping or parenting
        }

        public void QuitApp()
        {
            Application.Quit();
        }

        public void ToggleCasting()
        {
            isCastingActive = !isCastingActive;
        }

        public void ToggleCastingUI()
        {

        }

        public void SetLog(string log)
        {
            logTMP.text = log;
        }

        public void TriggerDetect()
        {
            triggerTMP.gameObject.SetActive(true);
            Invoke(nameof(triggerInvoke), 0.7f);
        }

        public void OnRightTriggerPressed()
        {
            if (currentPlaceholder.activeSelf)
            {
                PlaceObject(prevPos, prevRot);
            }
        }

        public void OnPrimaryButtonPressed(InputAction.CallbackContext obj)
        {
            if (currentPlaceholder.activeSelf)
            {
                PlaceObject(prevPos, prevRot);
            }
        }

        private void triggerInvoke()
        {
            triggerTMP.gameObject.SetActive(false);

        }
    }
}
