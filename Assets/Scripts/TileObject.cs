﻿using System;
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

        private Vector3 effectSideEdgeScale = new Vector3(0.1f, 1.20f, 1f);
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
        
        private void Start()
        {
            spawner = FindObjectOfType<TileCasting>();
            YesButton.onClick.AddListener(YesButtonPressed);
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
            spawner.ShowPlacementPrompt();
            spawner.TileSelectText("Tile Dropped");
            DisableInteraction();
        }
    }
}