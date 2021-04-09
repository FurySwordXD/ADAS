using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{    
    public Text speedText;

    public static UIScript instance;

    void Awake()
    {
        UIScript.instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpeed(float speed)
    {
        speedText.text = speed.ToString();
    }
}
