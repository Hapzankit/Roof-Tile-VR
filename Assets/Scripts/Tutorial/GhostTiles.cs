using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTiles : MonoBehaviour
{
    public void ShowTiles()
    {
        this.gameObject.SetActive(true);
    }

    public void HideTiles()
    {
        this.gameObject.SetActive(false);
    }
}
