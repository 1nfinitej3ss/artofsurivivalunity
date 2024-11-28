using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {
    //moves camera on x axis at this speed
    public float speed = 5.0f;

	// Update is called once per frame
	void LateUpdate () {
        transform.position += Vector3.right * (speed * Time.deltaTime);
	}
}
