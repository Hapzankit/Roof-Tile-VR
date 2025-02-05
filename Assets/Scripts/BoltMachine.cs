
using System;
using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using UnityEngine;
using UnityEngine.UIElements;

public class BoltMachine : MonoBehaviour
{
    // Start is called before the first frame update
    public Bolts bolts;
    public Transform BoltPosition;
    Bolts currentBolt;
    public TileCasting tileCasting;


    void Start()
    {
        currentBolt = Instantiate(bolts, BoltPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentBolt.isBoltPlaced)
        {
            SpawnAnotherBolt();
        }
    }


    public void SpawnAnotherBolt()
    {
        print("Spawn bolt");
        // tileCasting.WriteOnHandMenu("Fastener placed.");
        tileCasting.statisticsManager.boltsPlaced++;
        currentBolt = Instantiate(bolts, BoltPosition);
    }

    void OnCollisionEnter(Collision collision)
    {

        gameObject.layer = LayerMask.NameToLayer("NailMachine");

    }
    void OnCollisionExit(Collision collision)
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
