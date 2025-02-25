
using System;
using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BoltMachine : MonoBehaviour
{
    // Start is called before the first frame update
    public Bolts bolts;
    public Transform BoltPosition;
    Bolts currentBolt;
    public TileCasting tileCasting;
    public GameObject NailerObject;
    public Renderer nailRenderer;


    void Start()
    {
        currentBolt = Instantiate(bolts, BoltPosition);
        nailRenderer = NailerObject.GetComponent<Renderer>();
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

    public void HighlightNailer()
    {
        InvokeRepeating("ChangeColor", 0.5f, 0.5f);
    }

    void ChangeColor()
    {
        if (nailRenderer.material.color == Color.green)
        {
            nailRenderer.material.color = Color.white;
        }
        else
        {
            nailRenderer.material.color = Color.green;
        }
    }
    public void StopHighlight()
    {

        CancelInvoke("ChangeColor");
        nailRenderer.material.color = Color.white;
    }
}
