using UnityEngine;

namespace RoofTileVR.UI
{
    public class TileSpawnerPanelUI : MonoBehaviour
    {
        private int index = -1;

        public static event System.Action<int> TileSelected;

        public void TileSelect(int idx)
        {
            if (index == idx)   return;

            index = idx;
            TileSelected?.Invoke(idx);
            
        }
    }
}