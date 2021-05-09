using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SteeringWheel : MonoBehaviour
{
    public float vehicleOffset = 0f;
    public RectTransform steeringWheelRectTransform;
    public Transform steeringWheelTransform;
    // Start is called before the first frame update

    public Queue<float> angleWindow = new Queue<float>();

    LeanTweenType easeType = LeanTweenType.easeInOutCubic;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    void CalculateAverageVehicleOffset()
    {
        float s = 0;        
        foreach (float vo in angleWindow)
            s += vo;
        vehicleOffset = s / angleWindow.Count;
    }

    public void UpdateParameters(float vehicleOffset)
    {        
        // angleWindow.Enqueue(vehicleOffset);
        // while(angleWindow.Count > Mathf.Round(1 / Time.deltaTime))
        //     angleWindow.Dequeue();
        
        //CalculateAverageVehicleOffset();

        this.vehicleOffset = vehicleOffset;        
        
        // LeanTween.cancel(steeringWheelRectTransform.gameObject);
        // LeanTween.cancel(steeringWheelTransform.gameObject);

        // LeanTween.rotateLocal(steeringWheelRectTransform.gameObject, new Vector3(0f, 0f, vehicleOffset * 50f), 2f).setEase(easeType);
        // LeanTween.rotateLocal(steeringWheelTransform.gameObject, new Vector3(15f, 0f, vehicleOffset * 50f), 2f).setEase(easeType);

        steeringWheelRectTransform.localEulerAngles = new Vector3(0f, 0f, vehicleOffset * 50f);
        steeringWheelTransform.localEulerAngles = new Vector3(15f, 0f, vehicleOffset * 50f);
    }
}
