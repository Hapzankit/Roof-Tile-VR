
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltMachine : MonoBehaviour
{
    // Start is called before the first frame update
    public Bolts bolts;
    public Transform BoltPosition;
    Bolts currentBolt;


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
        currentBolt = Instantiate(bolts, BoltPosition);
    }
}
