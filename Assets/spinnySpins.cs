using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlanetRunner;  // Simplified namespace

public class spinnySpins : MonoBehaviour
{
    private PlanetController planetController;

    // Start is called before the first frame update
    void Start()
    {
        // Find the PlanetController in the scene
        planetController = FindObjectOfType<PlanetController>();
        
        if (planetController == null)
        {
            Debug.LogWarning("No PlanetController found in scene! The spinnySpins script needs it to sync rotation.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (planetController != null)
        {
            // Instead of applying rotation speed, match the planet's exact rotation
            float planetRotation = planetController.transform.rotation.eulerAngles.z;
            transform.rotation = Quaternion.Euler(0, 0, planetRotation);
        }
    }
}
