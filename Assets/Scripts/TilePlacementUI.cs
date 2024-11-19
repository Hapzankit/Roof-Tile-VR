using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace RoofTileVR.UI
{
    public class TilePlacementUI : MonoBehaviour
    {
        public GameObject panel;
        public TextMeshProUGUI promptLabel;
        public Camera cam;

        public string passRemark = "Tile Placed correctly, Good!";
        public string failRemark = "Tile Placed correctly, Good!";
        [SerializeField] private GameObject failGraphics;
        [SerializeField] private GameObject passGraphics;
        [SerializeField] private Vector3 offsetPos;
        private void Start()
        {
            cam = Camera.main;
            failGraphics.SetActive(false);
            passGraphics.SetActive(false);
        }

        public void ShowTilePassUI(Transform target)
        {
            //Vector3 offsetPos = panel.transform.localPosition;
            
            panel.transform.SetParent(target);
            promptLabel.text = passRemark;
            panel.transform.localPosition = Vector3.zero + offsetPos;
            Vector3 oldScale = panel.transform.localScale;
            
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(true);
            panel.transform.DOScale(oldScale, 1f).SetEase(Ease.OutBounce);
            
            passGraphics.transform.localScale = Vector3.zero;
            passGraphics.SetActive(true);
            passGraphics.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce);
            
        }
        
        public void ShowTileFailUI(Transform target)
        {
            //Vector3 offsetPos = panel.transform.localPosition;
            
            panel.transform.SetParent(target);
            promptLabel.text = failRemark;
            panel.transform.localPosition = Vector3.zero + offsetPos;
            Vector3 oldScale = panel.transform.localScale;
            
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(true);
            panel.transform.DOScale(oldScale, 1f).SetEase(Ease.OutBounce);
            
            failGraphics.transform.localScale = Vector3.zero;
            failGraphics.SetActive(true);
            failGraphics.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce);
            
        }

        public bool HidePanel()
        {
            if (panel.activeSelf == false)
            {
                return false;
            }
            panel.transform.DOKill();
            panel.gameObject.SetActive(false);
            passGraphics.SetActive(false);
            failGraphics.SetActive(false);
            return true;
        }
        
        public void Update()
        {
            if (panel.activeSelf && cam)
            {
                //panel.transform.DOLookAt(panel.transform.position - cam.transform.position, 0.1f, AxisConstraint.None, Vector3.up).SetEase(Ease.InOutSine);
                
                var rotationAngle = Quaternion.LookRotation ( panel.transform.position - cam.transform.position); // we get the angle has to be rotated
                panel.transform.rotation =
                    Quaternion.Slerp(panel.transform.rotation, rotationAngle, Time.deltaTime * 5f);
            }
        }
    }
}