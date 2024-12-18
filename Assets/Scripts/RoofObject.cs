using UnityEngine;

namespace RoofTileVR
{
    public class RoofObject : MonoBehaviour
    {
        [SerializeField] private Transform leftEdge;
        [SerializeField] private Transform rightEdge;
        [SerializeField] private Transform bottomEdge;
        [SerializeField] private Transform topEdge;

        public Transform LeftRoofPoint => leftEdge;
        public Transform RightRoofPoint => rightEdge;
        public Transform BottomRoofPoint => bottomEdge;

        public GameObject SideOverhangEffect;
        public GameObject BottomOverhangEffect;
     
    }
}