using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using RoofTileVR.UI;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace RoofTileVR
{
    public class TileCasting : MonoBehaviour
    {
        public Transform rightController; // Reference to the right controller transform
        public float boxCastSize = 0.1f; // Size of the box cast
        public LayerMask layerMask; // Layer mask to define what to hit

        // [SerializeField] private GameObject currentPlaceholder;
        [SerializeField] private TextMeshProUGUI logTMP;


        private System.Nullable<Vector3> prevHitPoint = null;
        private Vector3 prevPos;
        private Vector3 prevRot;
        [SerializeField] private RoofObject m_RoofObject;

        public bool isCastingActive = false;

        public static Action<string> tilePlacedMsg;
        public GameObject starterColliderHolder;

        public List<BoxCollider> starterColliders;
        public GameObject markerCube;


        [SerializeField] public List<GameObject> TileStands;
        public float tileSpanWidthConstant;
        public float tileSpanWidth = 0;

        public float currentTileWidth;
        public float overlapSpanAccordingtoExposure = 11;
        // public TMP_Dropdown exposureDropdown;

        public List<HandMenu> handMenu;
        bool isTilePicked = false;

        public Transform BoardPosition;
        public List<Transform> boardPositionsToSnap;
        public List<float> exposureConvertedToUnityfromInches;

        public GameObject TileStandGroup;
        TileObject[] TileGrabInteractables;

        public AODPanel aODPanel;

        void Awake()
        {
            TileGrabInteractables = TileStandGroup.GetComponentsInChildren<TileObject>();
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


        // Start is called before the first frame update

        public void WriteOnHandMenu(String textToWrite)
        {
            foreach (var menu in handMenu)
            {
                if (menu)
                {
                    menu.handText.text = textToWrite;
                }
            }
        }
        void Start()
        {
            exposureConvertedToUnityfromInches.Add(10);

            WriteOnHandMenu("Draw a chalk Line");


            foreach (GameObject stands in TileStands)
            {
                stands.SetActive(false);
            }

            foreach (var coll in starterColliderHolder.GetComponentsInChildren<BoxCollider>())
            {
                starterColliders.Add(coll);
                tileSpanWidth += 12.18f;
                coll.gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = false;
            }

            tileSpanWidthConstant = tileSpanWidth;
            print("Width of roof" + tileSpanWidthConstant);
            starterColliders[0].gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = true;

            if (m_RoofObject != null)
            {
                TryGetComponent(out m_RoofObject);
            }


        }

        public void EnableDisableTileGrab(bool shouldBeGrabbed, string messageToWrite, float timeToShow, Color AODcolor)
        {
            foreach (TileObject tile in TileGrabInteractables)
            {
                tile.isGrabbable = shouldBeGrabbed;
                tile.messageToWrite = messageToWrite;
                tile.timeToShow = timeToShow;
                tile.AODcolor = AODcolor;
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




        [SerializeField] public TileObject currentTilePrefab;

        public void SetActiveTileStateToPlaced(int state = 1)
        {
            tileState = state;
            // tilePlaced_log.transform.parent.gameObject.SetActive(true);
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
        // private List<Transform> tileTransformList = new List<Transform>();


        public TileDropCollisionCheck currentTileRegion;

        public List<GameObject> tilesPickedUp;



        void PlaceTile()
        {
            currentTilePrefab.ShowPlacementPrompt();

            try
            {
                if (objectsToCheck.Contains(currentTilePrefab.gameObject) == false)
                {
                    objectsToCheck.Add(currentTilePrefab.gameObject);
                    // currentTilePrefab.SetName("Tile" + i);
                    currentTilePrefab.name = "Tile" + i;
                    i++;
                }
            }
            catch (Exception e)
            {
                distanceLog.text = (e.Message);

            }




        }







        public TileObject CurrentTileObject => currentTilePrefab;




        public void GetDistanceAccordingToExposure(float num)
        {
            //Placing tiles according to exposure chosen
            overlapSpanAccordingtoExposure = num;

        }



        public void OnTilePick()
        {
            m_TilePlacementUI.HidePanel();
            placementPrompt.SetActive(false);
            wrongRegionPlacementPrompt.SetActive(false);
            isTilePicked = true;
        }


        public MeshRenderer tileSideEdge_Effect;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"> for right= 1, for left= -1</param>

        [SerializeField] private TilePlacementUI m_TilePlacementUI;






        public GameObject placementPrompt;
        public GameObject wrongRegionPlacementPrompt;
        public void ShowPlacementPrompt()
        {



            currentTilePrefab.GetComponent<Rigidbody>().isKinematic = true;
            currentTilePrefab.GetComponent<Rigidbody>().useGravity = false;
            YesButtonPressed();
            currentTilePrefab.GetComponent<TileObject>().BoltPlaceHolders[0].gameObject.SetActive(true);
            currentTilePrefab.GetComponent<TileObject>().BoltPlaceHolders[1].gameObject.SetActive(true);
            currentTilePrefab.GetComponent<TileObject>().DestroyErrors();

        }




        public void OnTileDropped()
        {
            // print("Is all starter placed " + starterTilesPlaced + isFirstShakePlaced);
            isTilePicked = false;
            // SnapToTheRoof();

            // SnapTilesAndShowErrors();



        }





        public TileObject TileToCompare;
        public bool rightToLeft = true;
        public bool starterTilesPlaced = false;
        public void changeTiles()
        {
            int num = starterColliders.IndexOf(currentTileRegion.GetComponent<BoxCollider>());
            print("Change starter tiles" + num);

            currentTileRegion.gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = false;
            if (num < starterColliders.Count - 1)
            {
                starterColliders[++num].gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = true;
            }
            else
            {
                starterColliders[num].gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = true;
                starterTilesPlaced = true;
                // TileToCompare
                foreach (var coll in starterColliders)
                {
                    coll.GetComponent<MeshRenderer>().enabled = true;
                }
            }

            placementPrompt.gameObject.SetActive(true);
        }
        public List<GameObject> TilesPlaced;
        public bool isFirstShakePlaced = false;
        public bool reverseTheLine = false;
        public StatisticsManager statisticsManager;
        public int linesOfTileTobePlaced = 1;
        public void YesButtonPressed()
        {
            int numOfTile = 9999;
            currentTilePrefab.transform.SetParent(null);
            StartCoroutine(aODPanel.WriteTextForTime(1, Color.green, "Tile placed!"));
            EnableDisableTileGrab(false, "Fasten the previous tile first!", 3, Color.red);
            // for strter tiles
            if (!currentTilePrefab.GetComponent<TileObject>().isStarter)
            {
                isFirstShakePlaced = true;
                print("First Shake Placed");
            }
            // normal shakes
            else
            {
                int num = starterColliders.IndexOf(currentTileRegion.GetComponent<BoxCollider>());
                // print("Change starter tiles" + num + " " + starterColliders.Count);

                currentTileRegion.gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = false;
                // currentTileRegion.gameObject.GetComponent<TileDropCollisionCheck>().isNormalRegion = ;
                if (num < starterColliders.Count - 1)
                {
                    starterColliders[++num].gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = true;
                    // print("Tile number" + num);
                }
                else
                {
                    starterTilesPlaced = true;
                    // foreach (var tiles in TilesPlaced)
                    // {
                    //     tiles.GetComponent<l>().tileSize = 12;
                    // }
                    markerCube.SetActive(true);
                    markerCube.GetComponent<WhiteboardMarker>().ChangeObjects();
                    starterColliders[starterColliders.Count - 1].gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = false;

                    print("All tiles placed");
                    WriteOnHandMenu("All Starter tiles placed, Now select Exposure.");

                    numOfTile = num;
                }
            }



            if (tileSpanWidth <= 0)
            {
                tileSpanWidth = tileSpanWidthConstant;
                reverseTheLine = !reverseTheLine;

            }



            currentTilePrefab.GetComponent<XRGrabInteractable>().enabled = false;
            currentTilePrefab.GetComponent<TileObject>().CorrectTileIndicator.SetActive(false);
            tileSpanWidth -= currentTileWidth;

            if (tileSpanWidth < 0 && currentTilePrefab)
            {
                print("Called once right to left");
                rightToLeft = !rightToLeft;

            }
            TilesPlaced.Add(currentTilePrefab.gameObject);
            if (currentTilePrefab.GetComponent<TileObject>().isStarter)
            {
                // print("apply the area covered");
                currentTilePrefab.GetComponent<TileObject>().tileSize = 12;
                currentTilePrefab.areaLeftByTileAbove = 12;
            }
            else
            {
                currentTilePrefab.GetComponent<TileObject>().ReduceTileArea();
            }
            // print("Tile placed number" + TilesPlaced.Count);
            // DisableTileGrab();
            SetActiveTileStateToPlaced(1);
            // TileSelectText("Tile Placed! Pick New Tile");
            print("Tile width reduced to" + tileSpanWidth + " " + currentTileWidth);
            currentTilePrefab.GetComponent<TileObject>().isPlaced = true;
            if (numOfTile > starterColliders.Count)
            {
                WriteOnHandMenu("Now fasten the fasteners to the tile (it should be atleast 3/4\" inches deep).");
            }

            print(linesOfTileTobePlaced + "Number of lines of tile placed " + markerCube.GetComponent<WhiteboardMarker>().whiteboard.numberOfLinesOftileTobeMade);
            if (tileSpanWidth <= 0)
            {
                linesOfTileTobePlaced++;
                BoardPosition = boardPositionsToSnap[linesOfTileTobePlaced - 1];
            }
            if (linesOfTileTobePlaced == markerCube.GetComponent<WhiteboardMarker>().whiteboard.numberOfLinesOftileTobeMade + 2)
            {
                statisticsManager.SaveStatistics(tilesPickedUp, TilesPlaced);
                statisticsManager.ShowStatsTable();
            }

            // placementPrompt.gameObject.SetActive(true);
        }


        public TextMeshProUGUI distanceLog;






        // Update is called once per frame
        void SnapTilesAndShowErrors()
        {
            if (currentTilePrefab.isValidTile && !currentTilePrefab.checkKeywayFlag)
            {

                if (starterTilesPlaced)
                {
                    if (isFirstShakePlaced)
                    {
                        if (currentTilePrefab.CalculateSidelapCheck())
                        {

                            if (currentTilePrefab.ShowKeywayerrors())
                            {
                                ShowPlacementPrompt();
                                currentTilePrefab.tileNameText.text = "";
                                currentTilePrefab.checkKeywayFlag = true;
                                // currentTilePrefab = null;
                                // currentTilePrefab.GetComponent<XRGrabInteractable>().enabled = false;
                            }
                        }
                    }
                    else
                    {
                        if (currentTilePrefab.CalculateSidelapCheck())
                        {

                            if (currentTilePrefab.ShowShakeTIleErrors(!isFirstShakePlaced))
                            {
                                ShowPlacementPrompt();
                                currentTilePrefab.tileNameText.text = "";
                                currentTilePrefab.checkKeywayFlag = true;
                                // currentTilePrefab = null;
                                // currentTilePrefab.GetComponent<XRGrabInteractable>().enabled = false;

                            }
                        }
                    }
                }
                else
                {
                    if (currentTilePrefab.CalculateSidelapCheck())
                    {

                        if (currentTilePrefab.ShowStarterErrors())
                        {
                            ShowPlacementPrompt();
                            currentTilePrefab.tileNameText.text = "";
                            currentTilePrefab.checkKeywayFlag = true;
                            // currentTilePrefab = null;
                            // currentTilePrefab.GetComponent<XRGrabInteractable>().enabled = false;

                        }
                    }
                }
            }

        }
        public void ShowHideTiles(bool shouldShow)
        {
            foreach (GameObject stand in TileStands)
            {
                stand.SetActive(shouldShow);
            }
        }

        void Update()
        {
            if (isTilePicked)
            {
                // print("Running function");
                if (currentTilePrefab)
                {
                    SnapTilesAndShowErrors();
                }
            }


            if (markerCube.GetComponent<WhiteboardMarker>().isLineDrawnForStarter)
            {
                ShowHideTiles(true);
            }
            else
            {
                ShowHideTiles(false);
            }
            if (currentTilePrefab && currentTilePrefab.isFirstBoltPlaced && currentTilePrefab.isSecondBoltPlaced)
            {
                EnableDisableTileGrab(true, "", 3, Color.white);
                StartCoroutine(aODPanel.WriteTextForTime(1, Color.green, "Fastened!"));
                currentTilePrefab = null;
            }
            // currentTilePrefab.ShowShakeTIleErrors(true);
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



            }
            else
            {
                SetLog($"No Object detected in front {rightController.name}");
            }

        }










        [SerializeField] private TileSelectPanelUI m_TileSelectPanelUI;


        public void SetLog(string log)
        {
            logTMP.text = log;
        }


    }
}
