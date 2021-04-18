using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    
    public float maxValue;

    //public float value;

    public Image mask;
    public Text valueText;

    float ConvertRange(float value, float oldMin, float oldMax, float minValue, float maxValue)
    {
        return ( (value - oldMin) / (oldMax - oldMin) ) * (maxValue - minValue) + minValue;
    }
    
    public void UpdateParameters(float value)
    {
        valueText.text = Mathf.Round(value).ToString();
        mask.fillAmount = ConvertRange(value / maxValue, 0f, 1f, .12f, .88f);        
    }    
}
