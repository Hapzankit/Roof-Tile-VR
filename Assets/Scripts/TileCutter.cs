using UnityEngine;

namespace RoofTileVR.Utility
{
    public class TileCutter : MonoBehaviour
    {
        public float cutDistance = 1.0f;        // The width of the cut (distance from left or right)
        public bool cutFromLeft = true;         // Flag to decide which side to cut from
        
        private void Start()
        {
            //CutCube();
        }

        public void CutCube(MeshFilter meshFilter)
        {
            //MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh originalMesh = meshFilter.mesh;
                Vector3[] vertices = originalMesh.vertices;
                Vector3[] newVertices1, newVertices2;
                int[] triangles = originalMesh.triangles;

                // Separate the vertices into two groups
                if (cutFromLeft)
                {
                    newVertices1 = SplitMesh(vertices, cutDistance, true);
                    newVertices2 = SplitMesh(vertices, cutDistance, false);
                }
                else
                {
                    newVertices1 = SplitMesh(vertices, cutDistance, false);
                    newVertices2 = SplitMesh(vertices, cutDistance, true);
                }

                // Create new mesh for the left side
                GameObject leftSide = new GameObject("LeftSide");
                leftSide.AddComponent<MeshFilter>().mesh = CreateMeshFromVertices(newVertices1, triangles);
                leftSide.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;

                // Create new mesh for the right side
                GameObject rightSide = new GameObject("RightSide");
                rightSide.AddComponent<MeshFilter>().mesh = CreateMeshFromVertices(newVertices2, triangles);
                rightSide.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;

                // Optionally, animate the separation (e.g., move the two parts apart)
                // leftSide.transform.Translate(Vector3.left * 2);
                // rightSide.transform.Translate(Vector3.right * 2);
            }
        }

        Vector3[] SplitMesh(Vector3[] vertices, float cutDistance, bool leftSide)
        {
            // This is a simple approximation where we select vertices based on their X position
            Vector3[] newVertices = new Vector3[vertices.Length];
            int index = 0;

            foreach (var vertex in vertices)
            {
                if (leftSide && vertex.x <= cutDistance)
                {
                    newVertices[index] = vertex;
                    index++;
                }
                else if (!leftSide && vertex.x > cutDistance)
                {
                    newVertices[index] = vertex;
                    index++;
                }
            }
            return newVertices;
        }

        Mesh CreateMeshFromVertices(Vector3[] vertices, int[] triangles)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}