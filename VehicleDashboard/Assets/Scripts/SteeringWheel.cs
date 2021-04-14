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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
                
        // LeanTween.cancel(steeringWheelRectTransform.gameObject);
        // LeanTween.cancel(steeringWheelTransform.gameObject);

        LeanTween.rotate(steeringWheelRectTransform.gameObject, new Vector3(0f, 0f, vehicleOffset * 40f), 1f).setEase(LeanTweenType.easeInOutCubic);
        LeanTween.rotateLocal(steeringWheelTransform.gameObject, new Vector3(15f, 0f, vehicleOffset * 40f), 1f).setEase(LeanTweenType.easeInOutCubic);
        //steeringWheelRectTransform.eulerAngles = new Vector3(0f, 0f, vehicleOffset * -50f);
        //float steeringAngle = Mathf.Lerp(steeringWheelTransform.rotation.z, vehicleOffset * -50f, Time.deltaTime);
        // float steeringAngle = LeanTween.easeInOutCubic(steeringWheelTransform.rotation.z, vehicleOffset * -50f, Time.deltaTime);
        // steeringWheelTransform.rotation = Quaternion.EulerAngles(0f, 0f, steeringAngle);        
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
        //steeringWheelRectTransform.eulerAngles = new Vector3(0f, 0f, vehicleOffset * -50f);
        //LeanTween.rotateZ(steeringWheelRectTransform.gameObject, vehicleOffset * -50f, 0.1f);
        
        // LeanTween.cancel(steeringWheelRectTransform.gameObject);
        // LeanTween.cancel(steeringWheelTransform.gameObject);

        // LeanTween.rotate(steeringWheelRectTransform.gameObject, new Vector3(0f, 0f, vehicleOffset * 400f), 1f);//.setEase(LeanTweenType.easeInOutCubic);
        // LeanTween.rotateLocal(steeringWheelTransform.gameObject, new Vector3(15f, 0f, vehicleOffset * 400f), 1f);
        
    }
}
