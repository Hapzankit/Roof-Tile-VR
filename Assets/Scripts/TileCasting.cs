using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoofTileVR
{
    public class TileCasting : MonoBehaviour
    {
        public Transform rightController; // Reference to the right controller transform
        public GameObject placeholderPrefab; // The placeholder object to show
        public GameObject prefabToPlace; // The prefab to place when the trigger is pressed
        public float boxCastSize = 0.1f; // Size of the box cast
        public LayerMask layerMask; // Layer mask to define what to hit
        
        [SerializeField] private GameObject currentPlaceholder;
        [SerializeField] private TextMeshProUGUI logTMP;
        [SerializeField] private GameObject triggerTMP;
        [SerializeField] private InputAction RightTriggerAction;
        [SerializeField] private InputActionReference RightTriggerRef;
        [SerializeField] private InputActionReference PrimaryButtonRef;
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
            if (isCastingActive == false)   return;
            
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
        
        void PlaceObject(Vector3 position, Vector3 normal)
        {
            // Instantiate the prefab at the hit position with rotation based on the normal
            GameObject placedObject = Instantiate(prefabToPlace, position, Quaternion.FromToRotation(Vector3.up, normal));
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
