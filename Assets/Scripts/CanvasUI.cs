using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace RoofTileVR
{
    public class CanvasUI : MonoBehaviour
    {
        public TextMeshProUGUI tilePrompt;
        public GameObject container;

        private Vector3 tilePromptPos;
        
        private void Start()
        {
            container.gameObject.SetActive(false);
            tilePromptPos = tilePrompt.transform.parent.position;
        }

        private void OnEnable()
        {
            TileCasting.tilePlacedMsg += ToastTileMsg;
        }

        private void OnDisable()
        {
            TileCasting.tilePlacedMsg -= ToastTileMsg;
            
        }

        private void Update()
        {
            
        }

        public void ToastTileMsg(string msg)
        {
            container.gameObject.SetActive(true);
            var tileGameObject = tilePrompt.transform.parent.gameObject;
            if (tileGameObject.activeSelf)
            {
                tileGameObject.SetActive(true);
                tileGameObject.transform.DOKill();
                tilePrompt.text = msg;
                
                tileGameObject.transform.DOMoveX(-1f, 0.5f).OnComplete(() =>
                {
                    //tileGameObject.transform.DOMoveX(tilePromptX, 0.2f);
                    container.gameObject.SetActive(false);
                    
                    tileGameObject.transform.position = tilePromptPos;
                });
                
            }
        }
    }
}