using System;
using System.Collections.Generic;
using RoofTileVR.UI;
using UnityEngine;

namespace RoofTileVR
{
    public class TileSpawner : MonoBehaviour
    {
        private int currentIndex;

        [SerializeField] private List<GameObject> tileList;
        [SerializeField] private Transform spawnPoint;
        
        private void OnEnable()
        {
            TileSpawnerPanelUI.TileSelected += OnTileSpawnerPanelUISelected;
        }

        private void OnDisable()
        {
            TileSpawnerPanelUI.TileSelected -= OnTileSpawnerPanelUISelected;
        }

        private void OnTileSpawnerPanelUISelected(int index)
        {
            currentIndex = index;
            
        }

        private void SpawnTile()
        {
            var newTile = Instantiate(tileList[currentIndex]);
            
        }
    }
}