using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSetup : MonoBehaviour
{
    public float worldRadius = 10f; // adjust this to match your world's size
    public GameObject[] prefabs; // assign the prefabs you want to instantiate

    void Start()
    {
        // Make sure we have at least one prefab
        if (prefabs.Length == 0) {
            Debug.LogError("No prefabs assigned!");
            return;
        }

        for (int i = 0; i < prefabs.Length; i++)
        {
            // calculate the angle in radians
            float angle = 2 * Mathf.PI / prefabs.Length * i; 

            // calculate the x and y position
            float x = worldRadius * Mathf.Cos(angle);
            float y = worldRadius * Mathf.Sin(angle);

            // Instantiate the object at this position
            // Instantiate the object at this position and rotate it to face away from the center
            GameObject newObj = Instantiate(prefabs[i], new Vector3(x, 0, y), Quaternion.identity);
            newObj.transform.up = newObj.transform.position;
        }
    }
}
