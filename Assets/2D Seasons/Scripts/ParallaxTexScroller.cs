using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxTexScroller : MonoBehaviour {
    //Renfrence to renderer
    Renderer myRenderer;
    //Scrolling speed , set this to a very small value or high if that's what you want
    public float scrollSpeed = 0.0015f;
    //Camera to follow
    public Transform camToFollow;
    void Start()
    {
        //Get the renderer
        if (transform.GetComponent<Renderer>())
            myRenderer = transform.GetComponent<Renderer>();

        if (!camToFollow)
            camToFollow = Camera.main.transform;

    }

    // Update is called once per frame
    void Update()
    {
        //Lets move
        if (!myRenderer || !camToFollow)
            return;
        myRenderer.material.mainTextureOffset = new Vector2(camToFollow.position.x * scrollSpeed , 0.0f);
    }
}
