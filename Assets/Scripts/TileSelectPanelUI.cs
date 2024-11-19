using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoofTileVR.UI
{
    public class TileSelectPanelUI : MonoBehaviour
    {

        public Image panelBg;
        public TextMeshProUGUI captionText;
        public Transform marker;

        public Transform tileIcon;
        [SerializeField] private string selectedText = "Now place the selected tile on top of roof surface";
        [SerializeField] private string deselectedText = "Select any tile to start placing on  roof surface";
        private Color colorOrigin;
        
        private void Start()
        {
            marker.gameObject.SetActive(false);
            colorOrigin = panelBg.color;
            
        }

        public void OnPanelSelect(bool raycastToggle, int index)
        {
            panelBg.DOKill();
            if (raycastToggle)
            {
                panelBg.DOColor(Color.green, 0.1f);
                Vector3 markPos = marker.localPosition;
                marker.parent = tileIcon;
                marker.localPosition = markPos;
                marker.gameObject.SetActive(true);
                captionText.text = selectedText;
            }
            else
            {
                captionText.text = deselectedText;
                marker.gameObject.SetActive(false);
                panelBg.DOColor(Color.red, 0.5f);
                
            }
        }

        public void BGColorChange(Color color)
        {
            panelBg.DOColor(color, 1f).SetEase(Ease.InOutSine);
        }

        public void BGColorReset()
        {
            BGColorChange(colorOrigin);
        }
    }
}