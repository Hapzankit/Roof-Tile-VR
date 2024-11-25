using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace RoofTileVR
{
    public class TileObject : MonoBehaviour
    {
        public GameObject tileCanvasUI;

        public XRBaseInteractable Interactable;
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
        }

        public void OnTilePicked()
        {
            spawner.TileSelectText("Tile Picked");
            spawner.OnTilePick();
        }
        
        public void OnTileDropped()
        {
            //ShowPlacementPrompt();
            spawner.TileSelectText("Tile Dropped");
            spawner.OnTileDropped();
            //spawner.ShowPlacementPrompt();
            //DisableInteraction();
        }

        public bool isTileAbove = false;
        public void SetTileAboveRoof(bool isAbove)
        {
            isTileAbove = isAbove;
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
    }
}