using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDimensionsIntro : MonoBehaviour
{
    // Start is called before the first frame update
    // Update is called once per frame

    public GameObject dimensions;
    public GameObject starterDimensions;
    void Update()
    {
        dimensions.SetActive(AudioHandler.Instance.showDimensions);
        starterDimensions.SetActive(AudioHandler.Instance.showStarterDimensions);
    }
}
