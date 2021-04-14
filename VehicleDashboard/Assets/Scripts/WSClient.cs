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
    public float vehicle_offset;
}

[System.Serializable]
public class VehicleType
{
    public GameObject vehicle;
    public string type;
}


public class WSClient : MonoBehaviour
{    
    public VehicleType[] vehicleTypes;

    public Camera fovCamera;

    public float xCorrection = 1f;
    public float yCorrection = 1f;

    public float xFinalCorrection = 1f;
    public float yFinalCorrection = 1f;


    public float speed = 20f;
    
        
    WebSocket ws;    
    JSONRoot jsonData;
    //List<GameObject> spawnedVehicles = new List<GameObject>();
    public GameObject spawnedVehiclePrefab;
    public Dictionary <int, Vehicle> spawnedVehicles = new Dictionary<int, Vehicle>();    

    public SteeringWheel steeringWheel;


    void Start()
    {        
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
        jsonData = JsonUtility.FromJson<JSONRoot>(data);
        
        steeringWheel.UpdateParameters(jsonData.vehicle_offset);
    }

    float ConvertRange(float value, float oldMin, float oldMax, float minValue, float maxValue)
    {
        return ( (value - oldMin) / (oldMax - oldMin) ) * (maxValue - minValue) + minValue;
    }
        
    GameObject FindVehicleByType(string type)
    {
        foreach (VehicleType vtype in vehicleTypes)
        {
            if (vtype.type.Equals(type))
                return vtype.vehicle;
        }

        return vehicleTypes[0].vehicle;
    }

    void Update()
    {
        RoadScrollTexture.instance.scrollSpeed = speed;
        UIScript.instance.SetSpeed(speed);

        // string test = "{\"objects\":[{\"class_label\":\"car\",\"center_x\":0.6125,\"center_y\":0.8111111111111111}]}";
        // ParseData(test);
        if (jsonData == null)
            return;

        List<int> keys = new List<int>(spawnedVehicles.Keys);

        for (int key_idx = 0; key_idx < keys.Count; key_idx++) {
            int key = keys[key_idx];
            bool found = false;
            for (int i = 0; i < jsonData.objects.Length; i++) {
                if (jsonData.objects[i].id == key)
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

        for (int i = 0; i < jsonData.objects.Length; i++)
        {            
            try
            {
                DetectedObject o = jsonData.objects[i];
                float x = ConvertRange(ConvertRange(o.center_x, 0, 1, -1, 1) * xCorrection, -1, 1, 0, 1) * fovCamera.pixelWidth;                                
                float y = (1 - o.center_y) * fovCamera.pixelHeight * yCorrection;                
                
                Vector3 location = Get3DLocation(x, y);

                if (spawnedVehicles.ContainsKey(o.id))
                {
                    Vehicle spawnedVehicle = spawnedVehicles[o.id];                    
                    spawnedVehicle.UpdateParameters(speed, location);
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
    }

    Vector3 Get3DLocation(float x, float y)
    {
        float distance = 100f;
        Ray ray = fovCamera.ScreenPointToRay(new Vector3(x, y, fovCamera.nearClipPlane));        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance)) {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            return new Vector3(hit.point.x * xFinalCorrection, 0.5f, hit.point.z * yFinalCorrection);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.red);
            Vector3 rayEnd = ray.origin + ray.direction * distance;                       
            return Vector3.zero;
        }            
    }

    void SpawnVehicle(int id, Vector3 spawnLocation, string type = "")
    {
        if (spawnLocation.magnitude > 0f)
        {            
            // // Vehicle newVehicle = new Vehicle();
            //Vehicle newVehicle = Vehicle.SpawnVehicle(id, spawnedVehiclePrefab, spawnLocation, type);
            
            GameObject vehiclePrefab = FindVehicleByType(type);

            GameObject spawnedVehicle = Instantiate(vehiclePrefab, spawnLocation, Quaternion.identity) as GameObject;

            Vehicle vehicleScript = spawnedVehicle.GetComponent<Vehicle>();                                    
            vehicleScript.id = id;            

            spawnedVehicles.Add(id, vehicleScript);
        }
    }
}