using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

[System.Serializable]
public class DetectedObject
{
    public int id;
    public string class_label;
    public float center_x;
    public float center_y;
}

[System.Serializable]
public class JSONRoot
{
    public DetectedObject[] objects;    
}

public class WSClient : MonoBehaviour
{    
    public Camera camera;

    public float xCorrection = 1f;
    public float yCorrection = 1f;
    public float speed = 20f;

    //List<GameObject> spawnedVehicles = new List<GameObject>();
    public GameObject spawnedVehiclePrefab;
    public Dictionary <int, Vehicle> spawnedVehicles = new Dictionary<int, Vehicle>();

    DetectedObject[] detectedObjects = new DetectedObject[0];

    WebSocket ws;

    void Start()
    {
        Debug.Log("Start");
        ws = new WebSocket("ws://localhost:8080");
        ws.Connect();
        ws.OnMessage += (sender, e) => {
            Debug.Log("Message Received from " + ((WebSocket)sender).Url + ", Data : " + e.Data);
            string data = e.Data;
            ParseData(data);
        };

        ws.OnOpen += (sender, e) => {
            Debug.Log("Connected");
        };        
    }

    public void OnDestroy()
    {
        ws.Close();
    }

    void ParseData(string data)
    {
        JSONRoot root = JsonUtility.FromJson<JSONRoot>(data);
        Debug.Log(root.objects.Length);
        detectedObjects = root.objects;
    }

    float ConvertRange(float value, float oldMin, float oldMax, float minValue, float maxValue)
    {
        return ( (value - oldMin) / (oldMax - oldMin) ) * (maxValue - minValue) + minValue;
    }

    void Update()
    {
        RoadScrollTexture.instance.scrollSpeed = speed;
        UIScript.instance.SetSpeed(speed);

        // string test = "{\"objects\":[{\"class_label\":\"car\",\"center_x\":0.6125,\"center_y\":0.8111111111111111}]}";
        // ParseData(test);
        
        List<int> keys = new List<int>(spawnedVehicles.Keys);

        for (int key_idx = 0; key_idx < keys.Count; key_idx++) {
            int key = keys[key_idx];
            bool found = false;
            for (int i = 0; i < detectedObjects.Length; i++) {
                if (detectedObjects[i].id == key)
                {
                    found = true;
                    break;
                }                    
            }

            if (!found)
            {
                spawnedVehicles[key].Destroy();
                spawnedVehicles.Remove(key);
            }                
        }

        for (int i = 0; i < detectedObjects.Length; i++)
        {            
            try
            {
                DetectedObject o = detectedObjects[i];
                //float x = o.center_x * camera.pixelWidth * ((1 - o.center_y) < 0.3f ? 1.25f * (o.center_x > .5f ? 1f : -1f) : 1f);
                //float x = o.center_x * camera.pixelWidth * xCorrection;                
                float x = ConvertRange(ConvertRange(o.center_x, 0, 1, -1, 1) * xCorrection, -1, 1, 0, 1) * camera.pixelWidth;                
                //x += (Mathf.Pow(1 / (1 - Mathf.Abs(.5f - o.center_x)), 2) * xCorrection);
                float y = (1 - o.center_y) * camera.pixelHeight * yCorrection;
                //float y = ConvertRange(ConvertRange(1 - o.center_y, 0, 1, -1, 1) * yCorrection, -1, 1, 0, 1) * camera.pixelHeight;
                
                Vector3 location = Get3DLocation(x, y);

                if (spawnedVehicles.ContainsKey(o.id))
                {
                    Vehicle spawnedVehicle = spawnedVehicles[o.id];                    
                    spawnedVehicle.UpdateParameters(0f, location);
                }
                else
                    SpawnVehicle(o.id, location, o.class_label);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    void DebugSpawn()
    {  
        Vector3 spawnLocation = Get3DLocation(Input.mousePosition.x, Input.mousePosition.y);            
        SpawnVehicle(0, spawnLocation, "car");
        //Ray ray = camera.ScreenPointToRay(new Vector3(.868f * camera.pixelWidth, (1f - .907f) * camera.pixelHeight, camera.nearClipPlane));       
        
    }

    Vector3 Get3DLocation(float x, float y)
    {
        float distance = 100f;
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y, camera.nearClipPlane));        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance)) {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            return new Vector3(hit.point.x, 0.5f, hit.point.z);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.red);
            Vector3 rayEnd = ray.origin + ray.direction * distance;
            //return new Vector3(rayEnd.x, 0.5f, rayEnd.z);            
            return Vector3.zero;
        }            
    }

    void SpawnVehicle(int id, Vector3 spawnLocation, string type = "")
    {
        if (spawnLocation.magnitude > 0f)
        {            
            // // Vehicle newVehicle = new Vehicle();
            Vehicle newVehicle = Vehicle.SpawnVehicle(id, spawnedVehiclePrefab, spawnLocation, type);
            spawnedVehicles.Add(id, newVehicle);
        }                    
    }
}