using System;
using DG.Tweening;
using UnityEngine;

namespace RoofTileVR.Utility
{
    public class MaterialBlinking : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        
        private void OnEnable()
        {
            if (TryGetComponent(out meshRenderer))
            {
                meshRenderer.DOKill();
                meshRenderer.material.DOFade(0f, 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
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