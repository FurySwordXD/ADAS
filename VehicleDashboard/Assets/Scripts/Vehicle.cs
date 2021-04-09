using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VehicleType
{
    public GameObject vehicle;
    public string type;
}

public class Vehicle : MonoBehaviour
{
    public VehicleType[] vehicleTypes;

    public int id;    
    public string type;
    public float speed = 0f;
    
    public Text speedText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {        
    }    

    
    private GameObject FindVehicleByType(string type)
    {
        foreach (VehicleType vtype in vehicleTypes)
        {
            if (vtype.type.Equals(type))
                return vtype.vehicle;
        }

        return vehicleTypes[0].vehicle;
    }

    public static Vehicle SpawnVehicle(int id, GameObject spawnedVehiclePrefab, Vector3 spawnLocation, string type = "")
    {
        GameObject spawnedVehicle = Instantiate(spawnedVehiclePrefab, spawnLocation, Quaternion.identity) as GameObject;

        Vehicle vehicleScript = spawnedVehicle.GetComponent<Vehicle>();

        GameObject vehiclePrefab = vehicleScript.FindVehicleByType(type);        
        GameObject newVehicle = Instantiate(vehiclePrefab, Vector3.zero, Quaternion.identity) as GameObject;        
        newVehicle.transform.SetParent(vehicleScript.transform);
        newVehicle.transform.localPosition = Vector3.zero;

        vehicleScript.id = id;
        vehicleScript.type = type;
        return vehicleScript;
    }

    public void UpdateParameters(float baseSpeed, Vector3 location)
    {
        speed = baseSpeed + ( (location.z - transform.position.z)  / Time.deltaTime / 10f );
        speedText.text = (Mathf.Round(speed).ToString() + " km/h");
        transform.position = Vector3.Lerp(transform.position, location, 1f * Time.deltaTime);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
