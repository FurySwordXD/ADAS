using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public CanvasGroup canvas;

    public static ScreenFade instance;
    // Start is called before the first frame update
    void Awake()
    {
        ScreenFade.instance = this;
    }

    void OnDestroy()
    {
        ScreenFade.instance = null;
    }

    public void FadeIn(float time = 0.5f)
    {
        LeanTween.alphaCanvas(canvas, 0f, time);
    }

    public void FadeOut(float time = 0.5f)
    {
        LeanTween.alphaCanvas(canvas, 1f, time);
    }
}
