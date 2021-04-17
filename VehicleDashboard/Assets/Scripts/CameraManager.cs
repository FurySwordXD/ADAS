using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{

    public GameObject mainCamera, viewCamera, carCamera, dashCamera;
    public CanvasGroup vehicleUI;

    public static CameraManager instance = null;

    bool exterior = false;
    bool animating = false;
    // Start is called before the first frame update
    void Awake()
    {
        CameraManager.instance = this;        
    }

    void Start()
    {
        StartCoroutine(ShowExterior());
    }

    void OnDestroy()
    {
        CameraManager.instance = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleView()
    {
        if (!animating)
        {
            if (exterior)
                StartCoroutine(ShowInterior());
            else
                StartCoroutine(ShowExterior());                
        }                 
    }

    IEnumerator ShowInterior()
    {
        animating = true;
        exterior = false;

        mainCamera.transform.position = viewCamera.transform.position;
        mainCamera.transform.rotation = viewCamera.transform.rotation;

        ScreenFade.instance.FadeOut();
        LeanTween.alphaCanvas(vehicleUI, 0f, .5f);
        yield return new WaitForSeconds(0.5f);        

        mainCamera.transform.position = dashCamera.transform.position;
        mainCamera.transform.rotation = dashCamera.transform.rotation;

        ScreenFade.instance.FadeIn();
        yield return new WaitForSeconds(0.25f);

        LeanTween.move(mainCamera, carCamera.transform.position, .5f);
        LeanTween.rotate(mainCamera, carCamera.transform.eulerAngles, .5f);

        yield return new WaitForSeconds(0.5f);
        animating = false;
    }

    IEnumerator ShowExterior()
    {
        animating = true;
        exterior = true;

        mainCamera.transform.position = carCamera.transform.position;
        mainCamera.transform.rotation = carCamera.transform.rotation;

        LeanTween.move(mainCamera, dashCamera.transform.position, .5f);        
        LeanTween.rotate(mainCamera, dashCamera.transform.eulerAngles, .5f);
        
        yield return new WaitForSeconds(0.25f);

        ScreenFade.instance.FadeOut(.25f);

        yield return new WaitForSeconds(0.5f);

        mainCamera.transform.position = viewCamera.transform.position;
        mainCamera.transform.rotation = viewCamera.transform.rotation;
        
        LeanTween.alphaCanvas(vehicleUI, 1f, .5f);
        ScreenFade.instance.FadeIn();

        yield return new WaitForSeconds(0.5f);
        animating = false;
    }
}
