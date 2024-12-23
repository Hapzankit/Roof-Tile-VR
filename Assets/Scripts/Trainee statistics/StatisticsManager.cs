using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    // Start is called before the first frame update





    // AI-Powered Compliance Check and Summary Report:

    // Once a section is completed, use an AI module to analyze the installed roof section and provide a compliance summary.
    // Analysis Parameters: Overhang, keyway spacing, sidelap, fastener placement, and exposure.
    // Output: Visual report highlighting compliant and non-compliant sections, with a pass/fail indicator for each metric.




    // Performance Metrics and User Progress Tracking:

    // Track and record user actions, including errors made, corrections followed, and time taken per installation task.
    // Display a final performance score at the end of each training session to help users identify areas for improvement.


    // public TileCasting tileCasting;
    int IncorrectOverhangs = 0;
    int CorrectOverhangs = 0;

    int IncorrectKeyway = 0;
    int CorrectKeyway = 0;


    int IncorrectExposure = 0;
    int CorrectExposure = 0;

    int IncorrectSidelap = 0;
    int CorrectSidelap = 0;

    public float timeSpent = 0;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timeSpent += Time.deltaTime;
    }

    public void SaveStatistics(List<GameObject> tilesPlaced)
    {
        foreach (var tile in tilesPlaced)
        {
            if (tile.GetComponent<TileObject>().isPlacedCorrectlyAfterConfirmedPlacement)
            {
                CorrectExposure++;
                CorrectKeyway++;
                CorrectOverhangs++;
                CorrectSidelap++;
            }
            IncorrectKeyway += tile.GetComponent<TileObject>().InCorrectKeywaySpaces;
            IncorrectOverhangs += tile.GetComponent<TileObject>().InCorrectoverHangs;
            IncorrectExposure += tile.GetComponent<TileObject>().IncorrectExposure;
            IncorrectSidelap += tile.GetComponent<TileObject>().IncorrectSidelap;
        }
        print("Correct Exposure" + CorrectExposure + "Correct Keyway" + CorrectKeyway + "Correct Overhangs" + CorrectOverhangs + "Correct Sidelap" + CorrectSidelap + "\n");
        print("InCorrect Exposure" + IncorrectExposure + "InCorrect Keyway" + IncorrectKeyway + "InCorrect Overhangs" + IncorrectOverhangs + "InCorrect Sidelap" + IncorrectSidelap + "\n");
    }



}
