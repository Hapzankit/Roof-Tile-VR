using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public enum TutorialType
    {
        PickUpTile, NailTile, None
    }
    public GameObject tutorialHand;
    public TutorialType tutorialType;
    TileObject tile;
    void Start()
    {
        tutorialHand.gameObject.SetActive(false);
        tile = GetComponent<TileObject>();
    }

    public void SetHandActive(bool setActive)
    {
        tutorialHand.gameObject.SetActive(setActive);
    }

    public void ConfigureAnimation()
    {
        if (tile.isStarter)
        {
            if (tile.isPlaced)
            {
                tutorialType = TutorialType.NailTile;
            }
            else
            {
                tutorialType = TutorialType.PickUpTile;
            }

            if (tile.isTileGrabbed || tile.isBothFastnersplaced)
            {
                tutorialType = TutorialType.None;
                PauseEveryAnimation();
            }
        }
    }

    // Update is called once per frame
    public void PlayPauseAnimation(bool shouldPlay)
    {




        if (tutorialType == TutorialType.PickUpTile)
        {
            // PauseEveryAnimation();
            SetHandActive(shouldPlay);
            tutorialHand.GetComponent<Animator>().SetBool("showPick", shouldPlay);
        }
        else if (tutorialType == TutorialType.NailTile)
        {
            // PauseEveryAnimation();
            SetHandActive(shouldPlay);
            tutorialHand.GetComponent<Animator>().SetBool("showNailing", shouldPlay);
        }
        else if (tutorialType == TutorialType.None)
        {
            PauseEveryAnimation();
        }

    }

    public void PauseEveryAnimation()
    {
        tutorialHand.GetComponent<Animator>().SetBool("showPick", false);
        tutorialHand.GetComponent<Animator>().SetBool("showNailing", false);
        SetHandActive(false);
    }

    void Update()
    {
        if (tile)
        {
            ConfigureAnimation();
        }
    }
}
