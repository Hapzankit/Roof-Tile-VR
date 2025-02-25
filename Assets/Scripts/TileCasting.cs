using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using RoofTileVR.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
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


        private Vector3? prevHitPoint = null;
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

        public Cutter cutter;

        public GameObject rightEnd;
        public GameObject leftEnd;

        public BoltMachine boltMachine;

        public GameObject CuttingTutorial;

        public ExposureMeasurement exposureMeasurement;

        void Awake()
        {
            TileGrabInteractables = TileStandGroup.GetComponentsInChildren<TileObject>();
            foreach (GameObject stand in TileStands)
            {
                stand.SetActive(true);
            }
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
            CuttingTutorial.SetActive(false);
            exposureMeasurement.gameObject.SetActive(false);
            exposureConvertedToUnityfromInches.Add(10);

            WriteOnHandMenu("Draw a chalk Line");

            AudioHandler.Instance.StartIntroductionSound();



            foreach (var coll in starterColliderHolder.GetComponentsInChildren<BoxCollider>())
            {
                starterColliders.Add(coll);
                tileSpanWidth += 12f;
                coll.gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = false;
            }

            tileSpanWidthConstant = tileSpanWidth;
            print("Width of roof" + tileSpanWidthConstant);
            starterColliders[0].gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = true;

            if (m_RoofObject != null)
            {
                TryGetComponent(out m_RoofObject);
            }

            markerCube.SetActive(false);


        }

        public void EnableDisableTileGrab(bool shouldBeGrabbed, string messageToWrite, float timeToShow, Color AODcolor)
        {
            print("Change tile grab" + shouldBeGrabbed);
            foreach (TileObject tile in TileGrabInteractables)
            {
                if (!tile.isPlaced)
                {

                    tile.isGrabbable = shouldBeGrabbed;
                    tile.messageToWrite = messageToWrite;
                    tile.timeToShow = timeToShow;
                    tile.AODcolor = AODcolor;

                    if (shouldBeGrabbed)
                    {
                        tile.GetComponent<XRGrabInteractable>().interactionLayers = InteractionLayerMask.GetMask("Default");
                    }
                }
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
        public TileObject previousTilePrefab;

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
            bool checkForLastTile = false;
            if (currentTilePrefab.tilesUnderneath.Count > 0)
            {
                if (currentTilePrefab.tilesUnderneath.Count > 1)
                {
                    checkForLastTile = currentTilePrefab.tilesUnderneath[currentTilePrefab.tilesUnderneath.Count - 1].tileSize + currentTilePrefab.areaLeftByTile < currentTilePrefab.tileSize;
                }
                else
                {
                    checkForLastTile = TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().areaLeftByTile < currentTilePrefab.tileSize;
                }
            }
            if (currentTilePrefab.isStarter)
            {
                checkForLastTile = false;
            }
            if (currentTilePrefab.isStarter || (tileSpanWidth - currentTilePrefab.tileSize < 0.5f && checkForLastTile))
            {
                if (!currentTilePrefab.yesbuttonPressed)
                {

                    YesButtonPressed();
                }
            }
            currentTilePrefab.GetComponent<TileObject>().BoltPlaceHolders[0].gameObject.SetActive(true);
            currentTilePrefab.GetComponent<TileObject>().BoltPlaceHolders[1].gameObject.SetActive(true);
            EnableDisableTileGrab(false, "Check Fasteners", 3, Color.red);
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
        public float ActualTileSpanWidth = 0;
        float actualTileWidthLeft = 0;
        public void YesButtonPressed()
        {

            currentTilePrefab = previousTilePrefab;
            int numOfTile = 9999;
            currentTilePrefab.transform.SetParent(null);
            StartCoroutine(aODPanel.WriteTextForTime(1, Color.green, "Tile placed!"));

            // for strter tiles
            if (!currentTilePrefab.GetComponent<TileObject>().isStarter)
            {

                isFirstShakePlaced = true;
                // print("First Shake Placed");
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
                    // AudioHandler.Instance.PlayListOfSound();
                    markerCube.SetActive(true);
                    markerCube.GetComponent<WhiteboardMarker>().ChangeObjects();
                    starterColliders[starterColliders.Count - 1].gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = false;
                    print("All tiles placed");
                    List<AudioHandler.Sound> clip = new List<AudioHandler.Sound>();
                    clip.Add(AudioHandler.Sound.MaximumExposureis); clip.Add(AudioHandler.Sound.ChalkLineto); clip.Add(AudioHandler.Sound.SelectYourDesired);
                    AudioHandler.Instance.DoAfterSound += HideExposure;
                    AudioHandler.Instance.DoBeforeSound += ShowExposure;
                    AudioHandler.Instance.PlayListOfSound(clip);
                    WriteOnHandMenu("All Starter tiles placed, Now select Exposure.");
                    numOfTile = num;

                }
            }




            if (tileSpanWidth <= 0)
            {
                actualTileWidthLeft = ActualTileSpanWidth;
                ActualTileSpanWidth = 0;
                tileSpanWidth = tileSpanWidthConstant;
                reverseTheLine = !reverseTheLine;

            }



            currentTilePrefab.GetComponent<XRGrabInteractable>().enabled = false;
            currentTilePrefab.GetComponent<TileObject>().CorrectTileIndicator.SetActive(false);


            float tileSpanWidthBefore = 0;

            if (!currentTilePrefab.isStarter)
            {
                if (rightToLeft)
                {
                    if (TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().rowNumber != currentTilePrefab.rowNumber)
                    {
                        actualTileWidthLeft -= currentTilePrefab.tileSize;
                        tileSpanWidthBefore = tileSpanWidth;
                        tileSpanWidth = actualTileWidthLeft;
                    }
                    else
                    {
                        rightEnd = TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().SideEdgeLeft.gameObject;
                        leftEnd = currentTilePrefab.SideEdgeLeft.gameObject;

                        actualTileWidthLeft -= Vector3.Distance(rightEnd.transform.position, leftEnd.transform.position) * 39.37f;
                        tileSpanWidthBefore = tileSpanWidth;
                        tileSpanWidth = actualTileWidthLeft;
                        print("Width to decrease" + Vector3.Distance(rightEnd.transform.position, leftEnd.transform.position) * 39.37f);
                    }
                }
                else
                {
                    if (TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().rowNumber != currentTilePrefab.rowNumber)
                    {
                        actualTileWidthLeft -= currentTilePrefab.tileSize;
                        tileSpanWidthBefore = tileSpanWidth;
                        tileSpanWidth = actualTileWidthLeft;
                    }
                    else
                    {

                        rightEnd = TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().SideEdgeRight.gameObject;
                        leftEnd = currentTilePrefab.SideEdgeRight.gameObject;
                        actualTileWidthLeft -= Vector3.Distance(rightEnd.transform.position, leftEnd.transform.position) * 39.37f;
                        tileSpanWidthBefore = tileSpanWidth;
                        tileSpanWidth = actualTileWidthLeft;
                        print("Width to decrease" + Vector3.Distance(rightEnd.transform.position, leftEnd.transform.position) * 39.37f);
                    }
                }

            }









            // tileSpanWidth -= currentTileWidth;

            if (tileSpanWidth <= 0 && currentTilePrefab)
            {
                // cutter.transform.SetParent(currentTilePrefab.cutPosition.transform);
                // cutter.MoveTheCutter(currentTilePrefab.cutPosition.Top.transform, currentTilePrefab.cutPosition.Bottom.transform);
                // print("Called once right to left");
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
                // cutter.transform.position = currentTilePrefab.transform.position;


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
            float arealeft;
            bool cutTheTile = false;
            if (currentTilePrefab.tilesUnderneath.Count > 0)
            {
                if (currentTilePrefab.tilesUnderneath.Count > 1)
                {
                    cutTheTile = tileSpanWidthBefore < currentTilePrefab.tileSize;
                }
                else
                {
                    cutTheTile = TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().areaLeftByTile < currentTilePrefab.tileSize;
                }
            }
            // if (tileSpanWidth < 12 && tileSpanWidth > 0)
            // {
            //     //     if (currentTilePrefab.tilesUnderneath.Count > 1)
            //     //     {
            //     //         tileSpanWidth = currentTilePrefab.tilesUnderneath[currentTilePrefab.tilesUnderneath.Count - 1].tileSize + TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().areaLeftByTile;
            //     //     }
            //     // else
            //     // {
            //     tileSpanWidth = TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().areaLeftByTile;
            //     // }
            // }


            if (tileSpanWidth <= 0 && cutTheTile)
            {

                print("Actual tile span width" + ActualTileSpanWidth);
                currentTilePrefab.isGrabbable = false;
                if (currentTilePrefab.tilesUnderneath.Count > 1)
                {
                    arealeft = tileSpanWidthBefore;
                }
                else
                {
                    arealeft = TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().areaLeftByTile - 0.37f;
                }
                float percentageToCut = (currentTilePrefab.tileSize - arealeft) / currentTilePrefab.tileSize;
                print("Percentage to cut" + percentageToCut);
                if (!rightToLeft)
                {
                    currentTilePrefab.removeErrorPLacements();
                    currentTilePrefab.MoveTheCutPosition(percentageToCut - 0.5f);
                    StartCoroutine(cutTheLastTile(percentageToCut, currentTilePrefab));
                    currentTilePrefab.isLastTile = true;
                    currentTilePrefab.tileSize = currentTilePrefab.tileSize * (1 - percentageToCut);
                }
                else
                {
                    currentTilePrefab.removeErrorPLacements();
                    currentTilePrefab.MoveTheCutPosition(-(percentageToCut - 0.5f));
                    StartCoroutine(cutTheLastTile(percentageToCut, currentTilePrefab));
                    currentTilePrefab.isLastTile = true;
                    currentTilePrefab.tileSize = currentTilePrefab.tileSize * (1 - percentageToCut);
                }
                // cutter.transform.SetParent(currentTilePrefab.cutPosition.transform);
                // cutter.MoveTheCutter(currentTilePrefab.cutPosition.Top.transform, currentTilePrefab.cutPosition.Bottom.transform);
                linesOfTileTobePlaced++;
                BoardPosition = boardPositionsToSnap[linesOfTileTobePlaced - 1];
            }
            else
            {
                if (!currentTilePrefab.isStarter)
                {
                    Destroy(currentTilePrefab.actualTile.GetComponent<Sliceable>());
                }
            }
            if (linesOfTileTobePlaced == markerCube.GetComponent<WhiteboardMarker>().whiteboard.numberOfLinesOftileTobeMade + 2)
            {
                statisticsManager.SaveStatistics(tilesPickedUp, TilesPlaced);
                statisticsManager.ShowStatsTable();
            }
            currentTilePrefab.yesbuttonPressed = true;

            // placementPrompt.gameObject.SetActive(true);
            if (TilesPlaced.Count > 1)
            {
                if (rightToLeft)
                {
                    if (TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().rowNumber != currentTilePrefab.rowNumber)
                    {
                        ActualTileSpanWidth += currentTilePrefab.tileSize;
                    }
                    else
                    {
                        rightEnd = TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().SideEdgeLeft.gameObject;
                        leftEnd = currentTilePrefab.SideEdgeLeft.gameObject;

                        ActualTileSpanWidth += Vector3.Distance(rightEnd.transform.position, leftEnd.transform.position) * 39.37f;
                    }
                }
                else
                {
                    if (TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().rowNumber != currentTilePrefab.rowNumber)
                    {
                        ActualTileSpanWidth += currentTilePrefab.tileSize;
                    }
                    else
                    {

                        rightEnd = TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().SideEdgeRight.gameObject;
                        leftEnd = currentTilePrefab.SideEdgeRight.gameObject;
                        ActualTileSpanWidth += Vector3.Distance(rightEnd.transform.position, leftEnd.transform.position) * 39.37f;
                    }
                }

            }
            else
            {
                ActualTileSpanWidth += currentTilePrefab.tileSize;
            }
            int n = starterColliders.IndexOf(currentTileRegion.GetComponent<BoxCollider>());
            // print("Change starter tiles" + num + " " + starterColliders.Count);
            currentTileRegion.gameObject.GetComponent<TileDropCollisionCheck>().isStarterRegion = false;
            // currentTileRegion.gameObject.GetComponent<TileDropCollisionCheck>().isNormalRegion = ;
            if (n >= starterColliders.Count - 1 && currentTilePrefab.isStarter)
            {
                actualTileWidthLeft = ActualTileSpanWidth;
                print("Actual width made" + actualTileWidthLeft);
                ActualTileSpanWidth = 0;
            }


            print("Actual tile span width" + ActualTileSpanWidth);

        }


        void ShowExposure()
        {
            exposureMeasurement.gameObject.SetActive(true);
            StartCoroutine(exposureMeasurement.ShowTileExposure(5));
            AudioHandler.Instance.DoBeforeSound -= ShowExposure;
        }
        void HideExposure()
        {
            exposureMeasurement.gameObject.SetActive(false);
            AudioHandler.Instance.DoAfterSound -= HideExposure;
        }


        IEnumerator cutTheLastTile(float percentageCut, TileObject tile)
        {

            cutter.gameObject.SetActive(true);
            Transform initialTransform = tile.transform;

            tile.transform.DOMove(new Vector3(initialTransform.position.x, initialTransform.position.y + 0.3f, initialTransform.position.z), 2);
            tile.transform.DOLocalRotate(new Vector3(tile.transform.rotation.x, tile.transform.rotation.y, tile.transform.rotation.z - 180), 2);
            yield return new WaitForSeconds(2);

            cutter.transform.SetParent(tile.cutPosition.transform);
            cutter.MoveTheCutter(tile.cutPosition.Top.transform, tile.cutPosition.Bottom.transform);
            yield return new WaitForSeconds(1.1f);

            cutter.gameObject.SetActive(false);
            tile.transform.DORotate(new Vector3(-45, initialTransform.transform.rotation.y, initialTransform.transform.rotation.z), 2);
            tile.actualTile.gameObject.SetActive(true);
            tile.transform.localScale = new Vector3(tile.transform.localScale.x * (1 - percentageCut), tile.transform.localScale.y, tile.transform.localScale.z);
            yield return new WaitForSeconds(2.6f);
            tile.SnapTileAccordingToKeyway();
            Destroy(tile.actualTile.GetComponent<Sliceable>());
            if (!rightToLeft)
            {
                // print("showing measurements right to left");
                tile.SpawnTileMeasurements(tile.SideEdgeLeft, "1\" rake overhang", Color.green, 0.03f, false);
                tile.PlaceTheMeasurementTag(false, new Vector3(0.03f, 0, 0.55f), new Vector3(0.12f * (1 + percentageCut), 0, 0), tile.SideEdgeLeft);
                tile.SpawnTileMeasurements(tile.SideEdgeRight.transform, "3/8\" keyway spacing", Color.green, 0.5f, false);
                tile.PlaceTheMeasurementTag(false, new Vector3(-0.02f, 0, 0.55f), new Vector3(0.1f * (1 + percentageCut), 0, 0), tile.SideEdgeRight);
            }
            else
            {
                // print("showing measurements left to right");
                tile.SpawnTileMeasurements(tile.SideEdgeRight, "1\" rake overhang", Color.green, 0.03f, false);
                tile.PlaceTheMeasurementTag(false, new Vector3(-0.03f, 0, 0.55f), new Vector3(-0.12f * (1 + percentageCut), 0, 0), tile.SideEdgeRight);
                tile.SpawnTileMeasurements(tile.SideEdgeLeft.transform, "3/8\" keyway spacing", Color.green, 0.5f, false);
                tile.PlaceTheMeasurementTag(false, new Vector3(-0.02f, 0, 0.55f), new Vector3(0.1f * (1 + percentageCut), 0, 0), tile.SideEdgeLeft);
            }
        }


        public TextMeshProUGUI distanceLog;

        public void SnapBackCuttedTile()
        {

            GameObject cuttedTile = cutter.cutParts[1];
            if (cuttedTile)
            {
                Destroy(cuttedTile);
            }

        }




        // Update is called once per frame
        void SnapTilesAndShowErrors()
        {
            if (currentTilePrefab.isValidTile && !currentTilePrefab.checkKeywayFlag && !currentTilePrefab.yesbuttonPressed)
            {

                if (starterTilesPlaced)
                {
                    if (isFirstShakePlaced)
                    {
                        if (currentTilePrefab.CalculateSidelapCheck())
                        {
                            print("In the sidelap check true");
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
                            print("In the sidelap check true");

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
                        print("In the sidelap check true");

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
        bool callonceEnableTile = true;
        bool callOnceAfterIntro = true;
        void Update()
        {

            if (callOnceAfterIntro)
            {
                if (AudioHandler.Instance.introductionDone)
                {
                    markerCube.SetActive(true);
                    callOnceAfterIntro = false;
                }
            }

            ///////////////////Show directional arrows///////////////////////
            if (TilesPlaced.Count > 1)
            {
                TilesPlaced[TilesPlaced.Count - 2].GetComponent<TileObject>().DisableAllArrows();
            }

            if (TilesPlaced.Count != 0)
            {
                if (TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().isStarter)
                {
                    if (TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().isFirstBoltPlaced && TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().isSecondBoltPlaced)
                    {

                        int num = TilesPlaced.Count - 1;

                        if (num < starterColliders.Count - 1)
                        {

                            if (rightToLeft)
                            {
                                TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().leftArrow.gameObject.SetActive(true);
                            }
                            else
                            {
                                TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().rightArrow.gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            TilesPlaced[0].GetComponent<TileObject>().upArrow.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {

                    if (TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().isFirstBoltPlaced && TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().isSecondBoltPlaced)
                    {

                        if (tileSpanWidth < 0)
                        {
                            TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().upArrow.gameObject.SetActive(true);
                        }
                        else
                        {
                            if (rightToLeft)
                            {
                                TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().leftArrow.gameObject.SetActive(true);
                            }
                            else
                            {
                                TilesPlaced[TilesPlaced.Count - 1].GetComponent<TileObject>().rightArrow.gameObject.SetActive(true);

                            }
                        }
                    }
                }
            }



            if (isTilePicked)
            {
                // print("Running function");
                if (currentTilePrefab)
                {
                    SnapTilesAndShowErrors();
                }
            }


            if (!markerCube.GetComponent<WhiteboardMarker>().isLineDrawnForStarter)
            {
                // ShowHideTiles(true);
                EnableDisableTileGrab(false, "Make a chalk line", 3, Color.red);
            }
            else
            {
                if (callonceEnableTile)
                {
                    EnableDisableTileGrab(true, "", 3, Color.white);
                    callonceEnableTile = false;
                }
            }

            if (currentTilePrefab && currentTilePrefab.isFirstBoltPlaced && currentTilePrefab.isSecondBoltPlaced)
            {
                // Should not be called if we are placing the last starter
                if (currentTilePrefab.isStarter && markerCube.GetComponent<WhiteboardMarker>().isExposureButtonActive)
                {
                    currentTilePrefab = null;

                }
                else
                {
                    EnableDisableTileGrab(true, "", 3, Color.white);
                    StartCoroutine(aODPanel.WriteTextForTime(1, Color.green, "Fastened!"));
                    currentTilePrefab = null;
                }
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
