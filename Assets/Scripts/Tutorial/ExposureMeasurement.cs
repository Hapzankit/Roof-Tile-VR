using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposureMeasurement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Exposure10;
    public GameObject Exposure8;


    public void TurnOn10()
    {
        Exposure10.SetActive(true);
        Exposure8.SetActive(false);
    }

    public void TurnOn8()
    {
        Exposure8.SetActive(true);
        Exposure10.SetActive(false);
    }

    public IEnumerator ShowTileExposure(float delay)
    {
        TurnOn10();
        yield return new WaitForSeconds(delay);
        TurnOn8();
        yield return new WaitForSeconds(3);
        Exposure10.SetActive(false);
        Exposure8.SetActive(false);
    }


}
