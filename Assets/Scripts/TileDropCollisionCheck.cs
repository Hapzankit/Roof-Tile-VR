using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RoofTileVR
{
    public class TileDropCollisionCheck : MonoBehaviour
    {
        public bool isStarterRegion;
        public bool isNormalRegion;
        private bool isTileNearRoof;


        void Start()
        {
            // tileCasting=FindObjectOfType<TileCasting>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (isStarterRegion)
            {
                if (other.TryGetComponent(out TileObject tile))
                {
                    if (tile.isStarter)
                    {
                        // Starter in starter region
                        print("Starter in starter region");
                        isTileNearRoof = true;
                        tile.SetTileAboveRoof(true, true, this);
                        // tileCasting.changeTiles();
                    }
                    else
                    {
                        isTileNearRoof = true;
                        tile.SetTileAboveRoof(true, false, this);
                        //Normal tile in starter region
                        print("Normal tile in starter region");
                    }
                }
            }
            else
            {
                if (other.TryGetComponent(out TileObject tile))
                {
                    if (tile.isStarter)
                    {
                        //starter in normal tile region
                        print("starter in normal tile region");
                        isTileNearRoof = true;
                        tile.SetTileAboveRoof(true, false, this);
                    }
                    else
                    {
                        isTileNearRoof = true;
                        tile.SetTileAboveRoof(true, true, this);
                        print("normal tile in normal tile region");
                        // normal tile in normal tile region
                    }
                }

            }
            // if (other.TryGetComponent(out TileObject tile))
            // {
            //     isTileNearRoof = true;
            //     tile.SetTileAboveRoof(true);
            // }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out TileObject tile))
            {
                isTileNearRoof = false;
                tile.SetTileAboveRoof(false, true, this);
            }
        }

        public bool TileAboveRoof()
        {
            return isTileNearRoof;
        }
        void Update()
        {


            // GetComponent<MeshRenderer>().enabled = isStarterRegion;

        }
    }
}