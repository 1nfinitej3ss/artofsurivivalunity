using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonRotation : MonoBehaviour
{
    // Public variable to set the speed of rotation    // Public variable to set the speed of rotation
    public float rotationSpeed = 100f;

    void Update()
    {
        // Calculate rotation based on speed and time
        float rotationAmount = rotationSpeed * Time.deltaTime;

        // Apply rotation to the GameObject around the Z-axis for flat rotation
        transform.Rotate(0, 0, rotationAmount);
    }
}
