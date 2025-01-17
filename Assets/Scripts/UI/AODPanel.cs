using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AODPanel : MonoBehaviour
{
    // Start is called before the first frame update

    public Image image;
    public TMP_Text writingText;
    void Start()
    {
        image = GetComponent<Image>();

        writingText = GetComponentInChildren<TMP_Text>();
        image.enabled = false;
        writingText.text = "";
    }


    public IEnumerator WriteTextForTime(float timeToShow, Color imageColor, string textToWrite)
    {
        print("Show panel");
        image.enabled = true;
        image.color = imageColor;
        writingText.text = textToWrite;
        writingText.color = imageColor;
        yield return new WaitForSeconds(timeToShow);
        image.enabled = false;
        writingText.text = "";
    }


}
