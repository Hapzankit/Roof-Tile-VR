using System;
using System.Collections.Generic;
using System.Linq;
using com.marufhow.meshslicer.core;
using TMPro;
// using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


namespace RoofTileVR
{
    public class TileObject : MonoBehaviour
    {
        public bool isStarter;
        public GameObject tileCanvasUI;
        public GameObject DistanceErrorCubeRL;
        public GameObject DistanceErrorCubeTB;
        public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable Interactable;
        public Button YesButton;
        public TextMeshProUGUI tileNameText;

        private TileCasting spawner;

        [SerializeField] private Color failColor;
        [SerializeField] private Color passColor;


        [Header("Edge points")]
        [SerializeField] private Transform sideEdgeRight;
        [SerializeField] private Transform sideEdgeLeft;
        [SerializeField] private Transform sideEdgeBottom;
        [SerializeField] private Transform sideEdgeTop;

        [SerializeField] private Vector3 effectSideEdgeScale = new Vector3(0.1f, 1.20f, 1f);
        [SerializeField] private Vector3 canvasOffset;
        public Vector3 OffsetUI => canvasOffset;
        public Transform ConfirmTileUIRoot;
        public Transform BottomOverhangLogUIRoot;
        public GameObject CorrectTileIndicator;
        public List<Transform> BoltPlaceHolders;
        public bool isFirstBoltPlaced = false;
        public bool isSecondBoltPlaced = false;

        public float tileSize;
        public bool isPlaced = false;
        public bool isInStarterRegion; // Tracks if the tile is currently in a starter region

        public int rowNumber = 0;

        /// ///////////////////////////////Tile bending/////////////////////////////////////
        public Bend tileBend;
        /////////////////////Tile Cutting/////////////////
        // MHCutter mHCutter;
        public GameObject actualTile;
        public CutPosition cutPosition;

        public GameObject TileCutParent;


        /// /////////////////////////////TIle Direction///////////////////////
        public DirectionalArrow upArrow;
        public DirectionalArrow downArrow;
        public DirectionalArrow leftArrow;
        public DirectionalArrow rightArrow;


        ////////////////////////////////////For Statistics///////////////////////////////
        public int InCorrectoverHangs = 0;
        public int InCorrectKeywaySpaces = 0;
        public int IncorrectExposure = 0;
        public int IncorrectSidelap;
        public bool isBothFastnersplaced = false;
        public bool isPlacedCorrectlyAfterConfirmedPlacement = false;
        public TMP_Text distanceChecks;
        public List<GameObject> measurements;
        public List<GameObject> Errormeasurements;

        public GameObject MeasurementTags;

        public bool isGrabbable = true;


        public bool checkKeywayFlag = false;



        /// //////////////////////SideLap Checks///////////////////////////       
        // Use when tile is placed (other tiles will be placed on top of this)
        public float areaCoveredByTileAbove;
        public float areaLeftByTileAbove;
        // Use when tile is being placed
        public List<TileObject> tilesUnderneath;

        public bool isLastTile = false;


        // Example method for resetting the flag when leaving regions
        public void ResetStarterRegionStatus()
        {
            isInStarterRegion = false;
        }

        public Vector3 EffectSideEdgeScale
        {
            get
            {
                return effectSideEdgeScale;
            }
        }

        public Transform SideEdgeRight
        {
            get => sideEdgeRight;
        }
        public Transform SideEdgeLeft
        {
            get => sideEdgeLeft;
        }
        public Transform SideEdgeTop
        {
            get => sideEdgeTop;
        }
        public Transform SideEdgeBottom
        {
            get => sideEdgeBottom;
        }



        private void Start()
        {
            // mHCutter = FindObjectOfType<MHCutter>();
            spawner = FindObjectOfType<TileCasting>();
            YesButton?.onClick.AddListener(YesButtonPressed);
            areaCoveredByTileAbove = 0;
            areaLeftByTileAbove = tileSize;
            BoltPlaceHolders[0].gameObject.SetActive(false);
            BoltPlaceHolders[1].gameObject.SetActive(false);
            TileCutParent = actualTile.transform.parent.gameObject;
            // DrawLine();
            //tileCanvasUI.gameObject.SetActive(false);

            upArrow.gameObject.SetActive(false);
            downArrow.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            // CutTheTile(BoltPlaceHolders[0].transform.position);
            // CutTheTile();
        }

        // public void CutTheTile(float percentage)
        // {
        //     mHCutter.Cut(actualTile.gameObject,percentage);
        // }

        public void ShowPlacementPrompt()
        {
            tileCanvasUI.gameObject.SetActive(true);
        }

        public void YesButtonPressed()
        {
            spawner.SetActiveTileStateToPlaced(1);
            // DisableInteraction();
            // spawner.TileSelectText("Tile Placed! Pick New Tile");
            tileCanvasUI.gameObject.SetActive(true);

        }

        public void SetName(string txt)
        {
            tileNameText.text = txt;
        }

        public void EnableInteraction()
        {
            Interactable.enabled = true;
        }

        public void DisableInteraction()
        {
            Interactable.enabled = false;
            this.GetComponent<XRGrabInteractable>().enabled = false;
            // Destroy(this.gameObject);
        }
        public void RemoveIndications()
        {
            DistanceErrorCubeRL.gameObject.SetActive(false);
            DistanceErrorCubeTB.gameObject.SetActive(false);
            CorrectTileIndicator.gameObject.SetActive(false);
            bottomTopTextError.text = "";
            LeftRightTextError.text = "";


        }
        public string messageToWrite;
        public float timeToShow;
        public Color AODcolor;
        public void OnTilePicked()
        {

            // spawner.TileSelectText("Tile Picked");
            if (isGrabbable)
            {
                this.GetComponent<XRGrabInteractable>().interactionLayers = InteractionLayerMask.GetMask("Default");
                removeErrorPLacements();
                spawner.OnTilePick();
                spawner.currentTilePrefab = this;
                this.transform.rotation = Quaternion.Euler(-45, 0, 0);
                spawner.currentTileWidth = tileSize;
                DistanceErrorCubeRL.gameObject.SetActive(false);
                DistanceErrorCubeTB.gameObject.SetActive(false);
                CorrectTileIndicator.gameObject.SetActive(false);
                this.GetComponent<Rigidbody>().useGravity = true;
                this.GetComponent<Rigidbody>().isKinematic = false;
                if (!spawner.tilesPickedUp.Contains(gameObject)) // Check if the object is already in the list
                {
                    spawner.tilesPickedUp.Add(gameObject);
                }
                tileNameText.text = "";

            }
            else
            {
                this.GetComponent<XRGrabInteractable>().interactionLayers = InteractionLayerMask.GetMask("Nothing");
                StartCoroutine(spawner.aODPanel.WriteTextForTime(timeToShow, AODcolor, messageToWrite));
            }


        }

        public void OnTileDropped()
        {

            print("Tile Dropped" + isTileAbove + isValidTile + isPlaced);



            if (isTileAbove && !isPlaced)
            {
                this.GetComponent<Rigidbody>().isKinematic = true;
                this.GetComponent<Rigidbody>().useGravity = false;
                this.transform.SetParent(spawner.BoardPosition);
                if (isStarter)
                {

                    this.transform.localPosition = new Vector3(0, 0, this.transform.localPosition.z);

                }
                else
                {
                    //Convert from inches to unity measurements
                    print(spawner.exposureConvertedToUnityfromInches[spawner.linesOfTileTobePlaced - 1] + " Row number");
                    this.transform.localPosition = new Vector3((11 - spawner.exposureConvertedToUnityfromInches[spawner.linesOfTileTobePlaced - 1] / 2) * 0.0254f + (0.07f * spawner.exposureConvertedToUnityfromInches[spawner.linesOfTileTobePlaced - 1] / 10), 0, this.transform.localPosition.z);
                }

                ShowErrorsWhenDropped();
            }
            else
            {
                this.GetComponent<Rigidbody>().isKinematic = false;
                this.GetComponent<Rigidbody>().useGravity = true;
            }
            if (isPlaced)
            {
                this.GetComponent<Rigidbody>().isKinematic = true;
                this.GetComponent<Rigidbody>().useGravity = false;
            }
            StoreStatistics();



        }






        float Sidelap = 1.5f;
        float areaToBeCovered; float areaLeftByTile; int tileNum; int numberoftilesunderneath;
        public void ReduceTileArea()
        {
            tilesUnderneath[tileNum].areaCoveredByTileAbove = areaToBeCovered;
            tilesUnderneath[tileNum].areaLeftByTileAbove = areaLeftByTile;
        }


        public bool CalculateSidelapCheck()

        {
            print("Calculate Side Lap");

            if (isStarter) return true;

            //GEt all the tiles underneath this tile;
            tilesUnderneath.Clear();
            float checkDepth = 0.04f;  // Adjust depth to the needed value
            Vector3 planeSize = GetComponent<Renderer>().bounds.size;
            Vector3 boxSize = new Vector3(planeSize.x, checkDepth, planeSize.z);
            Vector3 boxCenter = transform.position + transform.TransformDirection(new Vector3(0, -checkDepth, 0));

            // Align the box with the object's local orientation
            Quaternion boxOrientation = transform.rotation;

            Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2, boxOrientation, LayerMask.GetMask("Default"));
            foreach (Collider collider in colliders)
            {
                if (collider.GetComponent<TileObject>())
                {
                    Debug.Log("Detected object underneath: " + collider.gameObject.name);
                    if (collider.GetComponent<TileObject>().isStarter && rowNumber == 1)
                    {
                        tilesUnderneath.Add(collider.GetComponent<TileObject>());
                    }
                    else if (collider.GetComponent<TileObject>().rowNumber == rowNumber - 1 && !collider.GetComponent<TileObject>().isStarter)
                    {
                        tilesUnderneath.Add(collider.GetComponent<TileObject>());
                    }
                }
            }
            if (tilesUnderneath.Count == 0) return false;

