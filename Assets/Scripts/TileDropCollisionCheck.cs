using System;
using UnityEngine;

namespace RoofTileVR
{
    public class TileDropCollisionCheck : MonoBehaviour
    {
        private bool isTileNearRoof;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out TileObject tile))
            {
                isTileNearRoof = true;
                tile.SetTileAboveRoof(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out TileObject tile))
            {
                isTileNearRoof = false;
                tile.SetTileAboveRoof(false);
            }
        }

        public bool TileAboveRoof()
        {
            return isTileNearRoof;
        }
    }
}