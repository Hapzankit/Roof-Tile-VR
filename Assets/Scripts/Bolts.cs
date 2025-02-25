using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Bolts : MonoBehaviour
{

    bool checkDistance = false;

    TileObject tileToCheckFrom;
    // public TMP_Text textToShowErrors;

    public bool isBoltPlaced = false;
    //1- check from 1st bolt placeholder
    //2- check from second bolt placeholder
    //-1 - all bolts are placed
    int num = 0;
    int tNum = 0;
    private void OnTriggerEnter(Collider other)
    {
        // print("Enetered Tile");
        if (other.gameObject.TryGetComponent<TileObject>(out TileObject tileToPlaceBolt))
        {
            tileToCheckFrom = tileToPlaceBolt;
            if (tileToPlaceBolt.isFirstBoltPlaced && tileToPlaceBolt.isSecondBoltPlaced)
            {
                //Dont check it
                num = -1;
            }
            else
            {
                if (!tileToPlaceBolt.isFirstBoltPlaced)
                {
                    num = 1;
                    tNum = 0;

                }
                else
                {
                    num = 2;
                    tNum = 1;
                }
            }
        }
    }

    public void OnBoltPicked()
    {
        // textToShowErrors.gameObject.SetActive(false);
        this.transform.rotation = Quaternion.Euler(0, 90, 135);
    }

    public void OnBoltDropped()
    {
        switch (num)
        {
            case 1:
                CheckBoltDepth(0);
                break;
            case 2:
                CheckBoltDepth(1);
                break;
            case -1:
                break;
            default: print("No tile found"); break;
        }
    }

    void Update()
    {
        // CheckBoltDepth(0);
        if (tileToCheckFrom)
        {
            snapBolt(tNum);
        }
        else
        {
            this.transform.localPosition = Vector3.zero;
        }
    }

    void snapBolt(int tileNum)
    {
        if (num != -1)
        {

            // print("trying to snap" + Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37);
            if (Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37 < 4f && tileToCheckFrom.BoltPlaceHolders[tileNum].gameObject.activeInHierarchy)
            {
                this.transform.position = tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position;
                tileToCheckFrom.BoltPlaceHolders[tileNum].GetComponentInChildren<MeshRenderer>().enabled = false;
                // Vector3 ogSize = this.transform.localScale;
                this.transform.SetParent(tileToCheckFrom.BoltPlaceHolders[tileNum], false);
                // this.transform.localScale = ogSize;
                // this.transform.SetParent(this.transform);
                this.transform.localRotation = Quaternion.Euler(0, 90, 180);
                this.transform.localPosition = new Vector3(0, 0, 0);
                // this.transform.localScale = new Vector3(1, 1, 1);
                isBoltPlaced = true;
                // textToShowErrors.gameObject.SetActive(false);
                if (tileNum == 0)
                {
                    tileToCheckFrom.isFirstBoltPlaced = true;
                }
                if (tileNum == 1)
                {

                    tileToCheckFrom.isSecondBoltPlaced = true;
                }
                this.GetComponent<XRGrabInteractable>().enabled = false;
                this.enabled = false;
            }
            else
            {
                this.transform.localPosition = Vector3.zero;
            }
        }
    }

    void CheckBoltDepth(int tileNum)
    {
        Vector3 direction = this.transform.position - tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position;

        double distanceInInches = Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37;
        if (Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37 > .5f)
        {
                this.transform.localPosition = Vector3.zero;
        }
        else
        {
            print("Bolt placed");
            this.transform.position = tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position;

            // textToShowErrors.gameObject.SetActive(false);
            if (tileNum == 0)
            {
                tileToCheckFrom.isFirstBoltPlaced = true;

            }
            if (tileNum == 1)
            {

                tileToCheckFrom.isSecondBoltPlaced = true;
            }
        }
    }

}
