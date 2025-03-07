using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bend : MonoBehaviour
{
    public enum BendAxis { X, Y, Z };

    public float rotate = 90;
    public float fromPosition = 0.5F; //from 0 to 1
    public BendAxis axis = BendAxis.X;
    Mesh mesh;
    Vector3[] vertices;

    void Start()
    {

        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        BendTheTile(1);
    }


    public void BendTheTile(int rowNum)
    {

        rotate = rotate * rowNum;
        float meshWidth = mesh.bounds.size.z;
        for (var i = 0; i < vertices.Length; i++)
        {
            float formPos = Mathf.Lerp(meshWidth / 2, -0.9f, fromPosition);
            float zeroPos = vertices[i].z + formPos;
            float rotateValue = (-rotate / 2) * (zeroPos / meshWidth);

            zeroPos -= 2 * vertices[i].y * Mathf.Cos((90 - rotateValue) * Mathf.Deg2Rad);

            vertices[i].y += zeroPos * Mathf.Sin(rotateValue * Mathf.Deg2Rad);
            vertices[i].z = zeroPos * Mathf.Cos(rotateValue * Mathf.Deg2Rad) - formPos;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }


}