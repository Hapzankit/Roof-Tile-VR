using System;
using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    private TileCasting spawner;
    public TileObject activeTile = null;
    
    private void Start()
    {
        spawner = FindObjectOfType<TileCasting>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out TileObject tileObject))
        {
            if (tileObject == spawner.CurrentTileObject)
            {
                activeTile = tileObject;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TileObject tileObject))
        {
            if (tileObject == activeTile)
            {
                activeTile = null;
            }
        }
    }
}
