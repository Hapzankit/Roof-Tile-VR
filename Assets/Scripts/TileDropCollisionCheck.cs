using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RoofTileVR
{
    public class TileDropCollisionCheck : MonoBehaviour
    {
        public bool isStarterRegion;
        private bool isTileNearRoof;

        TileCasting tileCasting;


        void Start()
        {
            tileCasting = FindObjectOfType<TileCasting>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out TileObject tile))
                return; // Exit early if no TileObject is found

            // Check if the tile is already in a starter region
            if (tile.isInStarterRegion)
            {
                print("Tile is already in a starter region, ignoring other regions.");
                if (tile.isStarter)
                {
                    // Starter in starter region
                    print("Starter in starter region");
                    isTileNearRoof = true;
                    tile.SetTileAboveRoof(true, true, this);
                }
                else
                {
                    // Normal tile in starter region
                    print("Normal tile in starter region");
                    isTileNearRoof = true;
                    tile.SetTileAboveRoof(true, false, this);
                }
                return;
            }

            if (isStarterRegion)
            {
                // Handle tiles entering a starter region
                tile.isInStarterRegion = true; // Lock the tile to the starter region
                if (tile.isStarter)
                {
                    // Starter in starter region
                    print("Starter in starter region");
                    isTileNearRoof = true;
                    tile.SetTileAboveRoof(true, true, this);
                }
                else
                {
                    // Normal tile in starter region
                    print("Normal tile in starter region");
                    isTileNearRoof = true;
                    tile.SetTileAboveRoof(true, false, this);
                }
            }
            else
            {
                // Handle tiles in non-starter regions
                if (tile.isStarter)
                {
                    // Starter in normal tile region
                    print("Starter in normal tile region");
                    isTileNearRoof = true;
                    tile.SetTileAboveRoof(true, false, this);
                }
                else
                {
                    if (tileCasting.starterTilesPlaced)
                    {
                        // Normal tile in normal tile region after starter tiles placed
                        print("Normal tile in normal tile region");
                        isTileNearRoof = true;
                        tile.SetTileAboveRoof(true, true, this);
                    }
                }
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out TileObject tile))
            {
                isTileNearRoof = false;
                tile.SetTileAboveRoof(false, false, this);
                tile.RemoveIndications();
            }

            if (isStarterRegion && other.TryGetComponent(out TileObject tile2))
            {
                tile2.isInStarterRegion = false; // Allow the tile to react to other regions again
                print("Tile left the starter region.");
            }
        }



        public bool TileAboveRoof()
        {
            return isTileNearRoof;
        }
    }
}