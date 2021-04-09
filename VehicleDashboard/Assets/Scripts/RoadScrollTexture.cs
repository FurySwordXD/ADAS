using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScrollDirection {
    X, Y
}

public class RoadScrollTexture : MonoBehaviour
{   
    public Material roadMaterial; 
    public Material laneMaterial;

    public ScrollDirection scrollDirection;

    public float scrollSpeed = 0.5f;

    public static RoadScrollTexture instance;
    // Update is called once per frame
    void Awake() {
        if (RoadScrollTexture.instance == null)
            RoadScrollTexture.instance = this;
        else
            Destroy(this);
    }

    void Update()
    {
        float speed = scrollSpeed * Time.time * -0.278f;
        Vector2 texureOffset = new Vector2(speed * (scrollDirection == ScrollDirection.X ? 1 : 0), speed * (scrollDirection == ScrollDirection.Y ? 1 : 0));
        roadMaterial.mainTextureOffset = texureOffset;
        laneMaterial.mainTextureOffset = texureOffset / 20f;        
    }
}
