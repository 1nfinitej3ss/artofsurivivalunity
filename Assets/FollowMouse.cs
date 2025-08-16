using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Make sure we have a SpriteRenderer
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on this object!");
        }
        else
        {
            Debug.Log("SpriteRenderer found successfully!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // If mouse is to the left, scale is negative (flipped)
        // If mouse is to the right, scale is positive (normal)
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * (mousePosition.x < transform.position.x ? -1 : 1);
        transform.localScale = newScale;
    }
}
