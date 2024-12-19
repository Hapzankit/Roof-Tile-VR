using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
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
        public Color ColorFail => failColor;
        [SerializeField] private Color passColor;
        public Color ColorTrue => passColor;

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





        ////////////////////////////////////For Statistics///////////////////////////////
        public int InCorrectoverHangs = 0;
        public int InCorrectKeywaySpaces = 0;
        public int IncorrectExposure = 0;
        public int IncorrectSidelap;
        public bool isBothFastnersplaced = false;
        public bool isPlacedCorrectlyAfterConfirmedPlacement = false;






        /// //////////////////////SideLap Checks///////////////////////////       
        // Use when tile is placed (other tiles will be placed on top of this)
        public float areaCoveredByTileAbove;
        public float areaLeftByTileAbove;
        // Use when tile is being placed
        public List<TileObject> tilesUnderneath;


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

        public Transform SideEdgeBottom => sideEdgeBottom;
        public Transform SideEdgeTop => sideEdgeTop;

        private void Start()
        {
            spawner = FindObjectOfType<TileCasting>();
            YesButton?.onClick.AddListener(YesButtonPressed);
            areaCoveredByTileAbove = 0;
            areaLeftByTileAbove = tileSize;

            //tileCanvasUI.gameObject.SetActive(false);
        }

        public void ShowPlacementPrompt()
        {
            tileCanvasUI.gameObject.SetActive(true);
        }

        public void YesButtonPressed()
        {
            spawner.SetActiveTileStateToPlaced(1);
            DisableInteraction();
            spawner.TileSelectText("Tile Placed! Pick New Tile");
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

        public void OnTilePicked()
        {

            spawner.TileSelectText("Tile Picked");
            spawner.OnTilePick();
            spawner.currentTilePrefab = this;
            this.transform.rotation = Quaternion.Euler(-45, 0, 0);
            spawner.currentTileWidth = tileSize;
            DistanceErrorCubeRL.gameObject.SetActive(false);
            DistanceErrorCubeTB.gameObject.SetActive(false);
            CorrectTileIndicator.gameObject.SetActive(false);


        }
        Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);
        float checkDepth = 0.04f; // Depth of the box below the plane
        public void OnTileDropped()
        {
            StoreStatistics();
            //ShowPlacementPrompt();
            spawner.TileSelectText("Tile Dropped" + isTileAbove + isValidTile);
            if (CalculateSidelapCheck())
            {
                spawner.OnTileDropped();
            }
            else
            {
                DistanceErrorCubeTB.SetActive(true);
                DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
            }


        }



        // void OnDrawGizmos()
        // {
        //     float checkDepth = 0.04f;  // Match this with your actual usage
        //     Vector3 planeSize = GetComponent<Renderer>().bounds.size;
        //     Vector3 boxSize = new Vector3(planeSize.x, checkDepth, planeSize.z);
        //     Vector3 boxCenter = transform.position + transform.TransformDirection(new Vector3(0, -checkDepth, 0));

        //     Gizmos.color = Color.red;
        //     Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        //     Gizmos.DrawWireCube(Vector3.zero, boxSize);
        // }


        float Sidelap = 1.5f;
        float areaToBeCovered; float areaLeftByTile; int tileNum; int numberoftilesunderneath;
        public void ReduceTileArea()
        {
            tilesUnderneath[tileNum].areaCoveredByTileAbove = areaToBeCovered;
            tilesUnderneath[tileNum].areaLeftByTileAbove = areaLeftByTile;
        }


        public bool CalculateSidelapCheck()

        {
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
                    tilesUnderneath.Add(collider.GetComponent<TileObject>());
                }
            }

            // if 1 tile is under
            if (tilesUnderneath.Count == 1)
            {
                numberoftilesunderneath = 1;
                if (tilesUnderneath[0].areaCoveredByTileAbove == 0)
                {
                    if (tilesUnderneath[0].tileSize > tileSize)
                    {
                        print("TIle size correct");
                        // tilesUnderneath[0].areaCoveredByTileAbove = tileSize;
                        // tilesUnderneath[0].areaLeftByTileAbove = tilesUnderneath[0].tileSize - tileSize;
                        areaToBeCovered = tileSize;
                        areaLeftByTile = tilesUnderneath[0].tileSize - tileSize;
                        tileNum = 0;
                        return true;
                    }
                    else
                    {
                        print("TIle size incorrect");
                        spawner.WriteOnHandMenu("Incorrect sidelap");
                        IncorrectSidelap++;
                        return false;
                    }
                }
                else
                {
                    float areaLeft = tileSize - tilesUnderneath[0].areaLeftByTileAbove;
                    areaLeft = Math.Abs(areaLeft);
                    if (areaLeft < Sidelap)
                    {
                        print("TIle size incorrect");
                        spawner.WriteOnHandMenu("Incorrect sidelap");
                        IncorrectSidelap++;
                        return false;
                    }
                    else
                    {
                        print("TIle size correct");
                        // tilesUnderneath[0].areaCoveredByTileAbove += tileSize;
                        // tilesUnderneath[0].areaLeftByTileAbove = tilesUnderneath[0].areaCoveredByTileAbove - tileSize;

                        areaToBeCovered = tilesUnderneath[0].areaCoveredByTileAbove + tileSize;
                        areaLeft = tilesUnderneath[0].areaCoveredByTileAbove - tileSize;
                        tileNum = 0;
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

                if (!spawner.reverseTheLine)
                {
                    //right to left
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

                        // tilesUnderneath[1].areaCoveredByTileAbove = leftwardDistanceLocal2 * 39.37f;
                        // tilesUnderneath[1].areaLeftByTileAbove = tilesUnderneath[1].tileSize - leftwardDistanceLocal2 * 39.37f;

                        areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                        areaLeftByTile = tilesUnderneath[1].tileSize - leftwardDistanceLocal2 * 39.37f;
                        tileNum = 1;
                        return true;

                    }
                    else
                    {
                        IncorrectSidelap++;
                        return false;
                    }
                }
                else
                {
                    //left to right
                    // Correct the local left direction
                    Vector3 localLeft = -this.transform.right;

                    // Calculate the vector distances
                    Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[1].sideEdgeLeft.transform.position;

                    // Project the world direction vector onto the local left vector
                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);

                    // Draw lines to visualize the directions and magnitudes
                    Debug.DrawLine(sideEdgeLeft.transform.position, sideEdgeLeft.transform.position + (localLeft * leftwardDistanceLocal1), Color.red, 5.0f);
                    Debug.DrawLine(sideEdgeRight.transform.position, sideEdgeRight.transform.position + (localLeft * leftwardDistanceLocal2), Color.blue, 5.0f);


                    print("Going left to right Distances:-" + leftwardDistanceLocal1 * 39.37f + " " + leftwardDistanceLocal2 * 39.37f + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[1].name);
                    spawner.WriteOnHandMenu("Sidelap from left tile " + (float)Math.Round(leftwardDistanceLocal1 * 39.37f, 2) + " Sidelap from right tile " + (float)Math.Round(leftwardDistanceLocal2 * 39.37f, 2) + " Sidelap must be atleast 1.5 inches on both sides");
                    if (leftwardDistanceLocal1 * 39.37 > Sidelap && leftwardDistanceLocal2 * 39.37 > Sidelap)
                    {
                        // tilesUnderneath[1].areaCoveredByTileAbove = leftwardDistanceLocal2 * 39.37f;
                        // tilesUnderneath[1].areaLeftByTileAbove = tilesUnderneath[1].tileSize - leftwardDistanceLocal2 * 39.37f;
                        areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                        areaLeftByTile = tilesUnderneath[1].tileSize - leftwardDistanceLocal2 * 39.37f;
                        tileNum = 1;
                        return true;
                    }
                    else
                    {
                        IncorrectSidelap++;
                        return false;
                    }
                }



            }

            if (numberoftilesunderneath == 3)
            {
                numberoftilesunderneath = 3;
                // bool isTileCovered = true;
                tilesUnderneath = tilesUnderneath.OrderBy(go => ExtractNumber(go.name)).ToList();

                if (!spawner.reverseTheLine)
                {
                    //right to left
                    Vector3 localLeft = -this.transform.right;


                    Vector3 distanceMeasured1 = sideEdgeRight.transform.position - tilesUnderneath[0].sideEdgeLeft.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeLeft.transform.position - tilesUnderneath[2].sideEdgeRight.transform.position;


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

                        // tilesUnderneath[1].areaCoveredByTileAbove = leftwardDistanceLocal2 * 39.37f;
                        // tilesUnderneath[1].areaLeftByTileAbove = tilesUnderneath[1].tileSize - leftwardDistanceLocal2 * 39.37f;

                        areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                        areaLeftByTile = tilesUnderneath[2].tileSize - leftwardDistanceLocal2 * 39.37f;
                        tileNum = 2;
                        return true;

                    }
                    else
                    {
                        IncorrectSidelap++;
                        return false;
                    }
                }
                else
                {
                    //left to right
                    // Correct the local left direction
                    Vector3 localLeft = -this.transform.right;

                    // Calculate the vector distances
                    Vector3 distanceMeasured1 = sideEdgeLeft.transform.position - tilesUnderneath[0].sideEdgeRight.transform.position;
                    Vector3 distanceMeasured2 = sideEdgeRight.transform.position - tilesUnderneath[1].sideEdgeLeft.transform.position;

                    // Project the world direction vector onto the local left vector
                    float leftwardDistanceLocal1 = Vector3.Dot(distanceMeasured1, localLeft);
                    float leftwardDistanceLocal2 = Vector3.Dot(distanceMeasured2, localLeft);

                    // If you want the magnitude of this projection as a positive number
                    leftwardDistanceLocal1 = Mathf.Abs(leftwardDistanceLocal1);
                    leftwardDistanceLocal2 = Mathf.Abs(leftwardDistanceLocal2);

                    // Draw lines to visualize the directions and magnitudes
                    Debug.DrawLine(sideEdgeLeft.transform.position, sideEdgeLeft.transform.position + (localLeft * leftwardDistanceLocal1), Color.red, 5.0f);
                    Debug.DrawLine(sideEdgeRight.transform.position, sideEdgeRight.transform.position + (localLeft * leftwardDistanceLocal2), Color.blue, 5.0f);


                    print("Going left to right Distances:-" + leftwardDistanceLocal1 * 39.37f + " " + leftwardDistanceLocal2 * 39.37f + "names 0 " + tilesUnderneath[0].name + "1:- " + tilesUnderneath[2].name);
                    spawner.WriteOnHandMenu("Sidelap from left tile " + (float)Math.Round(leftwardDistanceLocal1 * 39.37f, 2) + " Sidelap from right tile " + (float)Math.Round(leftwardDistanceLocal2 * 39.37f, 2) + " Sidelap must be atleast 1.5 inches on both sides");
                    if (leftwardDistanceLocal1 * 39.37 > Sidelap && leftwardDistanceLocal2 * 39.37 > Sidelap)
                    {
                        // tilesUnderneath[1].areaCoveredByTileAbove = leftwardDistanceLocal2 * 39.37f;
                        // tilesUnderneath[1].areaLeftByTileAbove = tilesUnderneath[1].tileSize - leftwardDistanceLocal2 * 39.37f;
                        areaToBeCovered = leftwardDistanceLocal2 * 39.37f;
                        areaLeftByTile = tilesUnderneath[2].tileSize - leftwardDistanceLocal2 * 39.37f;
                        tileNum = 2;
                        return true;
                    }
                    else
                    {
                        IncorrectSidelap++;
                        return false;
                    }
                }

            }
            else
            {
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

        void Update()
        {

            // ShowStarterErrors();
            // if (!isStarter)
            // {

            //     ShowShakeTIleErrorsTesting();
            // }


            //JUST FOR DEBUG
            // if (isTileAbove && isValidTile)
            // {
            //     ShowStarterErrors();
            // }
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
                            if (sideWaysDistance > 0)
                            {
                                InCorrectoverHangs++;
                                InCorrectKeywaySpaces++;
                            }
                            if (verticalDistance > 0)
                            {
                                InCorrectoverHangs++;
                            }
                        }
                        else
                        {
                            if (verticalDistance > 0)
                            {
                                InCorrectoverHangs++;
                            }
                            if (sideWaysDistance > 0)
                            {
                                InCorrectKeywaySpaces++;
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
                    if (objectToCheck.GetComponent<TileObject>().sideEdgeRight && isValidTile && isTileAbove/*true*/)
                    {
                        // this.transform.position = spawner.currentTileRegion.transform.position;
                        if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 > 5.3f)
                        {
                            Vector3 distanceFromPoint = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                            float sideWaysDistance = Math.Abs(Vector3.Dot(distanceFromPoint, rightDirection) * 39.37f);
                            float verticalDistance = Math.Abs(Vector3.Dot(distanceFromPoint, forwardDirection) * 39.37f);
                            if (verticalDistance > 0)
                            {
                                IncorrectExposure++;
                            }
                            if (sideWaysDistance > 0)
                            {
                                InCorrectoverHangs++;
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
                        distanceToCheckAccordingToExposure = 0.25f * 0.0254f;
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
                        Vector3 distanceFromPoint = objectToCheckFromNormal.transform.position - objectToCheckNormal.GetComponent<TileObject>().sideEdgeRight.transform.position;
                        float sideWaysDistance = Math.Abs(Vector3.Dot(distanceFromPoint, rightDirection) * 39.37f);
                        float verticalDistance = Math.Abs(Vector3.Dot(distanceFromPoint, forwardDirection) * 39.37f);
                        if (sideWaysDistance > 0)
                        {
                            InCorrectKeywaySpaces++;
                        }
                        if (verticalDistance > 0)
                        {
                            IncorrectExposure++;
                        }
                    }
                }
            }
        }

        public void ShowStarterErrors()
        {
            print("Showing starter tile errors");
            if (spawner.currentTileRegion && isValidTile && isTileAbove)
            {
                // this.transform.position = spawner.currentTileRegion.transform.position;
                if (Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 > 2.1f)
                {

                    isPlacedCorrectlyAfterConfirmedPlacement = false;


                    // DistanceErrorCube.SetActive(true);
                    Vector3 direction = this.transform.position - spawner.currentTileRegion.transform.position;

                    direction.y = 0; // Ignore the Y-axis


                    print("Distance from place" + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) + "Direction" + direction);

                    // Determine the relative position
                    if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                    {

                        DistanceErrorCubeRL.SetActive(true);
                        DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                        // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile is on right by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");

                    }
                    if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                    {
                        // Debug.Log("Target is to the Left.");
                        DistanceErrorCubeRL.SetActive(true);
                        DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                        // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile  is on left by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");
                    }
                    if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                    {
                        // Debug.Log("Target is Above (Forward).");
                        DistanceErrorCubeTB.SetActive(true);
                        DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                        // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile is above by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");
                    }
                    if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                    {
                        // Debug.Log("Target is Below (Backward).");
                        DistanceErrorCubeTB.SetActive(true);
                        DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                        // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                        spawner.WriteOnHandMenu("Tile is down by " + (float)Math.Round(Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37, 2) + " inches");
                    }
                }
                else
                {
                    isPlacedCorrectlyAfterConfirmedPlacement = true;
                    CorrectTileIndicator.SetActive(true);
                    // this.transform.localPosition = new Vector3(0, 0, 0.24f);
                    this.transform.position = spawner.currentTileRegion.transform.position;
                    DistanceErrorCubeRL.SetActive(false);
                    DistanceErrorCubeTB.SetActive(false);
                    spawner.WriteOnHandMenu("Tile Placed Correctly");

                    //JUST FOR DEBUG
                    // if (!isYesPressed)
                    // {
                    //     spawner.YesButtonPressed();
                    //     isYesPressed = true;
                    // }
                }



            }
        }

        bool isYesPressed = false;

        public bool isTileAbove = false;
        public bool isValidTile = false;
        public void SetTileAboveRoof(bool isAbove, bool isTileValid, TileDropCollisionCheck regionTileDropped)
        {
            // ShowPlacementPrompt();
            // spawner.ShowPlacementPrompt();
            isTileAbove = isAbove;
            isValidTile = isTileValid;
            spawner.currentTileRegion = regionTileDropped;
            this.transform.rotation = Quaternion.Euler(-45, 0, 0);


            // this.transform.localPosition = new Vector3(this.transform.localPosition.x, regionTileDropped.transform.localPosition.y, this.transform.localPosition.z);
        }

        private bool isTileOnRightSide = false;
        private bool isTileOnLeftSide = false;
        private bool isTileOnBottom = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CollisionCheck edge))
            {

            }
        }


        public float distanceToCheckAccordingToExposure;



        public void ShowShakeTIleErrors(bool isfirstShakeTile)
        {

            print("Showing shake tile errors");
            GameObject objectToCheck = spawner.TilesPlaced[0];
            GameObject objectToCheckFrom;
            if (isfirstShakeTile)
            {

                objectToCheckFrom = this.sideEdgeRight.gameObject;
                // objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
                if (objectToCheck.GetComponent<TileObject>().sideEdgeRight && isValidTile && isTileAbove/*true*/)
                {
                    // this.transform.position = spawner.currentTileRegion.transform.position;
                    if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 > 5.3f)
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
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                            // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                            spawner.WriteOnHandMenu("Tile is on right by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                        }
                        if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            // Debug.Log("Target is to the Left.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                            // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                            spawner.WriteOnHandMenu("Tile is on left by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                        }
                        if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            // Debug.Log("Target is Above (Forward).");
                            DistanceErrorCubeTB.SetActive(true);
                            DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                            // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                            spawner.WriteOnHandMenu("Tile is above by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                        }
                        if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            // Debug.Log("Target is Below (Backward).");
                            DistanceErrorCubeTB.SetActive(true);
                            DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                            // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                            spawner.WriteOnHandMenu("Tile is down by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37, 2) + " inches");
                        }
                    }
                    else
                    {

                        // Get the current world position of the child object
                        Vector3 childWorldPosition = objectToCheckFrom.transform.position;

                        // Get the target position from the side edge
                        Vector3 targetPosition = objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;

                        // Calculate the direction from the target point to the child's current position
                        Vector3 direction = (childWorldPosition - targetPosition).normalized;

                        // Calculate the new world position for the child in the local positive Y direction of the objectToCheck
                        Vector3 localYDirection = objectToCheck.transform.forward + new Vector3(0, 0.2f, 0); // This gets the local 'up' direction which corresponds to the local +Y axis
                        Vector3 newYDirectionWorldPosition = targetPosition + localYDirection * 5 * 0.0254f;

                        // Combine the Y-direction offset with the original directional offset
                        Vector3 combinedDirection = localYDirection.normalized;
                        Vector3 newChildWorldPosition = targetPosition + combinedDirection * 5 * 0.0254f;

                        // Calculate the required offset for the parent
                        Vector3 offset = newChildWorldPosition - childWorldPosition;

                        // Apply the offset to the parent to snap it
                        transform.position += offset;
                        CorrectTileIndicator.SetActive(true);
                        DistanceErrorCubeRL.SetActive(false);
                        DistanceErrorCubeTB.SetActive(false);
                        spawner.WriteOnHandMenu("Tile Placed Correctly");
                        isPlacedCorrectlyAfterConfirmedPlacement = true;
                    }
                }
                else
                {
                    // written in other function
                }



            }
        }

        public void ShowKeywayerrors()
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
                distanceToCheckAccordingToExposure = 0.25f * 0.0254f;
                distanceInInches = 2f;
            }

            if (spawner.reverseTheLine)
            {
                print("Going right to left");
                objectToCheck = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1].GetComponent<TileObject>().sideEdgeRight.gameObject;

                if (spawner.tileSpanWidth <= 0)
                {
                    objectToCheckFrom = this.sideEdgeRight.gameObject;
                    distanceToCheckAccordingToExposure = spawner.overlapSpanAccordingtoExposure * 0.0254f;
                    distanceInInches = spawner.overlapSpanAccordingtoExposure;
                    print("A row of tiles placed now checking from the last tile to the above" + distanceToCheckAccordingToExposure);
                }
                else
                {
                    objectToCheckFrom = this.sideEdgeLeft.gameObject;
                    distanceToCheckAccordingToExposure = 0.18f * 0.0254f;
                    distanceInInches = 2f;
                }

            }
            // objectToCheck = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1];
            // objectToCheckFrom = this.sideEdgeRight.gameObject;
            // this.transform.position = spawner.currentTileRegion.transform.position;
            // objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
            if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck
            .transform.position) * 39.37 > distanceInInches)
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
                    DistanceErrorCubeRL.SetActive(true);
                    DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                    // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right (Bad keyway spacing) " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37 + "inches";
                    spawner.WriteOnHandMenu("Tile is on right by (Bad keyway spacing) " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                }
                if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                {
                    // Debug.Log("Target is to the Left.");
                    DistanceErrorCubeRL.SetActive(true);
                    DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                    // DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37 + "inches";
                    spawner.WriteOnHandMenu("Tile is on left by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                }
                if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                {
                    // Debug.Log("Target is Above (Forward).");
                    DistanceErrorCubeTB.SetActive(true);
                    DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                    // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37 + "inches";
                    spawner.WriteOnHandMenu("Tile is above by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                }
                if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                {
                    // Debug.Log("Target is Below (Backward).");
                    DistanceErrorCubeTB.SetActive(true);
                    DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                    // DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37 + "inches";
                    spawner.WriteOnHandMenu("Tile is down by " + (float)Math.Round(Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.transform.position) * 39.37, 2) + " inches");
                }
            }
            else
            {




                isPlacedCorrectlyAfterConfirmedPlacement = true;


                if (spawner.tileSpanWidth <= 0)
                {
                    // Get the current world position of the child object
                    Vector3 childWorldPosition = objectToCheckFrom.transform.position;

                    // Get the target position from the side edge
                    Vector3 targetPosition = objectToCheck.transform.position;

                    // Calculate the direction from the target point to the child's current position
                    Vector3 direction = (childWorldPosition - targetPosition).normalized;

                    // Calculate the new world position for the child in the local positive Y direction of the objectToCheck
                    Vector3 localYDirection = objectToCheck.transform.forward + new Vector3(0, 0.2f, 0); // This gets the local 'up' direction which corresponds to the local +Y axis
                    Vector3 newYDirectionWorldPosition = targetPosition + localYDirection * distanceToCheckAccordingToExposure;

                    // Combine the Y-direction offset with the original directional offset
                    Vector3 combinedDirection = localYDirection.normalized;
                    Vector3 newChildWorldPosition = targetPosition + combinedDirection * distanceToCheckAccordingToExposure;

                    // Calculate the required offset for the parent
                    Vector3 offset = newChildWorldPosition - childWorldPosition;

                    // Apply the offset to the parent to snap it
                    transform.position += offset;


                }
                else
                {
                    //    // Get the current world position of the child object
                    Vector3 childWorldPosition = objectToCheckFrom.transform.position;

                    // Calculate the direction from the target point to the child's current position
                    Vector3 direction = (childWorldPosition - objectToCheck.transform.position).normalized;
                    Vector3 newChildWorldPosition;
                    if (spawner.reverseTheLine)
                    {
                        print("Going right to left");

                        newChildWorldPosition = objectToCheck.transform.position + (objectToCheck.transform.right + new Vector3(0, 0.2f, 0)) * -1 * distanceToCheckAccordingToExposure;
                    }
                    else
                    {
                        print("Going left to right");
                        newChildWorldPosition = objectToCheck.transform.position + (objectToCheck.transform.right + new Vector3(0, 0.2f, 0)) * distanceToCheckAccordingToExposure;
                    }
                    // Calculate the required offset for the parent
                    Vector3 offset = newChildWorldPosition - childWorldPosition;

                    // Apply the offset to the parent to snap it
                    transform.position += offset;

                }
                spawner.WriteOnHandMenu("Tile Placed Correctly");
                DistanceErrorCubeRL.SetActive(false);
                DistanceErrorCubeTB.SetActive(false);
                CorrectTileIndicator.SetActive(true);
            }
        }



















        ///////////////////////////////////////Just for testing//////////////////////////////////////


        public void ShowShakeTIleErrorsTesting()
        {

            print("Distance Checking");
            GameObject objectToCheck = spawner.TilesPlaced[0];
            GameObject objectToCheckFrom;
            if (true)
            {
                objectToCheckFrom = this.sideEdgeRight.gameObject;
                objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
                if (/*objectToCheck.GetComponent<TileObject>().sideEdgeRight && isValidTile && isTileAbove*/isTileAbove)
                {
                    // this.transform.position = spawner.currentTileRegion.transform.position;
                    if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 > 2.3f)
                    {

                        // DistanceErrorCube.SetActive(true);
                        Vector3 direction = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                        direction.y = 0; // Ignore the Y-axis


                        print("Distance from place" + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) + "Direction" + direction);

                        // Determine the relative position
                        if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            // Debug.Log("Target is to the Right.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            // Debug.Log("Target is to the Left.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            // Debug.Log("Target is Above (Forward).");
                            DistanceErrorCubeTB.SetActive(true);
                            DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                            DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            // Debug.Log("Target is Below (Backward).");
                            DistanceErrorCubeTB.SetActive(true);
                            DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                            DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                    }
                    else
                    {
                        DistanceErrorCubeRL.SetActive(false);
                        DistanceErrorCubeTB.SetActive(false);
                    }
                }
                // else
                // {
                //     objectToCheck = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1];
                //     objectToCheckFrom = this.sideEdgeLeft.gameObject;
                //     // this.transform.position = spawner.currentTileRegion.transform.position;
                //     objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
                //     if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight
                //     .transform.position) * 39.37 > 0.18)
                //     {

                //         // DistanceErrorCube.SetActive(true);
                //         Vector3 direction = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                //         direction.y = 0; // Ignore the Y-axis


                //         print("Distance from place" + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) + "Direction" + direction);

                //         // Determine the relative position
                //         if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                //         {
                //             Debug.Log("Target is to the Right.");
                //             DistanceErrorCubeRL.SetActive(true);
                //             DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                //             DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right (Bad keyway spacing) " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                //         }
                //         else if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                //         {
                //             Debug.Log("Target is to the Left.");
                //             DistanceErrorCubeRL.SetActive(true);
                //             DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                //             DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                //         }
                //         else if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                //         {
                //             Debug.Log("Target is Above (Forward).");
                //             DistanceErrorCubeTB.SetActive(true);
                //             DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                //             DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                //         }
                //         else if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                //         {
                //             Debug.Log("Target is Below (Backward).");
                //             DistanceErrorCubeTB.SetActive(true);
                //             DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                //             DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                //         }
                //     }
                //     else
                //     {
                //         DistanceErrorCubeRL.SetActive(false);
                //         DistanceErrorCubeTB.SetActive(false);
                //     }

                // }




            }
        }
    }
}