using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroller : MonoBehaviour {
    //Scrolling speed
    public Vector2 scrollSpeed;
    //My Renderer
    Renderer myRenderer;
    // Use this for initialization
	void Start () {
        //Get the renderer
        if (transform.GetComponent<Renderer>())
            myRenderer = transform.GetComponent<Renderer>();

    }
	
	// Update is called once per frame
	void Update () {
        //Lets move , not really , we are just scrolling
        if (!myRenderer)
            return;
        myRenderer.material.mainTextureOffset += new Vector2(scrollSpeed.x * Time.deltaTime, scrollSpeed.y * Time.deltaTime);
	}
}
