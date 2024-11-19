using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using RoofTileVR.UI;
using TMPro;
using Unity.Mathematics;
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
        [SerializeField] private RoofObject m_RoofObject;
        
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
            if (m_RoofObject != null)
            {
                TryGetComponent(out m_RoofObject);
            }
        }

        /// <summary>
        /// -1 = spawned but unplaced , 1= spawned and placed , 0= no tile , -2= retry placement
        /// </summary>
        private int tileState = -1;
        public bool CheckCurrentTileState()
        {
            if (tileState == 1)
            {
                return true;
            }
            else if (tileState == 0 || tileState == -1)
            {
                return false;
            }

            return false;
        }

        [SerializeField] private Transform tileOffset;
        [SerializeField] private GameObject solidTile;

        [SerializeField] private TileObject currentTilePrefab;
        private TileObject prevTilePrefab;
        [SerializeField] private TileObject[] TileObjects;
        private int currentTileIndex;
        
        public void PlaceTileInFront(int tileIndex = 0)
        {
            currentTileIndex = tileIndex;
            distanceLog.transform.parent.gameObject.SetActive(false);
            
            m_TileSelectPanelUI.BGColorReset();
            Vector3 pos = tileOffset.position;
            //pos.y = 1f;
            GameObject placedObject =
                Instantiate(TileObjects[currentTileIndex].gameObject, pos, tileOffset.localRotation);
            
            if (prevTilePrefab)
            {
                prevTilePrefab = currentTilePrefab;
                prevTileLog.text = "last tile = " + prevTilePrefab.name + "";
            }
            currentTilePrefab = placedObject.GetComponent<TileObject>();
            tileState = -1;
            
        }

        public TextMeshProUGUI tileStatusPrompt;
        public void TileSelectText(string label)
        {
            tileStatusPrompt.text = label;

        }

        public TextMeshProUGUI tilePlaced_log;
        public void SetActiveTileStateToPlaced(int state = 1)
        {
            tileState = state;
            tilePlaced_log.transform.parent.gameObject.SetActive(true);
            //tilePlaced_log.text = $"Tile {i + 1} placed";
            PlaceTile();
        }

        public void SetActiveTileStatePlaceFail(int state = -1)
        {
            tileState = state;
            
            //re enable grab for the tile
            currentTilePrefab.EnableInteraction();
            //m_TilePlacementUI.HidePanel();
        }
        
        int i = 1;

        private List<GameObject> objectsToCheck = new List<GameObject>();
        private List<Transform> tileTransformList = new List<Transform>();
        
        void PlaceTile()
        {
            currentTilePrefab.ShowPlacementPrompt();
            
            currentTilePrefab.SetName("Tile" + i);
            currentTilePrefab.name = "Tile" + i;
            tilePlaced_log.text = $"{currentTilePrefab.name} placed";
            i++;

            DOVirtual.DelayedCall(2f, delegate()
            {
                tilePlaced_log.transform.parent.gameObject.SetActive(false);
            });
            try
            {
                if (objectsToCheck.Contains(currentTilePrefab.gameObject) == false)
                {
                    objectsToCheck.Add(currentTilePrefab.gameObject);
                }
            }
            catch (Exception e)
            {
                distanceLog.text = (e.Message);

            }
            //CheckProximity(currentTilePrefab.gameObject);
            CheckDistanceFromLastTile();
        }

        /// <summary>
        /// 1.0f distance in Unity = 39.37inch in real world 
        /// </summary>
        public const float Inch = 39.37f;

        public float checkSideEdge;
        
        public TextMeshProUGUI prevTileLog;

        public string rr(float f)
        {
            return f.ToString("F3");
        }
        public bool CheckDistanceFromLastTile()
        {
            
            StringBuilder log = new StringBuilder();
            if (prevTilePrefab != null)
            {
                distanceLog.transform.parent.gameObject.SetActive(true);
                //check horizontal distance from previous tile
                //var closestTile = GetLeftRightMostTilePos();
                var closestTile = prevTilePrefab;
                float dx = currentTilePrefab.transform.position.x - closestTile.transform.position.x;
                log.Append($"{closestTile.name} to {currentTilePrefab.name} in X: {rr(dx * Inch)}''");

                float sideEdgeDx= Mathf.Epsilon;
                if (dx > 0)
                {
                    //new tile is placed to RIGHT
                    //compare Tile 2 side edge left x Tile 1 side edge right
                    Vector3 tile2Edge = currentTilePrefab.SideEdgeLeft.position;
                    
                    Vector3 tile1Edge = closestTile.SideEdgeRight.position;

                    sideEdgeDx = Vector3.Distance(tile2Edge, tile1Edge);
                    sideEdgeDx = tile2Edge.x - tile1Edge.x;
                    log.Append($"\n{currentTilePrefab.name} placed on RIGHT of {closestTile.name} by dist: {rr(Mathf.Abs(sideEdgeDx) * Inch)}''");
                }
                else
                {
                    //new tile is placed to LEFT    
                    //compare Tile 2 side edge right x Tile 1 side edge left
                    Vector3 tile2Edge = currentTilePrefab.SideEdgeRight.position;
                    
                    Vector3 tile1Edge = closestTile.SideEdgeLeft.position;
                    
                    sideEdgeDx = Vector3.Distance(tile2Edge, tile1Edge);
                    sideEdgeDx = (tile2Edge.x - tile1Edge.x);
                    log.Append($"\n{currentTilePrefab.name} placed on LEFT of {closestTile.name} by dist: {rr(Mathf.Abs(sideEdgeDx) * Inch)}''");
                }

                sideEdgeDx = Mathf.Abs(sideEdgeDx);
                if (sideEdgeDx > checkSideEdge)
                {
                    //Fail
                    
                    log.Append($"\nBAD Keyway Spacing {currentTilePrefab.name} ({rr(sideEdgeDx * Inch)}'') >> {rr(checkSideEdge * Inch)}'' distance ");
                    if (dx > 0)
                    {
                        HighlightIncorrectTilePlacement(-1);
                    }
                    else
                    {
                        HighlightIncorrectTilePlacement(1);
                    }
                    distanceLog.text = log.ToString();
                    SetActiveTileStatePlaceFail(-2);

                    return true;
                }
                else if (sideEdgeDx > 0 && sideEdgeDx < checkSideEdge)
                {
                    //Pass
                    log.Append($"\nGOOD Keyway Spacing {currentTilePrefab.name} ({rr(sideEdgeDx * Inch)}'') ~= {rr(checkSideEdge * Inch)}'' distance");
                    HighlightCorrectTilePlacement();
                    distanceLog.text = log.ToString();

                    return true;
                }
            }
            else
            {
                prevTilePrefab = currentTilePrefab;
                prevTileLog.text = "last tile = " + prevTilePrefab.name + "";
            }

            return false;
        }

        public TileObject GetLeftRightMostTilePos()
        {
            TileObject result = prevTilePrefab;
            
            float dx = (currentTilePrefab.transform.position.x - prevTilePrefab.transform.position.x);
            float closestDist = Mathf.Abs(dx);
            
            foreach (GameObject tile in objectsToCheck)
            {
                if (tile != currentTilePrefab)
                {
                    dx = currentTilePrefab.transform.position.x - tile.transform.position.x;
                    if (closestDist < Mathf.Abs(dx))
                    {
                        closestDist = Mathf.Abs(dx);
                        result = tile.GetComponent<TileObject>();
                    }
                }
            }
            
            return result;
        }
        
        public float checkEaveOverhang;
        public float checkRakeOverhang;
        public void CheckDistanceFromRoofSides()
        {
            
            //check leftmost tile
            float dx_left = (currentTilePrefab.SideEdgeLeft.position -
                m_RoofObject.LeftRoofPoint.position).x;
            
            if (CompareRakeOverHangDist(dx_left))
            {
                
            }
            else
            {
                
            }
            //check rightmost tile
            float dx_right = currentTilePrefab.SideEdgeRight.position.x -
                             m_RoofObject.RightRoofPoint.position.x;
            
            if (CompareRakeOverHangDist(dx_right))
            {
                
            }
            else
            {
                
            }
            
            float dy = (currentTilePrefab.SideEdgeLeft.position -
                             m_RoofObject.LeftRoofPoint.position).y;
            if (CompareEaveOverHangDist(dy))
            {
                
            }
            else
            {
                
            }
        }

        bool CompareRakeOverHangDist(float tileDistance)
        {
            float rakeOverHangDist;
            
            // rakeOverHangDist = Mathf.Abs(tileDistance - checkRakeOverhang);
            if (tileDistance < 0)
            {
                
                return false;
            }
            
            if (tileDistance > (checkRakeOverhang * 0.9f) && tileDistance <= checkRakeOverhang)
            {
                return true;
            }
            
            return false;
        }
        
        bool CompareEaveOverHangDist(float tileDistance)
        {
            // rakeOverHangDist = Mathf.Abs(tileDistance - checkRakeOverhang);
            if (tileDistance < 0)
            {
                
                return false;
            }
            
            if (tileDistance > (checkEaveOverhang * 0.9f) && tileDistance <= checkEaveOverhang)
            {
                return true;
            }
            
            return false;
        }
        
        public void OnTilePick()
        {
            m_TilePlacementUI.HidePanel();
            
        }
        
        public MeshRenderer tileSideEdge_Effect;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"> for right= 1, for left= -1</param>
        public void HighlightIncorrectTilePlacement(int right)
        {
            m_TileSelectPanelUI.BGColorChange(Color.red);
            
            Transform _transform = tileSideEdge_Effect.transform;
            _transform.SetParent(currentTilePrefab.transform);
            _transform.localRotation = Quaternion.Euler(Vector3.zero);
            _transform.localScale = currentTilePrefab.EffectSideEdgeScale;
            
            tileSideEdge_Effect.material.color = currentTilePrefab.ColorFail;
            
            if (right == 1)
            {
                _transform.position = currentTilePrefab.SideEdgeRight.position;
            }
            else if (right == -1)
            {
                _transform.position = currentTilePrefab.SideEdgeLeft.position;
            }
            
            _transform.gameObject.SetActive(true);
            m_TilePlacementUI.ShowTileFailUI(currentTilePrefab.tileCanvasUI.transform);
        }

        [SerializeField] private TilePlacementUI m_TilePlacementUI;
        public void HighlightCorrectTilePlacement()
        {
            m_TileSelectPanelUI.BGColorChange(Color.green);
            
            Transform _transform = tileSideEdge_Effect.transform;
            _transform.SetParent(currentTilePrefab.transform);
            _transform.localRotation = Quaternion.Euler(Vector3.zero);
            _transform.localScale = new Vector3(1f, currentTilePrefab.EffectSideEdgeScale.y, 1f);
            _transform.localPosition = Vector3.zero;
            
            tileSideEdge_Effect.material.color = currentTilePrefab.ColorTrue;
            
            _transform.gameObject.SetActive(true);
            m_TilePlacementUI.ShowTilePassUI(currentTilePrefab.tileCanvasUI.transform);
        }

        private void DisableTileHighlightEffect()
        {
            tileSideEdge_Effect.gameObject.SetActive(false);
        }
        
        public void EnableTileGrab()
        {
            tileStatusPrompt.text = "enabled grab";
            currentTilePrefab.EnableInteraction();
        }
        
        public void DisableTileGrab()
        {
            currentTilePrefab.DisableInteraction();
        }
        
        public GameObject placementPrompt;
        public void ShowPlacementPrompt()
        {
            placementPrompt.gameObject.SetActive(true);
        }

        public void YesButtonPressed()
        {
            DisableTileGrab();
            SetActiveTileStateToPlaced(1);
            TileSelectText("Tile Placed! Pick New Tile");
            //placementPrompt.gameObject.SetActive(true);
        }
        
        public TextMeshProUGUI distanceLog;
        private void CheckProximity(GameObject placedObject)
        {
            //distanceLog.text = "run proximity check";
            
            foreach (GameObject obj in objectsToCheck)
            {
                if (obj != placedObject) // Skip the object itself
                {
                    float distance = Vector3.Distance(placedObject.transform.position, obj.transform.position);
                    //distanceLog.gameObject.SetActive(true);
                    //distanceLog.text = "Distance between " + placedObject.name + " and " + obj.name + ": " + distance*39.37 + " inch.";

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
                        //Debug.Log($"Edge {i} distance: {edgeDistance}");
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

        private int currentTile;
        
        public void ToggleCasting(int tileIndex)
        {
            if (tileIndex == currentTile)
            {
                isCastingActive = !isCastingActive;
            }
            else
            {
                isCastingActive = true;
            }
            
            currentTile = tileIndex;
            ToggleCastingUI();
        }
        
        

        [SerializeField] private TileSelectPanelUI m_TileSelectPanelUI;
        
        public void ToggleCastingUI()
        {
            m_TileSelectPanelUI.OnPanelSelect(isCastingActive, currentTile);
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
