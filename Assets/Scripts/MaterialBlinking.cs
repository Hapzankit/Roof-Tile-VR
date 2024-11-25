using System;
using DG.Tweening;
using UnityEngine;

namespace RoofTileVR.Utility
{
    public class MaterialBlinking : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        
        [Range(0,1f)]
        public float blinkSpeed = 0.2f;
        [Range(0, 100)]
        public int colorFadePercentage = 0;
        private void OnEnable()
        {
            if (TryGetComponent(out meshRenderer))
            {
                meshRenderer.DOKill();
                meshRenderer.material.DOFade(colorFadePercentage/100f, blinkSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            }
            else
            {
                Debug.Log("MaterialBlinking=> please attach mesh model");
            }
        }

        private void OnDisable()
        {
            if (TryGetComponent(out meshRenderer))
            {
                meshRenderer.DOKill();
            }
        }
    }
}