            // if 1 tile is under
            if (tilesUnderneath.Count == 1)
            {
                numberoftilesunderneath = 1;
                // print("1 number of tile");
                if (tilesUnderneath[0].areaCoveredByTileAbove == 0)
                {
                    // print("1 number of tile and area covered 0");
                    if (tilesUnderneath[0].tileSize > tileSize)
                    {
                        print("TIle size correct");
                        areaToBeCovered = tileSize;
                        areaLeftByTile = tilesUnderneath[0].tileSize - tileSize;
                        print("area left by tile " + areaLeftByTile);
                        tileNum = 0;
                        CorrectTileIndicator.SetActive(false);
                        return true;
                    }
                    else
                    {
                        // print("TIle size incorrect");
                        spawner.WriteOnHandMenu("Incorrect sidelap (tile size doesnt fit the sidelap criteria choose tile of different size).");
                        IncorrectSidelap++;
                        // CorrectTileIndicator.SetActive(true);
                        return false;
                    }
                }
                else
                {
                    float areaLeft = tileSize - tilesUnderneath[0].areaLeftByTileAbove;
                    areaLeft = Math.Abs(areaLeft);
                    print("Area left " + areaLeft);
                    if (areaLeft < Sidelap)
                    {
                        // print("TIle size incorrect");
                        spawner.WriteOnHandMenu("Incorrect sidelap (tile size doesnt fit the sidelap criteria choose tile of different size).");
                        IncorrectSidelap++;
                        // CorrectTileIndicator.SetActive(true);
                        return false;
                    }
                    else
                    {
                        print("TIle size correct");

                        areaToBeCovered = tilesUnderneath[0].areaCoveredByTileAbove + tileSize;
                        areaLeftByTile = areaLeft;
                        // print("tilesUnderneath[0].areaCoveredByTileAbove "+tilesUnderneath[0].areaCoveredByTileAbove+" "+"tile size "+ tileSize + "area left"+areaLeftByTile);
                        tileNum = 0;
                        CorrectTileIndicator.SetActive(false);
                        return true;
                    }
                }
            }

            // if 2 are there
            if (tilesUnderneath.Count == 2)
            {
                numberoftilesunderneath = 2;
                // bool isTileCovered = true;

                tilesUnderneath = tilesUnderneath.OrderBy(go => ExtractNumber(go.name)).ToList();



                if (spawner.rightToLeft)
                {


                    //right to left
                    if (tilesUnderneath[0].isStarter)
                    {
                        Vector3 localLeft = -this.transform.right;


                        Vector3 distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;
                        Vector3 distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[1].sideEdgeRight.transform.position;


                        float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                        float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                        // If you want the magnitude of this projection as a positive number
                        leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                        leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);
                        print(" Going right to left Distances:-" + leftwardDistanceLocal1 * 39.37f + " " + leftwardDistanceLocal2 * 39.37f + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                        spawner.WriteOnHandMenu("Sidelap from left tile " + (float)Math.Round(leftwardDistanceLocal2 * 39.37f, 2) + " Sidelap from right tile " + (float)Math.Round(leftwardDistanceLocal1 * 39.37f, 2) + " Sidelap must be atleast 1.5 inches on both sides");
                        // print("Distances:-" + distanceMeasured1 + " " + distanceMeasured2 + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                        if (leftwardDistanceLocal1 * 39.37 > Sidelap && leftwardDistanceLocal2 * 39.37 > Sidelap)
                        {



                            areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                            areaLeftByTile = tilesUnderneath[1].tileSize - leftwardDistanceLocal2 * 39.37f;
                            tileNum = 1;
                            CorrectTileIndicator.SetActive(false);
                            return true;

                        }
                        else
                        {
                            IncorrectSidelap++;
                            // CorrectTileIndicator.SetActive(true);
                            return false;
                        }
                    }
                    else
                    {
                        Vector3 localLeft = -this.transform.right;


                        Vector3 distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[1].sideEdgeLeft.transform.position;
                        Vector3 distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;


                        float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                        float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                        // If you want the magnitude of this projection as a positive number
                        leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                        leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);
                        print(" Going right to left Distances:-" + leftwardDistanceLocal1 * 39.37f + " " + leftwardDistanceLocal2 * 39.37f + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                        spawner.WriteOnHandMenu("Sidelap from left tile " + (float)Math.Round(leftwardDistanceLocal2 * 39.37f, 2) + " Sidelap from right tile " + (float)Math.Round(leftwardDistanceLocal1 * 39.37f, 2) + " Sidelap must be atleast 1.5 inches on both sides");
                        // print("Distances:-" + distanceMeasured1 + " " + distanceMeasured2 + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                        if (leftwardDistanceLocal1 * 39.37 > Sidelap && leftwardDistanceLocal2 * 39.37 > Sidelap)
                        {



                            areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                            areaLeftByTile = tilesUnderneath[0].tileSize - leftwardDistanceLocal2 * 39.37f;
                            tileNum = 0;
                            CorrectTileIndicator.SetActive(false);
                            return true;

                        }
                        else
                        {
                            IncorrectSidelap++;
                            // CorrectTileIndicator.SetActive(true);
                            return false;
                        }
                    }

                }
                else
                {
                    //left to right
                    // Correct the local left direction
                    Vector3 localLeft = -this.transform.right;

                    // Calculate the vector distances
                    Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[1].sideEdgeRight.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;

                    // Project the world direction vector onto the local left vector
                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);



                    print("Going left to right Distances:-" + leftwardDistanceLocal1 * 39.37f + " " + leftwardDistanceLocal2 * 39.37f + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                    spawner.WriteOnHandMenu("Sidelap from left tile " + (float)Math.Round(leftwardDistanceLocal1 * 39.37f, 2) + " Sidelap from right tile " + (float)Math.Round(leftwardDistanceLocal2 * 39.37f, 2) + " Sidelap must be atleast 1.5 inches on both sides");
                    if (leftwardDistanceLocal1 * 39.37 > Sidelap && leftwardDistanceLocal2 * 39.37 > Sidelap)
                    {
                        areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                        areaLeftByTile = tilesUnderneath[0].tileSize - leftwardDistanceLocal2 * 39.37f;
                        tileNum = 0;
                        CorrectTileIndicator.SetActive(false);
                        return true;
                    }
                    else
                    {
                        IncorrectSidelap++;
                        // CorrectTileIndicator.SetActive(true);
                        return false;
                    }
                }



            }

            if (tilesUnderneath.Count == 3)
            {
                numberoftilesunderneath = 3;
                // bool isTileCovered = true;
                tilesUnderneath = tilesUnderneath.OrderBy(go => ExtractNumber(go.name)).ToList();
                print("Tiles in order:- " + tilesUnderneath[0].name + " " + tilesUnderneath[1].name);
                if (spawner.rightToLeft)
                {
                    //right to left
                    Vector3 localLeft = -this.transform.right;


                    Vector3 distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[2].sideEdgeLeft.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;


                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);
                    print(" Going right to left Distances:-" + leftwardDistanceLocal1 * 39.37f + " " + leftwardDistanceLocal2 * 39.37f + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                    spawner.WriteOnHandMenu("Sidelap from left tile " + (float)Math.Round(leftwardDistanceLocal2 * 39.37f, 2) + " Sidelap from right tile " + (float)Math.Round(leftwardDistanceLocal1 * 39.37f, 2) + " Sidelap must be atleast 1.5 inches on both sides");
                    // print("Distances:-" + distanceMeasured1 + " " + distanceMeasured2 + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                    if (leftwardDistanceLocal1 * 39.37 > Sidelap && leftwardDistanceLocal2 * 39.37 > Sidelap)
                    {
                        areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                        areaLeftByTile = tilesUnderneath[0].tileSize - leftwardDistanceLocal2 * 39.37f;
                        tileNum = 0;
                        CorrectTileIndicator.SetActive(false);
                        return true;

                    }
                    else
                    {
                        IncorrectSidelap++;
                        // CorrectTileIndicator.SetActive(true);
                        return false;
                    }
                }
                else
                {
                    //left to right
                    // Correct the local left direction
                    Vector3 localLeft = -this.transform.right;

                    // Calculate the vector distances
                    Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[2].sideEdgeRight.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;

                    // Project the world direction vector onto the local left vector
                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);

                    // Draw lines to visualize the directions and magnitudes



                    print("Going left to right Distances:-" + leftwardDistanceLocal1 * 39.37f + " " + leftwardDistanceLocal2 * 39.37f + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[2].name);
                    spawner.WriteOnHandMenu("Sidelap from left tile " + (float)Math.Round(leftwardDistanceLocal1 * 39.37f, 2) + " Sidelap from right tile " + (float)Math.Round(leftwardDistanceLocal2 * 39.37f, 2) + " Sidelap must be atleast 1.5 inches on both sides");
                    if (leftwardDistanceLocal1 * 39.37 > Sidelap && leftwardDistanceLocal2 * 39.37 > Sidelap)
                    {

                        areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                        areaLeftByTile = tilesUnderneath[0].tileSize - leftwardDistanceLocal2 * 39.37f;
                        tileNum = 0;
                        CorrectTileIndicator.SetActive(false);
                        return true;
                    }
                    else
                    {
                        IncorrectSidelap++;
                        // CorrectTileIndicator.SetActive(true);
                        return false;
                    }
                }

            }
            else
            {
                // CorrectTileIndicator.SetActive(true);
                return false;
            }
            // return false;

