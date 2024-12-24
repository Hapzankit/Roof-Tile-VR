using System.Collections;
using System.Collections.Generic;
using RoofTileVR;
using TMPro;
using UnityEngine;
using UnityEngine.UI.TableUI;

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
    public int CorrectOverhangs = 0;

    int IncorrectKeyway = 0;
    public int CorrectKeyway = 0;


    int IncorrectExposure = 0;
    public int CorrectExposure = 0;

    int IncorrectSidelap = 0;
    public int CorrectSidelap = 0;

    public float timeSpent = 0;

    public int boltsPlaced = 0;
    int boltsLeftToPlace = 0;

    public TableUI table;
    public Canvas StatsPage;
    public TMP_Text timeText;

    void Start()
    {
        StatsPage.gameObject.SetActive(false);
        // ShowStatsTable();
    }

    // Update is called once per frame
    void Update()
    {
        timeSpent += Time.deltaTime;
    }

    public void ShowStatsTable()
    {
        StatsPage.gameObject.SetActive(true);
        //starts at 1,1
        
        // OVERHANG
        float overhangPercentage = ((float)CorrectOverhangs / (CorrectOverhangs + IncorrectOverhangs)) * 100;
        table.GetCell(1, 1).text = CorrectOverhangs.ToString();
        table.GetCell(1, 2).text = IncorrectOverhangs.ToString();
        table.GetCell(1, 3).text = overhangPercentage.ToString("F2"); // Formats to 2 decimal places

        // KEYWAY
        float keywayPercentage = ((float)CorrectKeyway / (CorrectKeyway + IncorrectKeyway)) * 100;
        table.GetCell(2, 1).text = CorrectKeyway.ToString();
        table.GetCell(2, 2).text = IncorrectKeyway.ToString();
        table.GetCell(2, 3).text = keywayPercentage.ToString("F2");

        // EXPOSURE
        float exposurePercentage = ((float)CorrectExposure / (CorrectExposure + IncorrectExposure)) * 100;
        table.GetCell(3, 1).text = CorrectExposure.ToString();
        table.GetCell(3, 2).text = IncorrectExposure.ToString();
        table.GetCell(3, 3).text = exposurePercentage.ToString("F2");

        // SIDELAP
        float sidelapPercentage = ((float)CorrectSidelap / (CorrectSidelap + IncorrectSidelap)) * 100;
        table.GetCell(4, 1).text = CorrectSidelap.ToString();
        table.GetCell(4, 2).text = IncorrectSidelap.ToString();
        table.GetCell(4, 3).text = sidelapPercentage.ToString("F2");

        // BOLTS
        float boltPercentage = ((float)boltsPlaced / (boltsLeftToPlace + boltsPlaced)) * 100;
        table.GetCell(5, 1).text = boltsPlaced.ToString();
        table.GetCell(5, 2).text = boltsLeftToPlace.ToString();
        table.GetCell(5, 3).text = boltPercentage.ToString("F2");

        // TIME
        timeText.text = ConvertSecondsToHHMMSS((int)timeSpent);


    }

    string ConvertSecondsToHHMMSS(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        return $"<color=\"red\">{hours:D2}</color> H : <color=\"red\">{minutes:D2}</color> M : <color=\"red\">{seconds:D2}</color> S";
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
            if (!tile.GetComponent<TileObject>().isSecondBoltPlaced || !tile.GetComponent<TileObject>().isFirstBoltPlaced)
            {
                if (!tile.GetComponent<TileObject>().isSecondBoltPlaced && !tile.GetComponent<TileObject>().isFirstBoltPlaced)
                {
                    boltsLeftToPlace += 2;
                }
                else
                {
                    boltsLeftToPlace++;
                }
            }
        }
        print("Correct Exposure" + CorrectExposure + "Correct Keyway" + CorrectKeyway + "Correct Overhangs" + CorrectOverhangs + "Correct Sidelap" + CorrectSidelap + "\n");
        print("InCorrect Exposure" + IncorrectExposure + "InCorrect Keyway" + IncorrectKeyway + "InCorrect Overhangs" + IncorrectOverhangs + "InCorrect Sidelap" + IncorrectSidelap + "\n");
    }



}
