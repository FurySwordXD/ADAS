using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vehicle : MonoBehaviour
{
    public int id;    
    public string type;
    public float speed = 0f;
    
    public Text speedText;

    public GameObject brakes;
    // Start is called before the first frame update

    private Queue<float> speedWindow = new Queue<float>();

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {        
    }    


    void CalculateSpeed(float baseSpeed)
    {
        float speedSum = 0;
        foreach (float s in speedWindow)
        {
            speedSum += s;
        }
        speed = speedSum / speedWindow.Count;
        speedText.text = (Mathf.Round(speed).ToString() + " km/h");

        brakes.SetActive(speed < -5f);
    }

    public void UpdateParameters(float baseSpeed, Vector3 location)
    {
        float newSpeed = ( (location.z - transform.position.z)  / Time.deltaTime / 10f );

        speedWindow.Enqueue(newSpeed);        
        while (speedWindow.Count > Math.Round(1 / Time.deltaTime))
            speedWindow.Dequeue();

        CalculateSpeed(baseSpeed);
        
        transform.position = Vector3.Lerp(transform.position, location, 5f * Time.deltaTime);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
