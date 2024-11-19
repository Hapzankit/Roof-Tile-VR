using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RoofTileVR;
using UnityEngine;

public class ToggleSpawnButton : MonoBehaviour
{
    public GameObject placeTileBeforeWarn;

    private TileCasting spawner;
    // Start is called before the first frame update
    void Start()
    {
        spawner = FindObjectOfType<TileCasting>();
        placeTileBeforeWarn.SetActive(false);
    }

    public void OnWristButtonPressed(int index = 0)
    {
        if (spawner.CheckCurrentTileState())
        {
            spawner.PlaceTileInFront(index);
        }
        else
        {
            DOVirtual.DelayedCall(0f, () =>
            {
                placeTileBeforeWarn.SetActive(true);
            }).OnComplete((() =>
            {
                DOVirtual.DelayedCall(2f, () =>
                {
                    placeTileBeforeWarn.SetActive(false);
                });
            }));
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
