using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{    
    public Text speedText;
    public RectTransform steeringWheel;
    public ProgressBar speedometer, tachometer;

    float vehicleOffset = 0f;
    // Update is called once per frame
    void Update()
    {        
        speedometer.UpdateParameters(WSClient.instance.speed);
        tachometer.UpdateParameters(WSClient.instance.rpm);
        
        if (vehicleOffset != WSClient.instance.vehicleOffset)
        {
            LeanTween.cancel(steeringWheel.gameObject);
            vehicleOffset = WSClient.instance.vehicleOffset;
            LeanTween.rotateLocal(steeringWheel.gameObject, new Vector3(15f, 0f, vehicleOffset * 50f), 1f).setEase(LeanTweenType.easeInOutCubic);
        }        
    }
}
