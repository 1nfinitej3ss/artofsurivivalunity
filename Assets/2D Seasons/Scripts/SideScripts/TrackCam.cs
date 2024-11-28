using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCam : MonoBehaviour {
    //Camera to follow
    public Transform camToFollow;

	void LateUpdate () {
        //Move it based on camera's position
        if (camToFollow)
            transform.position = new Vector3(camToFollow.position.x, camToFollow.position.y, transform.position.z);
	}
}
