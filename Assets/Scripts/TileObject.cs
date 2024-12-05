using System;
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


        public bool isInStarterRegion; // Tracks if the tile is currently in a starter region

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

            DistanceErrorCubeRL.gameObject.SetActive(false);
            DistanceErrorCubeTB.gameObject.SetActive(false);


        }

        public void OnTileDropped()
        {
            //ShowPlacementPrompt();
            spawner.TileSelectText("Tile Dropped" + isTileAbove + isValidTile);
            spawner.OnTileDropped();
            ShowStarterErrors();
            //spawner.ShowPlacementPrompt();
            //DisableInteraction();
        }

        void Update()
        {
            // ShowStarterErrors();
            if (!isStarter)
            {

                ShowShakeTIleErrorsTesting();
            }
        }

        public void ShowStarterErrors()
        {
            if (spawner.currentTileRegion && isValidTile && isTileAbove)
            {
                // this.transform.position = spawner.currentTileRegion.transform.position;
                if (Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 > 1.1f)
                {

                    // DistanceErrorCube.SetActive(true);
                    Vector3 direction = this.transform.position - spawner.currentTileRegion.transform.position;
                    direction.y = 0; // Ignore the Y-axis


                    print("Distance from place" + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) + "Direction" + direction);

                    // Determine the relative position
                    if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                    {
                        Debug.Log("Target is to the Right.");
                        DistanceErrorCubeRL.SetActive(true);
                        DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                        DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                    }
                    else if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                    {
                        Debug.Log("Target is to the Left.");
                        DistanceErrorCubeRL.SetActive(true);
                        DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                        DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                    }
                    else if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                    {
                        Debug.Log("Target is Above (Forward).");
                        DistanceErrorCubeTB.SetActive(true);
                        DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                        DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                    }
                    else if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                    {
                        Debug.Log("Target is Below (Backward).");
                        DistanceErrorCubeTB.SetActive(true);
                        DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeBottom.localPosition.x, sideEdgeBottom.localPosition.y, sideEdgeBottom.localPosition.z);
                        DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is down " + Vector3.Distance(this.transform.position, spawner.currentTileRegion.transform.position) * 39.37 + "inches";
                    }
                }
                else
                {
                    DistanceErrorCubeRL.SetActive(false);
                    DistanceErrorCubeTB.SetActive(false);
                }



            }
        }

        public bool isTileAbove = false;
        public bool isValidTile = false;
        public void SetTileAboveRoof(bool isAbove, bool isTileValid, TileDropCollisionCheck regionTileDropped)
        {
            // ShowPlacementPrompt();
            // spawner.ShowPlacementPrompt();
            isTileAbove = isAbove;
            isValidTile = isTileValid;
            spawner.currentTileRegion = regionTileDropped;
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





        public void ShowShakeTIleErrors(bool isfirstShakeTile)
        {

            print("Distance Checking");
            GameObject objectToCheck = spawner.TilesPlaced[0];
            GameObject objectToCheckFrom;
            if (isfirstShakeTile)
            {
                objectToCheckFrom = this.sideEdgeRight.gameObject;
                objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
                if (objectToCheck.GetComponent<TileObject>().sideEdgeRight && isValidTile && isTileAbove/*true*/)
                {
                    // this.transform.position = spawner.currentTileRegion.transform.position;
                    if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 > 5.3f)
                    {

                        // DistanceErrorCube.SetActive(true);
                        Vector3 direction = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                        direction.y = 0; // Ignore the Y-axis


                        print("Distance from place" + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) + "Direction" + direction);

                        // Determine the relative position
                        if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            Debug.Log("Target is to the Right.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            Debug.Log("Target is to the Left.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            Debug.Log("Target is Above (Forward).");
                            DistanceErrorCubeTB.SetActive(true);
                            DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                            DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            Debug.Log("Target is Below (Backward).");
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
                else
                {
                    objectToCheck = spawner.TilesPlaced[spawner.TilesPlaced.Count - 1];
                    objectToCheckFrom = this.sideEdgeLeft.gameObject;
                    // this.transform.position = spawner.currentTileRegion.transform.position;
                    objectToCheck.GetComponent<TileObject>().CorrectTileIndicator.SetActive(true);
                    if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight
                    .transform.position) * 39.37 > 0.18)
                    {

                        // DistanceErrorCube.SetActive(true);
                        Vector3 direction = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                        direction.y = 0; // Ignore the Y-axis


                        print("Distance from place" + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) + "Direction" + direction);

                        // Determine the relative position
                        if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            Debug.Log("Target is to the Right.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right (Bad keyway spacing) " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            Debug.Log("Target is to the Left.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            Debug.Log("Target is Above (Forward).");
                            DistanceErrorCubeTB.SetActive(true);
                            DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                            DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            Debug.Log("Target is Below (Backward).");
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




            }
        }




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
                    if (Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 > 5.3f)
                    {

                        // DistanceErrorCube.SetActive(true);
                        Vector3 direction = objectToCheckFrom.transform.position - objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position;
                        direction.y = 0; // Ignore the Y-axis


                        print("Distance from place" + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) + "Direction" + direction);

                        // Determine the relative position
                        if (direction.x > 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            Debug.Log("Target is to the Right.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeRight.localPosition.x, sideEdgeRight.localPosition.y, sideEdgeRight.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on right " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            Debug.Log("Target is to the Left.");
                            DistanceErrorCubeRL.SetActive(true);
                            DistanceErrorCubeRL.transform.localPosition = new Vector3(sideEdgeLeft.localPosition.x, sideEdgeLeft.localPosition.y, sideEdgeLeft.localPosition.z);
                            DistanceErrorCubeRL.GetComponentInChildren<TMP_Text>().text = "Tile is on left " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z > 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            Debug.Log("Target is Above (Forward).");
                            DistanceErrorCubeTB.SetActive(true);
                            DistanceErrorCubeTB.transform.localPosition = new Vector3(sideEdgeTop.localPosition.x, sideEdgeTop.localPosition.y, sideEdgeTop.localPosition.z);
                            DistanceErrorCubeTB.GetComponentInChildren<TMP_Text>().text = "Tile is above " + Vector3.Distance(objectToCheckFrom.transform.position, objectToCheck.GetComponent<TileObject>().sideEdgeRight.transform.position) * 39.37 + "inches";
                        }
                        else if (direction.z < 0 && Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
                        {
                            Debug.Log("Target is Below (Backward).");
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