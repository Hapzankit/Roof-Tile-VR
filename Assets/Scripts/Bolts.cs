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
    public TMP_Text textToShowErrors;
    //1- check from 1st bolt placeholder
    //2- check from second bolt placeholder
    //-1 - all bolts are placed
    int num = 0;
    int tNum = 0;
    private void OnTriggerEnter(Collider other)
    {
        print("Enetered Tile");
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
        textToShowErrors.gameObject.SetActive(false);
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
    }

    void snapBolt(int tileNum)
    {
        print("trying to snap" + Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37);
        if (Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37 < 1f)
        {
            this.transform.position = tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position;
            textToShowErrors.gameObject.SetActive(false);
            if (tileNum == 0)
            {
                tileToCheckFrom.isFirstBoltPlaced = true;
            }
            if (tileNum == 1)
            {

                tileToCheckFrom.isSecondBoltPlaced = true;
            }
            this.GetComponent<XRGrabInteractable>().enabled = false;
        }
    }

    void CheckBoltDepth(int tileNum)
    {
        Vector3 direction = this.transform.position - tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position;

        double distanceInInches = Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37;
        if (Vector3.Distance(this.transform.position, tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position) * 39.37 > .5f)
        {
            textToShowErrors.gameObject.SetActive(true);
            if (direction.z > 0 || direction.z < 0)
            {
                textToShowErrors.text = "Bolt is not deep enough " + distanceInInches;
            }
            else if (direction.x > 0)
            {
                textToShowErrors.text = "Bolt is off by" + distanceInInches + "to the right";
            }
            else if (direction.x < 0)
            {
                textToShowErrors.text = "Bolt is off by" + distanceInInches + "to the left";
            }
            else if (direction.y < 0)
            {
                textToShowErrors.text = "Bolt is off by" + distanceInInches + "to the bottom";
            }
            else if (direction.y > 0)
            {

                textToShowErrors.text = "Bolt is off by" + distanceInInches + "to the top";
            }
        }
        else
        {
            print("Bolt placed");
            this.transform.position = tileToCheckFrom.BoltPlaceHolders[tileNum].transform.position;
            textToShowErrors.gameObject.SetActive(false);
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