            //subtract its length to areaLeftByTileAbove
        }
        int ExtractNumber(string name)
        {
            string numberString = new string(name.Where(char.IsDigit).ToArray());
            return int.Parse(numberString);
        }

        public void removeErrorPLacements()
        {
            if (measurements.Count != 0)
            {
                foreach (var measure in measurements)
                {
                    Destroy(measure.gameObject);
                }
            }

        }

        public void DestroyErrors()
        {
            if (Errormeasurements.Count != 0)
            {
                foreach (var measure in Errormeasurements)
                {
                    Destroy(measure.gameObject);
                }
            }
        }
        bool callOnceBend = true;

        public void DisableAllArrows()
        {
            upArrow.gameObject.SetActive(false);
            downArrow.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
        }
        void Update()
        {

            if (isFirstBoltPlaced && isSecondBoltPlaced)
            {


                foreach (var measure in measurements)
                {
                    if (measure)
                    {
                        measure.gameObject.SetActive(false);
                    }
                    this.GetComponent<XRGrabInteractable>().enabled = false;
                }

                if (callOnceBend && !isStarter)
                {
                    tileBend.BendTheTile(rowNumber);
                    foreach (Transform boltplaceholder in BoltPlaceHolders)
                    {
                        boltplaceholder.localPosition = new Vector3(boltplaceholder.localPosition.x, boltplaceholder.localPosition.y - 0.15f * rowNumber, boltplaceholder.localPosition.z);
                    }
                    callOnceBend = false;
                }
            }
            if (!isPlaced && !isStarter)
            {
                rowNumber = spawner.linesOfTileTobePlaced;
            }
            else
            {
                this.transform.SetParent(null);
            }



        }

        public void StoreStatistics()
        {
            Vector3 rightDirection = this.transform.right;//+VE means tile is on left side of the target
            Vector3 forwardDirection = this.transform.forward;//+VE means tile is above
            if (isStarter)
            {
                if (spawner.currentTileRegion && isValidTile && isTileAbove)
                {
                    // this.transform.position = spawner.currentTileRegion.transform.position;
                    if (Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 > 2.1f)
                    {
                        Vector3 distanceFromPoint = this.transform.position - spawner.currentTileRegion.transform.position;


                        float sideWaysDistance = Math.Abs(Vector3.Dot(distanceFromPoint, rightDirection) * 39.37f);
                        float verticalDistance = Math.Abs(Vector3.Dot(distanceFromPoint, forwardDirection) * 39.37f);
                        print(" Distance measured for starters" + sideWaysDistance + " " + verticalDistance);
                        if (spawner.TilesPlaced.Count == 0 || spawner.TilesPlaced.Count == spawner.starterColliders.Count - 1)
                        {
                            //Placing First Tile starter 
                            if (sideWaysDistance > 1)
                            {
                                InCorrectoverHangs++;
                                InCorrectKeywaySpaces++;
                                // print("Showing stats:- incorrect overhangs and keyway for starter");
                            }
                            else if (sideWaysDistance >= 0f && sideWaysDistance <= 1f)
                            {
                                spawner.statisticsManager.CorrectOverhangs++;
                                spawner.statisticsManager.CorrectKeyway++;
                                // print("Showing stats:- correct overhangs and keyway for starter");

                            }
                            if (verticalDistance > 1)
                            {
                                InCorrectoverHangs++;
                                // print("Showing stats:- incorrect overhangs  for starter");
                            }
                            else if (verticalDistance >= 0f && verticalDistance <= 1f)
                            {
                                spawner.statisticsManager.CorrectOverhangs++;
                                // print("Showing stats:- correct overhangs  for starter");
                            }
                        }
                        else
                        {
                            if (verticalDistance > 1)
                            {
                                InCorrectoverHangs++;
                                // print("Showing stats:- incorrect overhangs for starter");
                            }
                            else if (verticalDistance >= 0f && verticalDistance <= 1f)
                            {
                                spawner.statisticsManager.CorrectOverhangs++;
                                // print("Showing stats:- correct overhangs for starter");
                            }
                            if (sideWaysDistance > 1)
                            {
                                InCorrectKeywaySpaces++;
                                // print("Showing stats:- incorrect keyway for starter");
                            }
                            else if (sideWaysDistance >= 0f && sideWaysDistance <= 1f)
                            {
                                spawner.statisticsManager.CorrectKeyway++;
                                // print("Showing stats:- correct keyway for starter");
                            }
                        }


                    }


                }
            }
            else
            {


                // For shake tiles
                GameObject objectToCheck = spawner.TilesPlaced[0];
                GameObject objectToCheckFrom;
                if (!spawner.isFirstShakePlaced)
                {
                    // first shake errors
                    objectToCheckFrom = this.sideEdgeRight.gameObject;
                    // objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
                    if (objectToCheck.GetComponent<TileObject>().sideEdgeRight && isValidTile /*true*/)
                    {
                        // this.transform.position = spawner.currentTileRegion.transform.position;
                        if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 > 1f)
                        {
                            Vector3 distanceFromPoint = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                            float sideWaysDistance = Math.Abs(Vector3.Dot(distanceFromPoint, rightDirection) * 39.37f);
                            float verticalDistance = Math.Abs(Vector3.Dot(distanceFromPoint, forwardDirection) * 39.37f);
                            print(" Distance measured for shakes" + sideWaysDistance + " " + verticalDistance);
                            if (verticalDistance > 1)
                            {
                                IncorrectExposure++;
                                // print("Showing stats:- incorrect exposure for normal shake");
                            }
                            else if (verticalDistance >= 0f && verticalDistance <= 1f)
                            {
                                spawner.statisticsManager.CorrectExposure++;
                                // print("Showing stats:- correct exposure for normal shake");
                            }
                            if (sideWaysDistance > 1)
                            {
                                InCorrectoverHangs++;
                                // print("Showing stats:- incorrect overhang for normal shake");
                            }
                            else if (sideWaysDistance >= 0f && sideWaysDistance <= 1f)
                            {
                                spawner.statisticsManager.CorrectOverhangs++;
                                // print("Showing stats:- correct overhang for normal shake");
                            }

                        }
                    }
                }
                else
                {
                    // other shake errors

                    GameObject objectToCheckNormal;
                    GameObject objectToCheckFromNormal;
                    print("Checking keyway distance");
                    objectToCheckNormal = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.gameObject;
                    float distanceInInches;
                    if (spawner.tileSpanWidth <= 0)
                    {
                        objectToCheckFromNormal = this.sideEdgeLeft.gameObject;
                        distanceToCheckAccordingToExposure = spawner.overlapSpanAccordingtoExposure * 0.0254f;
                        distanceInInches = spawner.overlapSpanAccordingtoExposure;
                        print("A row of tiles placed now checking from the last tile to the above" + distanceToCheckAccordingToExposure);
                    }
                    else
                    {
                        objectToCheckFromNormal = this.sideEdgeRight.gameObject;
                        distanceToCheckAccordingToExposure = 0.25f * 0.0254f * 2;
                        distanceInInches = 2f;
                    }

                    if (spawner.reverseTheLine)
                    {
                        print("Going right to left");
                        objectToCheckNormal = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeRight.gameObject;

                        if (spawner.tileSpanWidth <= 0)
                        {
                            objectToCheckFromNormal = this.sideEdgeRight.gameObject;
                            distanceToCheckAccordingToExposure = spawner.overlapSpanAccordingtoExposure * 0.0254f;
                            distanceInInches = spawner.overlapSpanAccordingtoExposure;
                            print("A row of tiles placed now checking from the last tile to the above" + distanceToCheckAccordingToExposure);
                        }
                        else
                        {
                            objectToCheckFromNormal = this.sideEdgeLeft.gameObject;
                            distanceToCheckAccordingToExposure = 0.18f * 0.0254f;
                            distanceInInches = 2f;
                        }

                    }

                    if (Vector3.Distance(objectToCheckFromNormal.transform.position, objectToCheckNormal
                    .transform.position) * 39.37 > distanceInInches)
                    {
                        Vector3 distanceFromPoint = objectToCheckFromNormal.transform.position - objectToCheckNormal.transform.position;
                        float sideWaysDistance = Math.Abs(Vector3.Dot(distanceFromPoint, rightDirection) * 39.37f);
                        float verticalDistance = Math.Abs(Vector3.Dot(distanceFromPoint, forwardDirection) * 39.37f);
                        if (sideWaysDistance > 1)
                        {
                            InCorrectKeywaySpaces++;
                            // print("Showing stats:- incorrect keyway for normal shake");
                        }
                        else if (sideWaysDistance >= 0f && sideWaysDistance <= 1f)
                        {
                            spawner.statisticsManager.CorrectKeyway++;
                            // print("Showing stats:- correct keyway for normal shake");
                        }
                        if (verticalDistance > 1)
                        {
                            IncorrectExposure++;
                            // print("Showing stats:- incorrect exposure for normal shake");
                        }
                        else if (verticalDistance >= 0f && verticalDistance <= 1f)
                        {
                            spawner.statisticsManager.CorrectExposure++;
                            // print("Showing stats:- correct exposure for normal shake");
                        }
                    }
                }
            }
        }



        public bool ShowStarterErrors()
        {
            print("Showing starter tile errors");
            if (spawner.currentTileRegion && isValidTile)
            {
                // this.transform.position = spawner.currentTileRegion.transform.position;
                if (Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 > 2.1f)
                {

                    isPlacedCorrectlyAfterConfirmedPlacement = false;


                    // DistanceErrorCube.SetActive(true);
                    Vector3 direction = this.transform.position - spawner.currentTileRegion.transform.position;

                    direction.y = 0; // Ignore the Y-axis


                    // print("Distance from place" + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) + "Direction" + direction);

                    // Determine the relative position
                    if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                    {

                        // DistanceErrorCubeRL.SetActive(true);
                        // DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                        // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile is on right by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");
                        // tileNameText.text = ((float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2)).ToString();
                        // LeftRightTextError.text = (float)Math.Round(Vector3.Distance(sideEdgeRight.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) - 6 + "\"";
                        // LeftRightTextError.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, 0.03f, sideEdgeRight.localPosition.z);


                    }
                    if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                    {
                        // Debug.Log("Target is to the Left.");
                        // DistanceErrorCubeRL.SetActive(true);
                        // DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                        // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile  is on left by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");
                        // tileNameText.text = ((float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2)).ToString();
                        // LeftRightTextError.text = (float)Math.Round(Vector3.Distance(sideEdgeLeft.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) - 6 + "\"";
                        // LeftRightTextError.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, 0.03f, sideEdgeLeft.localPosition.z);
                    }
                    if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                    {
                        // Debug.Log("Target is Above (Forward).");
                        // DistanceErrorCubeTB.SetActive(true);
                        // DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                        // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile is above by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");
                        // tileNameText.text = ((float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2)).ToString();
                        // bottomTopTextError.text = (float)Math.Round(Vector3.Distance(sideEdgeTop.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) - 6 + "\"";
                        // bottomTopTextError.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, 0.03f, sideEdgeTop.localPosition.z);
                    }
                    if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                    {
                        // Debug.Log("Target is Below (Backward).");
                        // DistanceErrorCubeTB.SetActive(true);
                        // DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                        // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile is down by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");
                        // tileNameText.text = ((float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2)).ToString();
                        // bottomTopTextError.text = (float)Math.Round(Vector3.Distance(sideEdgeBottom.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) - 6 + "\"";
                        // bottomTopTextError.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, 0.03f, sideEdgeBottom.localPosition.z);
                    }
                    return false;
                }
                else
                {
                    isPlaced = true;

                    isPlacedCorrectlyAfterConfirmedPlacement = true;
                    // CorrectTileIndicator.SetActive(true);
                    // this.transform.localPosition = new Vector3(0, 0, 0.24f);
                    this.transform.position = spawner.currentTileRegion.transform.position;
                    DistanceErrorCubeRL.SetActive(false);
                    DistanceErrorCubeTB.SetActive(false);
                    bottomTopTextError.gameObject.SetActive(false);
                    LeftRightTextError.gameObject.SetActive(false);
                    spawner.WriteOnHandMenu("Tile Placed Correctly");
                    GetComponent<Rigidbody>().isKinematic = true;
                    SpawnTileMeasurements(sideEdgeBottom, "1/2\" eave overhang", Color.green, 0.03f, false);
                    PlaceTheMeasurementTag(true, new Vector3(-0.55f, 0, 0), new Vector3(0, 0, 0.07f), sideEdgeBottom);

                    if (spawner.TilesPlaced.Count == 0)
                    {
                        //First starter tile Being Placed
                        SpawnTileMeasurements(sideEdgeRight, "1\" rake overhang", Color.green, 0.03f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.03f, 0, 0.55f), new Vector3(-0.12f, 0, 0), sideEdgeRight);

                    }
                    else
                    {
                        if (spawner.TilesPlaced.Count == spawner.starterColliderHolder.GetComponentsInChildren<BoxCollider>().Length - 1)
                        {
                            //Last starter being placed
                            SpawnTileMeasurements(sideEdgeLeft, "1\" rake overhang", Color.green, 0.03f, false);
                            PlaceTheMeasurementTag(false, new Vector3(0.03f, 0, 0.55f), new Vector3(0.12f, 0, 0), sideEdgeLeft);
                            SpawnTileMeasurements(sideEdgeRight, "3/8\" keyway spacing", Color.green, 0.03f, false);
                            PlaceTheMeasurementTag(false, new Vector3(-0.03f, 0, 0.55f), new Vector3(0.08f, 0, 0), sideEdgeRight);

                        }
                        else
                        {
                            //0.49 0.532
                            //all other starter being placed
                            SpawnTileMeasurements(sideEdgeRight, "3/8\" keyway spacing", Color.green, 0.03f, false);
                            PlaceTheMeasurementTag(false, new Vector3(-0.03f, 0, 0.55f), new Vector3(0.08f, 0, 0), sideEdgeRight);


                        }
                    }

                    return true;
                    //JUST FOR DEBUG
                    // if (!isYesPressed)
                    // {
                    //     spawner.YesButtonPressed();
                    //     isYesPressed = true;
                    // }
                }



            }
            else
            {
                return false;
            }
        }
        GameObject measureTag1;
        GameObject measureTag2;
        TMP_Text Text;
        void SpawnTileMeasurements(Transform side, string measurementToWrite, Color color, float textHeight, bool isTemporary)
        {
            Text = Instantiate(distanceChecks, this.transform);
            measureTag1 = Instantiate(MeasurementTags, this.transform);
            measureTag2 = Instantiate(MeasurementTags, this.transform);
            if (color == Color.red)
            {
                measureTag1.GetComponent<MeasureTag>().ChangeMaterial();
                measureTag2.GetComponent<MeasureTag>().ChangeMaterial();
            }

            // Add a LineRenderer for the measurement line
            LineRenderer measureLine = measureTag1.gameObject.AddComponent<LineRenderer>();
            measureLine.startWidth = 0.003f;
            measureLine.endWidth = 0.003f;
            measureLine.material = new Material(Shader.Find("Sprites/Default")); // Use a default material
            measureLine.startColor = color;
            measureLine.endColor = color;
            measureLine.positionCount = 2; // Line connects two points

            // Set up the text position and content
            Text.transform.localPosition = new Vector3(side.transform.localPosition.x, textHeight, side.transform.localPosition.z);
            Text.text = measurementToWrite;
            Text.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);

            // Store the spawned objects for later reference
            measurements.Add(Text.gameObject);
            measurements.Add(measureTag1);
            measurements.Add(measureTag2);
            if (isTemporary)
            {
                Errormeasurements.Add(Text.gameObject);
                Errormeasurements.Add(measureTag1);
                Errormeasurements.Add(measureTag2);
            }
        }

        float MeasurementsHeight = 0.02f;

        void PlaceTheMeasurementTag(bool isSideways, Vector3 edge, Vector3 distance, Transform PositionOfEdge)
        {
            if (isSideways)
            {
                measureTag1.transform.localPosition = PositionOfEdge.localPosition + edge + new Vector3(0, MeasurementsHeight, 0);
                measureTag1.transform.localRotation = Quaternion.Euler(0, -90, 0);
                measureTag2.transform.localPosition = PositionOfEdge.localPosition + edge + distance + new Vector3(0, MeasurementsHeight, 0);
                measureTag2.transform.localRotation = Quaternion.Euler(0, -90, 0);
            }
            else
            {
                measureTag1.transform.localPosition = PositionOfEdge.localPosition + edge + new Vector3(0, MeasurementsHeight, 0);
                measureTag2.transform.localPosition = PositionOfEdge.localPosition + edge + distance + new Vector3(0, MeasurementsHeight, 0);
            }

            Vector3 midpoint = (measureTag1.transform.localPosition + measureTag2.transform.localPosition) / 2;
            Text.transform.localPosition = new Vector3(midpoint.x, Text.transform.localPosition.y, midpoint.z + 0.1f);

            // Update the LineRenderer positions
            LineRenderer measureLine = measureTag1.GetComponent<LineRenderer>();
            if (measureLine != null)
            {
                measureLine.SetPosition(0, measureTag1.transform.position);
                measureLine.SetPosition(1, measureTag2.transform.position);
            }
        }


        public bool isTileAbove = true;
        public bool isValidTile = false;

        public void SetTileAboveRoof(bool isAbove, bool isTileValid, TileDropCollisionCheck regionTileDropped)
        {
            // ShowPlacementPrompt();
            // spawner.ShowPlacementPrompt();
            isTileAbove = isAbove;
            isValidTile = isTileValid;
            if (regionTileDropped.isStarterRegion)
            {
                spawner.currentTileRegion = regionTileDropped;
            }
            this.transform.rotation = Quaternion.Euler(-45, 0, 0);


            // this.transform.localPosition = new Vector3(this.transform.localPosition.x, regionTileDropped.transform.localPosition.y, this.transform.localPosition.z);
        }



        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CollisionCheck edge))
            {

            }
        }


        public float distanceToCheckAccordingToExposure;

        public TMP_Text bottomTopTextError;
        public TMP_Text LeftRightTextError;
        // void Spawn2Measurements()
        // {
        //     bottomTopTextError = Instantiate(distanceChecks, this.transform);
        //     LeftRightTextError = Instantiate(distanceChecks, this.transform);
        // }


        public bool ShowShakeTIleErrors(bool isfirstShakeTile)
        {

            // print("Showing shake tile errors");
            GameObject objectToCheck = spawner.TilesPlaced[0];
            GameObject objectToCheckFrom;
            if (isfirstShakeTile)
            {

                objectToCheckFrom = this.sideEdgeRight.gameObject;
                // objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
                if (objectToCheck.GetComponent<TileObject>().sideEdgeRight && isValidTile && isTileAbove/*true*/)
                {
                    Vector3 localdirectionup = -objectToCheckFrom.transform.forward;
                    Vector3 localdirectionright = -objectToCheckFrom.transform.right;


                    Vector3 distanceMeasured = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                    float distanceMeasuredInDirection = Mathf.Abs(Vector3.Dot(distanceMeasured, localdirectionup));
                    float distanceMeasuredInDirectionSideways = Mathf.Abs(Vector3.Dot(distanceMeasured, localdirectionright));

                    print("First shake placing " + distanceMeasuredInDirection * 39.37 + " sideways:-" + distanceMeasuredInDirectionSideways * 39.37);

                    // this.transform.position = spawner.currentTileRegion.transform.position;
                    if (distanceMeasuredInDirection * 39.37 > 5f || distanceMeasuredInDirectionSideways * 39.37 > 0.4f)
                    {
                        isPlacedCorrectlyAfterConfirmedPlacement = false;
                        // DistanceErrorCube.SetActive(true);
                        Vector3 direction = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                        direction.y = 0; // Ignore the Y-axis


                        print("Distance from place" + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) + "Direction" + direction);

                        // Determine the relative position
                        if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            // Debug.Log("Target is to the Right.");
                            // DistanceErrorCubeRL.SetActive(true);
                            // DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                            spawner.WriteOnHandMenu("Tile is on right by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                            // tileNameText.text = ((float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2)).ToString();
                            // LeftRightTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + "\"";
                            // LeftRightTextError.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, 0.5f, sideEdgeRight.localPosition.z);

                        }
                        if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            // Debug.Log("Target is to the Left.");
                            // DistanceErrorCubeRL.SetActive(true);
                            // DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                            spawner.WriteOnHandMenu("Tile is on left by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                            // tileNameText.text = ((float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2)).ToString();
                            // LeftRightTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + "\"";
                            // LeftRightTextError.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, 0.5f, sideEdgeLeft.localPosition.z);
                        }
                        if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            // Debug.Log("Target is Above (Forward).");
                            // DistanceErrorCubeTB.SetActive(true);
                            // DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                            spawner.WriteOnHandMenu("Tile is above by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                            // bottomTopTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + "\"";
                            // bottomTopTextError.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, 0.5f, sideEdgeTop.localPosition.z);
                        }
                        if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            // Debug.Log("Target is Below (Backward).");
                            // DistanceErrorCubeTB.SetActive(true);
                            // DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                            spawner.WriteOnHandMenu("Tile is down by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                            // bottomTopTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + "\"";
                            // bottomTopTextError.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, 0.5f, sideEdgeBottom.localPosition.z);
                        }
                        return false;
                    }
                    else
                    {
                        isPlaced = true;

                        // Get the current world position of the child object
                        Vector3 childWorldPosition = objectToCheckFrom.transform.position;

                        // Get the target position from the side edge
                        Vector3 targetPosition = objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;

                        // Calculate the direction from the target point to the child's current position

                        // Calculate the new world position for the child in the local positive Y direction of the objectToCheck
                        Vector3 localYDirection = objectToCheck.transform.forward + new Vector3(0, 0.2f, 0); // This gets the local 'up' direction which corresponds to the local +Y axis


                        // Combine the Y-direction offset with the original directional offset
                        Vector3 combinedDirection = localYDirection.normalized;
                        print("Placing first tile");
                        Vector3 newChildWorldPosition = targetPosition + combinedDirection * 4.7f * 0.0254f;

                        // Calculate the required offset for the parent
                        Vector3 offset = newChildWorldPosition - childWorldPosition;

                        // Apply the offset to the parent to snap it
                        transform.position += offset;
                        // CorrectTileIndicator.SetActive(true);
                        DistanceErrorCubeRL.SetActive(false);
                        DistanceErrorCubeTB.SetActive(false);
                        spawner.WriteOnHandMenu("Tile Placed Correctly");
                        isPlacedCorrectlyAfterConfirmedPlacement = true;
                        GetComponent<Rigidbody>().isKinematic = true;
                        SpawnTileMeasurements(sideEdgeRight, "1\" rake overhang", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.03f, 0, 0.55f), new Vector3(-1 / tileSize, 0, 0), sideEdgeRight);
                        string convertTodecimal = areaLeftByTile.ToString("F2");
                        SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + " Sidelap left", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.55f, 0, 0), new Vector3(-areaLeftByTile / tileSize, 0, 0), sideEdgeBottom);
                        bottomTopTextError.gameObject.SetActive(false);
                        LeftRightTextError.gameObject.SetActive(false);
                        return true;
                    }
                }
                else
                {
                    // written in other function
                    return false;
                }



            }
            else
            {
                return false;
            }
        }
        // public bool rightToLeft = true;

        public bool ShowKeywayerrors()
        {
            GameObject objectToCheck;
            GameObject objectToCheckFrom;
            print("Checking keyway distance");
            objectToCheck = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.gameObject;
            float distanceInInches;
            if (spawner.tileSpanWidth <= 0)
            {
                objectToCheckFrom = this.sideEdgeLeft.gameObject;
                distanceToCheckAccordingToExposure = spawner.overlapSpanAccordingtoExposure * 0.0254f;
                distanceInInches = spawner.overlapSpanAccordingtoExposure;
                print("A row of tiles placed now checking from the last tile to the above" + distanceToCheckAccordingToExposure);
            }
            else
            {
                objectToCheckFrom = this.sideEdgeRight.gameObject;
                distanceToCheckAccordingToExposure = 0.25f * 0.0254f * 2;
                distanceInInches = 0.6f;
            }

            if (spawner.reverseTheLine)
            {

                objectToCheck = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeRight.gameObject;

                if (spawner.tileSpanWidth <= 0)
                {
                    objectToCheckFrom = this.sideEdgeRight.gameObject;
                    distanceToCheckAccordingToExposure = (spawner.overlapSpanAccordingtoExposure) * 0.0254f;
                    distanceInInches = spawner.overlapSpanAccordingtoExposure;
                    print("A row of tiles placed now checking from the last tile to the above" + distanceToCheckAccordingToExposure);
                }
                else
                {
                    objectToCheckFrom = this.sideEdgeLeft.gameObject;
                    distanceToCheckAccordingToExposure = 0.25f * 0.0254f * 2;
                    distanceInInches = 0.6f;
                }

            }

            Vector3 localdirectionup = -objectToCheckFrom.transform.forward;
            Vector3 localdirectionright = -objectToCheckFrom.transform.right;


            Vector3 distanceMeasured = objectToCheckFrom.transform.position - objectToCheck.transform.position;
            float distanceMeasuredInDirection = Mathf.Abs(Vector3.Dot(distanceMeasured, localdirectionup));
            float distanceMeasuredInDirectionSideways = Mathf.Abs(Vector3.Dot(distanceMeasured, localdirectionright));

            print("First shake placing " + distanceMeasuredInDirection * 39.37 + " sideways:-" + distanceMeasuredInDirectionSideways * 39.37);

            // this.transform.position = spawner.currentTileRegion.transform.position;
            if (distanceMeasuredInDirection * 39.37 > distanceInInches || distanceMeasuredInDirectionSideways * 39.37 > 0.4f)
            {

                // DistanceErrorCube.SetActive(true);
                Vector3 direction = objectToCheckFrom.transform.position - objectToCheck.transform.position;
                direction.y = 0; // Ignore the Y-axis
                isPlacedCorrectlyAfterConfirmedPlacement = false;

                print("Distance from place" + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) + "Direction" + direction);

                // Determine the relative position
                if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                {
                    // Debug.Log("Target is to the Right.");
                    // DistanceErrorCubeRL.SetActive(true);
                    // DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                    // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right (Bad keyway spacing) " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37 + "inches";
                    spawner.WriteOnHandMenu("Tile is on right by (Bad keyway spacing) " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                    // LeftRightTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + "\"";
                    // LeftRightTextError.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, 0.5f, sideEdgeRight.localPosition.z);

                }
                if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                {
                    // Debug.Log("Target is to the Left.");
                    // DistanceErrorCubeRL.SetActive(true);
                    // DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                    spawner.WriteOnHandMenu("Tile is on left by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                    // tileNameText.text = ((float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2)).ToString();
                    // LeftRightTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + "\"";
                    // LeftRightTextError.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, 0.5f, sideEdgeLeft.localPosition.z);

                }
                if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                {
                    // Debug.Log("Target is Above (Forward).");
                    // DistanceErrorCubeTB.SetActive(true);
                    // DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                    // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37 + "inches";
                    spawner.WriteOnHandMenu("Tile is above by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                    // tileNameText.text = ((float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2)).ToString();
                    // bottomTopTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + "\"";
                    // bottomTopTextError.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, 0.5f, sideEdgeTop.localPosition.z);
                }
                if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                {
                    // Debug.Log("Target is Below (Backward).");
                    // DistanceErrorCubeTB.SetActive(true);
                    // DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                    // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37 + "inches";
                    spawner.WriteOnHandMenu("Tile is down by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                    // tileNameText.text = ((float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2)).ToString();
                    // bottomTopTextError.text = (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + "\"";
                    // bottomTopTextError.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, 0.5f, sideEdgeBottom.localPosition.z);
                }
                return false;
            }
            else
            {

                isPlaced = true;


                isPlacedCorrectlyAfterConfirmedPlacement = true;
                GetComponent<Rigidbody>().isKinematic = true;
                bottomTopTextError.gameObject.SetActive(false);
                LeftRightTextError.gameObject.SetActive(false);

                if (spawner.tileSpanWidth <= 0)
                {


                    // Get the current world position of the child object
                    Vector3 childWorldPosition = objectToCheckFrom.transform.position;

                    // Get the target position from the side edge
                    Vector3 targetPosition = objectToCheck.transform.position;

                    // Calculate the new world position for the child in the local positive Y direction of the objectToCheck
                    Vector3 localYDirection = objectToCheck.transform.forward + new Vector3(0, 0.1f, 0); // This gets the local 'up' direction which corresponds to the local +Y axis

                    // Combine the Y-direction offset with the original directional offset
                    Vector3 combinedDirection = localYDirection.normalized;
                    Vector3 newChildWorldPosition = targetPosition + combinedDirection * distanceToCheckAccordingToExposure;

                    // Calculate the required offset for the parent
                    Vector3 offset = newChildWorldPosition - childWorldPosition;

                    // Apply the offset to the parent to snap it
                    transform.position += offset;
                    string convertTodecimal = areaLeftByTile.ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" Sidelap left", Color.green, 0.5f, false);
                    print(spawner.rightToLeft + " Right to left");
                    if (spawner.rightToLeft)
                    {

                        PlaceTheMeasurementTag(false, new Vector3(-0.5f, 0, 0), new Vector3(-areaLeftByTile / tileSize, 0, 0), sideEdgeBottom);
                        string convertTodec = areaLeftByTile.ToString("F2");
                        SpawnTileMeasurements(objectToCheckFrom.transform, convertTodec + " rake overhang", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.03f, 0, 0.55f), new Vector3(0.13f, 0, 0), sideEdgeRight);
                    }
                    else
                    {
                        PlaceTheMeasurementTag(false, new Vector3(0.5f, 0, 0), new Vector3(areaLeftByTile / tileSize, 0, 0), sideEdgeBottom);
                        string convertTodec = areaLeftByTile.ToString("F2");
                        SpawnTileMeasurements(objectToCheckFrom.transform, convertTodec + " rake overhang", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.03f, 0, 0.55f), new Vector3(0.13f, 0, 0), sideEdgeLeft);
                        // PlaceTheMeasurementTag(false, new Vector3(0.52f, 0, 0), new Vector3(areaLeftByTile / tileSize, 0, 0), sideEdgeBottom);

                    }


                }
                else
                {

                    //    // Get the current world position of the child object
                    Vector3 childWorldPosition = objectToCheckFrom.transform.position;
                    Vector3 targetPosition = objectToCheck.transform.position;
                    // Calculate the direction from the target point to the child's current position
                    Vector3 direction;
                    Vector3 newChildWorldPosition;
                    if (spawner.reverseTheLine)
                    {

                        print("Going left to right");
                        direction = (objectToCheck.transform.right + new Vector3(0, 0f, 0)).normalized;
                        // newChildWorldPosition = objectToCheck.transform.position + (objectToCheck.transform.right + new Vector3(0, 0.2f, 0)) * -1 * distanceToCheckAccordingToExposure;

                    }
                    else
                    {
                        direction = (-objectToCheck.transform.right + new Vector3(0, 0f, 0)).normalized;
                        print("Going right to left");
                        // newChildWorldPosition = objectToCheck.transform.position + (objectToCheck.transform.right + new Vector3(0, 0.2f, 0)) * distanceToCheckAccordingToExposure;

                    }
                    newChildWorldPosition = targetPosition + direction * distanceToCheckAccordingToExposure;
                    Vector3 offset = newChildWorldPosition - childWorldPosition;

                    transform.position += offset;

                    ///////////////////////Place tags/////////////////
                    if (spawner.reverseTheLine)
                    {
                        string convertTodec = areaLeftByTile.ToString("F2");
                        SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" Sidelap left", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(0.52f, 0, 0), new Vector3(areaLeftByTile / tileSize, 0, 0), sideEdgeBottom);
                        SpawnTileMeasurements(objectToCheckFrom.transform, "3/8\" keyway spacing", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.02f, 0, 0.55f), new Vector3(0.1f, 0, 0), sideEdgeLeft);
                    }
                    else
                    {
                        string convertTodec = areaLeftByTile.ToString("F2");
                        SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" Sidelap left", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.52f, 0, 0), new Vector3(-areaLeftByTile / tileSize, 0, 0), sideEdgeBottom);
                        SpawnTileMeasurements(objectToCheckFrom.transform, "3/8\" keyway spacing", Color.green, 0.5f, false);
                        PlaceTheMeasurementTag(false, new Vector3(-0.02f, 0, 0.55f), new Vector3(0.1f, 0, 0), sideEdgeRight);
                    }

                }
                spawner.WriteOnHandMenu("Tile Placed Correctly");
                ShowSidelapOnTile();
                DistanceErrorCubeRL.SetActive(false);
                DistanceErrorCubeTB.SetActive(false);
                CorrectTileIndicator.SetActive(true);
                return true;
            }
        }

        public void SnapTileAccordingToKeyway()
        {
            GameObject objectToCheckFrome;
            GameObject objectToChecke = spawner.TilesPlaced[spawner.TilesPlaced.Count - 2].GetComponent<TileObject>().sideEdgeLeft.gameObject;


            objectToCheckFrome = this.sideEdgeRight.gameObject;


            if (spawner.reverseTheLine)
            {

                objectToChecke = spawner.TilesPlaced[spawner.TilesPlaced.Count - 2].GetComponent<TileObject>().sideEdgeRight.gameObject;


                objectToCheckFrome = this.sideEdgeLeft.gameObject;



            }
            Vector3 childWorldPosition = objectToCheckFrome.transform.position;
            Vector3 targetPosition = objectToChecke.transform.position;
            // Calculate the direction from the target point to the child's current position
            Vector3 direction;
            Vector3 newChildWorldPosition;
            if (spawner.reverseTheLine)
            {

                print("Going left to right");
                direction = (objectToChecke.transform.right + new Vector3(0, 0f, 0)).normalized;
                // newChildWorldPosition = objectToCheck.transform.position + (objectToCheck.transform.right + new Vector3(0, 0.2f, 0)) * -1 * distanceToCheckAccordingToExposure;

            }
            else
            {
                direction = (-objectToChecke.transform.right + new Vector3(0, 0f, 0)).normalized;
                print("Going right to left");
                // newChildWorldPosition = objectToCheck.transform.position + (objectToCheck.transform.right + new Vector3(0, 0.2f, 0)) * distanceToCheckAccordingToExposure;

            }
            newChildWorldPosition = targetPosition + direction * 0.25f * 0.0254f * 2;
            // Calculate the required offset for the parent
            Vector3 offset = newChildWorldPosition - childWorldPosition;
            transform.position += offset;
        }





        void ShowSidelapOnTile()
        {
            if (tilesUnderneath.Count == 1)
            {

                // print("1 number of tile");
                if (tilesUnderneath[0].areaCoveredByTileAbove == 0)
                {

                    Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                    Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                    float distanceLeft = (localPosLeftSource.x - localPosLeftTarget.x); //SHOULD BE +VE

                    Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                    Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                    float distanceRight = (localPosRightSource.x - localPosRightTarget.x);//SHOULD BE -VE
                    print(distanceLeft + " distance left " + distanceRight + " distance right ");
                    float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - tilesUnderneath[0].sideEdgeRight.transform.position.x);
                    float actualDistanceLeft = Mathf.Abs(sideEdgeLeft.transform.position.x - tilesUnderneath[0].sideEdgeLeft.transform.position.x);

                    Color tagcolor;
                    if (actualDistanceLeft * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    if (distanceLeft > 0)
                    {
                        string convertTodec = (actualDistanceLeft * 39.7f).ToString("F2");
                        SpawnTileMeasurements(sideEdgeLeft, convertTodec + "\"", tagcolor, 0.03f, false);
                        PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(-distanceLeft, 0, 0), sideEdgeLeft);
                    }



                    if (actualDistanceRight * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    if (distanceRight < 0)
                    {
                        string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                        SpawnTileMeasurements(sideEdgeRight, convertTodec + "\"", tagcolor, 0.03f, false);
                        PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(-distanceRight, 0, 0), sideEdgeRight);
                    }

                }
                else
                {
                    if (spawner.reverseTheLine)
                    {
                        //left to right

                        Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                        Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                        float distanceRight = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);

                        float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - tilesUnderneath[0].sideEdgeRight.transform.position.x);
                        // float actualDistanceLeft = Mathf.Abs(sideEdgeLeft.transform.position.x - tilesUnderneath[0].sideEdgeLeft.transform.position.x);

                        Color tagcolor;

                        if (actualDistanceRight * 39.7f >= Sidelap)
                        {
                            tagcolor = Color.green;
                        }
                        else
                        {
                            tagcolor = Color.red;
                        }
                        string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                        SpawnTileMeasurements(sideEdgeRight, convertTodec + "\"", tagcolor, 0.03f, false);
                        PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(distanceRight, 0, 0), sideEdgeRight);
                    }
                    else
                    {
                        Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                        Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                        float distanceLeft = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                        // float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - tilesUnderneath[0].sideEdgeRight.transform.position.x);
                        float actualDistanceLeft = Mathf.Abs(sideEdgeLeft.transform.position.x - tilesUnderneath[0].sideEdgeLeft.transform.position.x);

                        Color tagcolor;
                        if (actualDistanceLeft * 39.7f >= Sidelap)
                        {
                            tagcolor = Color.green;
                        }
                        else
                        {
                            tagcolor = Color.red;
                        }
                        string convertTodec = (actualDistanceLeft * 39.7f).ToString("F2");
                        SpawnTileMeasurements(sideEdgeLeft, convertTodec + "\"", tagcolor, 0.03f, false);
                        PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(-distanceLeft, 0, 0), sideEdgeLeft);

                    }
                }
            }

            // if 2 are there
            if (tilesUnderneath.Count == 2)
            {


                if (spawner.rightToLeft)
                {
                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                    ///
                    float distanceRight;
                    float distanceLeft;
                    if (tilesUnderneath[0].isStarter)
                    {

                        Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                        Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                        distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                        Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                        Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeRight.transform.position);
                        distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                    }
                    else
                    {
                        Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                        Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeLeft.transform.position);
                        distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                        Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                        Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                        distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                    }
                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////





                    //right to left


                    Vector3 localLeft = -this.transform.right;

                    Vector3 distanceMeasured1;
                    Vector3 distanceMeasured2;
                    if (tilesUnderneath[0].isStarter)
                    {
                        distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;
                        distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[1].sideEdgeRight.transform.position;
                    }
                    else
                    {
                        distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[1].sideEdgeLeft.transform.position;
                        distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;
                    }

                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);
                    Color tagcolor;
                    if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceRight, 0, 0), sideEdgeBottom);

                    if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceLeft, 0, 0), sideEdgeBottom);


                }
                else
                {
                    //left to right
                    // Correct the local left direction


                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                    Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                    Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeRight.transform.position);
                    float distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                    Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                    Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                    float distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////






                    Vector3 localLeft = -this.transform.right;

                    // Calculate the vector distances
                    Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[1].sideEdgeRight.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;

                    // Project the world direction vector onto the local left vector
                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);

                    Color tagcolor;
                    if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceRight, 0, 0), sideEdgeBottom);

                    if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceLeft, 0, 0), sideEdgeBottom);

                }



            }

            if (tilesUnderneath.Count == 3)
            {

                if (spawner.rightToLeft)
                {
                    float distanceRight;
                    float distanceLeft;
                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                    if (tilesUnderneath[0].isStarter)
                    {

                        Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                        Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                        distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                        Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                        Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[2].sideEdgeRight.transform.position);
                        distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                    }
                    else
                    {
                        Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                        Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[2].sideEdgeLeft.transform.position);
                        distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                        Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                        Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                        distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                    }
                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////





                    //right to left


                    Vector3 localLeft = -this.transform.right;


                    Vector3 distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[2].sideEdgeLeft.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;


                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);
                    Color tagcolor;
                    if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceRight, 0, 0), sideEdgeBottom);

                    if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceLeft, 0, 0), sideEdgeBottom);


                }
                else
                {
                    //left to right
                    // Correct the local left direction


                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                    Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                    Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeRight.transform.position);
                    float distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                    Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                    Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                    float distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                    //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////






                    Vector3 localLeft = -this.transform.right;

                    // Calculate the vector distances
                    Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[2].sideEdgeRight.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;

                    // Project the world direction vector onto the local left vector
                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);

                    Color tagcolor;
                    if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceRight, 0, 0), sideEdgeBottom);

                    if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                    {
                        tagcolor = Color.green;
                    }
                    else
                    {
                        tagcolor = Color.red;
                    }
                    string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, false);
                    PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceLeft, 0, 0), sideEdgeBottom);

                }


            }
        }


        public void MoveTheCutPosition(float position)
        {
            print("Move the cut position" + position);
            cutPosition.transform.localPosition = new Vector3(cutPosition.transform.localPosition.x + position, cutPosition.transform.localPosition.y, cutPosition.transform.localPosition.z);
        }


        public void ShowErrorsWhenDropped()
        {
            // print
            if (isStarter)
            {

                SpawnTileMeasurements(sideEdgeBottom, "1/2\" eave overhang", Color.green, 0.03f, true);
                PlaceTheMeasurementTag(true, new Vector3(-0.55f, 0, 0), new Vector3(0, 0, 0.07f), sideEdgeBottom);


                if (spawner.TilesPlaced.Count == 0)
                {
                    // first starter being placed
                    float rakeOverHang = Vector3.Distance(this.transform.InverseTransformPoint(this.transform.position), this.transform.InverseTransformPoint(spawner.currentTileRegion.transform.position));
                    float ActualDistanceOverhang = Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position);
                    string convertTodec = (ActualDistanceOverhang * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeRight, convertTodec + "\" away from the point (0\" rake overhang)", Color.red, 0.03f, true);
                    PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(rakeOverHang, 0, 0), sideEdgeRight);

                }
                else if (spawner.TilesPlaced.Count == spawner.starterColliderHolder.GetComponentsInChildren<BoxCollider>().Length - 1)
                {
                    //last starter being placed
                    // float rakeOverHang = Vector3.Distance(this.transform.InverseTransformPoint(this.transform.position), this.transform.InverseTransformPoint(spawner.currentTileRegion.transform.position));
                    // SpawnTileMeasurements(sideEdgeLeft, rakeOverHang * 39.7f + "\" away from the point (0\" rake overhang)", Color.red, 0.03f, true);
                    // PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(-rakeOverHang, 0, 0), sideEdgeLeft);

                    float keywayError = Vector3.Distance(this.transform.InverseTransformPoint(sideEdgeRight.transform.position), this.transform.InverseTransformPoint(spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position));
                    float ActualkeywayError = Vector3.Distance(sideEdgeRight.transform.position, spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position);
                    print("keywayerror" + keywayError);
                    string convertTodec = (ActualkeywayError * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeRight, convertTodec + "\" keyway distance", Color.red, 0.03f, true);
                    PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(keywayError, 0, 0), sideEdgeRight);

                }
                else
                {
                    //rest of starters

                    float keywayError = Vector3.Distance(this.transform.InverseTransformPoint(sideEdgeRight.transform.position), this.transform.InverseTransformPoint(spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position));
                    float ActualkeywayError = Vector3.Distance(sideEdgeRight.transform.position, spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position);
                    print("keywayerror" + keywayError);
                    string convertTodec = (ActualkeywayError * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeRight, convertTodec + "\" keyway distance", Color.red, 0.03f, true);
                    PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(keywayError, 0, 0), sideEdgeRight);

                }
            }
            else
            {





                ////////////////////////////////////////////////////////Sidelap Errors/////////////////////////////////////////////////////////////////
                if (spawner.tileSpanWidth - tileSize > 0)
                {


                    if (tilesUnderneath.Count == 1)
                    {

                        // print("1 number of tile");
                        if (tilesUnderneath[0].areaCoveredByTileAbove == 0)
                        {

                            Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                            Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                            float distanceLeft = (localPosLeftSource.x - localPosLeftTarget.x); //SHOULD BE +VE

                            Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                            Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                            float distanceRight = (localPosRightSource.x - localPosRightTarget.x);//SHOULD BE -VE
                            print(distanceLeft + " distance left " + distanceRight + " distance right ");
                            float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - tilesUnderneath[0].sideEdgeRight.transform.position.x);
                            float actualDistanceLeft = Mathf.Abs(sideEdgeLeft.transform.position.x - tilesUnderneath[0].sideEdgeLeft.transform.position.x);

                            Color tagcolor;
                            if (actualDistanceLeft * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            if (distanceLeft > 0)
                            {
                                string convertTodec = (actualDistanceLeft * 39.7f).ToString("F2");
                                SpawnTileMeasurements(sideEdgeLeft, convertTodec + "\"", tagcolor, 0.03f, true);
                                PlaceTheMeasurementTag(false, new Vector3(-0.5f, 0, 0f), new Vector3(-distanceLeft, 0, 0), sideEdgeBottom);
                            }



                            if (actualDistanceRight * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            if (distanceRight < 0)
                            {
                                string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                                SpawnTileMeasurements(sideEdgeRight, convertTodec + "\"", tagcolor, 0.03f, true);
                                PlaceTheMeasurementTag(false, new Vector3(0.5f, 0, 0f), new Vector3(-distanceRight, 0, 0), sideEdgeBottom);
                            }

                        }
                        else
                        {
                            if (spawner.reverseTheLine)
                            {
                                //left to right

                                Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                                Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                                float distanceRight = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);

                                float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - tilesUnderneath[0].sideEdgeRight.transform.position.x);
                                // float actualDistanceLeft = Mathf.Abs(sideEdgeLeft.transform.position.x - tilesUnderneath[0].sideEdgeLeft.transform.position.x);

                                Color tagcolor;

                                if (actualDistanceRight * 39.7f >= Sidelap)
                                {
                                    tagcolor = Color.green;
                                }
                                else
                                {
                                    tagcolor = Color.red;
                                }
                                string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                                SpawnTileMeasurements(sideEdgeRight, convertTodec + "\"", tagcolor, 0.03f, true);
                                PlaceTheMeasurementTag(false, new Vector3(0.5f, 0, 0f), new Vector3(distanceRight, 0, 0), sideEdgeBottom);
                            }
                            else
                            {
                                Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                                Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                                float distanceLeft = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                                // float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - tilesUnderneath[0].sideEdgeRight.transform.position.x);
                                float actualDistanceLeft = Mathf.Abs(sideEdgeLeft.transform.position.x - tilesUnderneath[0].sideEdgeLeft.transform.position.x);

                                Color tagcolor;
                                if (actualDistanceLeft * 39.7f >= Sidelap)
                                {
                                    tagcolor = Color.green;
                                }
                                else
                                {
                                    tagcolor = Color.red;
                                }
                                string convertTodec = (actualDistanceLeft * 39.7f).ToString("F2");
                                SpawnTileMeasurements(sideEdgeLeft, convertTodec + "\"", tagcolor, 0.03f, true);
                                PlaceTheMeasurementTag(false, new Vector3(-0.5f, 0, 0f), new Vector3(-distanceLeft, 0, 0), sideEdgeBottom);

                            }
                        }
                    }

                    // if 2 are there
                    if (tilesUnderneath.Count == 2)
                    {


                        if (spawner.rightToLeft)
                        {
                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                            ///
                            float distanceRight;
                            float distanceLeft;
                            if (tilesUnderneath[0].isStarter)
                            {

                                Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                                Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                                distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                                Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                                Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeRight.transform.position);
                                distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                            }
                            else
                            {
                                Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                                Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeLeft.transform.position);
                                distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                                Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                                Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                                distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                            }
                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////





                            //right to left


                            Vector3 localLeft = -this.transform.right;

                            Vector3 distanceMeasured1;
                            Vector3 distanceMeasured2;
                            if (tilesUnderneath[0].isStarter)
                            {
                                distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;
                                distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[1].sideEdgeRight.transform.position;
                            }
                            else
                            {
                                distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[1].sideEdgeLeft.transform.position;
                                distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;
                            }

                            float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                            float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                            // If you want the magnitude of this projection as a positive number
                            leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                            leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);
                            Color tagcolor;
                            if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceRight, 0, 0), sideEdgeBottom);

                            if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceLeft, 0, 0), sideEdgeBottom);


                        }
                        else
                        {
                            //left to right
                            // Correct the local left direction


                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                            Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                            Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeRight.transform.position);
                            float distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                            Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                            Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                            float distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////






                            Vector3 localLeft = -this.transform.right;

                            // Calculate the vector distances
                            Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[1].sideEdgeRight.transform.position;
                            Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;

                            // Project the world direction vector onto the local left vector
                            float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                            float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                            // If you want the magnitude of this projection as a positive number
                            leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                            leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);

                            Color tagcolor;
                            if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceRight, 0, 0), sideEdgeBottom);

                            if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceLeft, 0, 0), sideEdgeBottom);

                        }



                    }

                    if (tilesUnderneath.Count == 3)
                    {

                        if (spawner.rightToLeft)
                        {
                            float distanceRight;
                            float distanceLeft;
                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                            if (tilesUnderneath[0].isStarter)
                            {

                                Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                                Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                                distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                                Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                                Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[2].sideEdgeRight.transform.position);
                                distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                            }
                            else
                            {
                                Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                                Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[2].sideEdgeLeft.transform.position);
                                distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                                Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                                Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeRight.transform.position);
                                distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                            }
                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////





                            //right to left


                            Vector3 localLeft = -this.transform.right;


                            Vector3 distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[2].sideEdgeLeft.transform.position;
                            Vector3 distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;


                            float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                            float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                            // If you want the magnitude of this projection as a positive number
                            leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                            leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);
                            Color tagcolor;
                            if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceRight, 0, 0), sideEdgeBottom);

                            if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceLeft, 0, 0), sideEdgeBottom);


                        }
                        else
                        {
                            //left to right
                            // Correct the local left direction


                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////
                            Vector3 localPosLeftSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                            Vector3 localPosLeftTarget = this.transform.InverseTransformPoint(tilesUnderneath[1].sideEdgeRight.transform.position);
                            float distanceRight = Mathf.Abs(localPosLeftSource.x - localPosLeftTarget.x);

                            Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                            Vector3 localPosRightTarget = this.transform.InverseTransformPoint(tilesUnderneath[0].sideEdgeLeft.transform.position);
                            float distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);
                            //////////////////////////////////////////Get distance for local tags//////////////////////////////////////////////////






                            Vector3 localLeft = -this.transform.right;

                            // Calculate the vector distances
                            Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[2].sideEdgeRight.transform.position;
                            Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;

                            // Project the world direction vector onto the local left vector
                            float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                            float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                            // If you want the magnitude of this projection as a positive number
                            leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                            leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);

                            Color tagcolor;
                            if (leftwardDistanceLocal1 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodec = (leftwardDistanceLocal1 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodec + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(-0.5f, -0.03f, -0.03f), new Vector3(distanceRight, 0, 0), sideEdgeBottom);

                            if (leftwardDistanceLocal2 * 39.7f >= Sidelap)
                            {
                                tagcolor = Color.green;
                            }
                            else
                            {
                                tagcolor = Color.red;
                            }
                            string convertTodecimal = (leftwardDistanceLocal2 * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeBottom, convertTodecimal + "\" ", tagcolor, 0.52f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0.5f, -0.03f, -0.03f), new Vector3(-distanceLeft, 0, 0), sideEdgeBottom);

                        }


                    }
                }

                /////////////////////////////////////////Keyway Errors///////////////////////////////////////////////
                ///

                if (!spawner.isFirstShakePlaced)
                {
                    // first shake being placed

                    Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                    Vector3 localPosRightTarget = this.transform.InverseTransformPoint(spawner.TilesPlaced[0].GetComponent<TileObject>().sideEdgeRight.transform.position);
                    float distanceRight = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);

                    float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - spawner.TilesPlaced[0].GetComponent<TileObject>().sideEdgeRight.transform.position.x);
                    string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                    SpawnTileMeasurements(sideEdgeRight, convertTodec + "\" away from position", Color.red, 0.03f, true);
                    PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0.5f), new Vector3(distanceRight, 0, 0), sideEdgeRight);
                }
                else
                {




                    if (!spawner.reverseTheLine)
                    {
                        print(!spawner.reverseTheLine + "!spawner.reverseTheLine");
                        //left to right
                        if (spawner.tileSpanWidth <= 0)
                        {
                            //////////////Line ended place the tile above///////////////////


                            Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                            Vector3 localPosRightTarget = this.transform.InverseTransformPoint(spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position);
                            float distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);

                            float actualDistanceRight = Mathf.Abs(sideEdgeLeft.transform.position.x - spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position.x);
                            string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeLeft, convertTodec + "\"", Color.red, 0.03f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0.5f), new Vector3(-distanceLeft, 0, 0), sideEdgeLeft);







                        }
                        else
                        {
                            Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                            Vector3 localPosRightTarget = this.transform.InverseTransformPoint(spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position);
                            float distanceRight = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);

                            float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeLeft.transform.position.x);
                            string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeRight, convertTodec + "\" keyway error", Color.red, 0.03f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(distanceRight, 0, 0), sideEdgeRight);
                        }

                    }
                    else
                    {
                        print(!spawner.reverseTheLine + "!spawner.reverseTheLine");
                        //right to left
                        if (spawner.tileSpanWidth <= 0)
                        {
                            //////////////Line ended place the tile above///////////////////
                            ///
                            Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeRight.transform.position);
                            Vector3 localPosRightTarget = this.transform.InverseTransformPoint(spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeRight.transform.position);
                            float distanceRight = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);

                            float actualDistanceRight = Mathf.Abs(sideEdgeRight.transform.position.x - spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeRight.transform.position.x);
                            string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeRight, convertTodec + "\" away from position", Color.red, 0.03f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0.5f), new Vector3(distanceRight, 0, 0), sideEdgeRight);
                        }
                        else
                        {
                            Vector3 localPosRightSource = this.transform.InverseTransformPoint(sideEdgeLeft.transform.position);
                            Vector3 localPosRightTarget = this.transform.InverseTransformPoint(spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeRight.transform.position);
                            float distanceLeft = Mathf.Abs(localPosRightSource.x - localPosRightTarget.x);

                            float actualDistanceRight = Mathf.Abs(sideEdgeLeft.transform.position.x - spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeRight.transform.position.x);
                            string convertTodec = (actualDistanceRight * 39.7f).ToString("F2");
                            SpawnTileMeasurements(sideEdgeLeft, convertTodec + "\"", Color.red, 0.03f, true);
                            PlaceTheMeasurementTag(false, new Vector3(0f, 0, 0f), new Vector3(-distanceLeft, 0, 0), sideEdgeLeft);
                        }

                    }
                }








            }


        }



















        ///////////////////////////////////////Just for testing//////////////////////////////////////





    }






}