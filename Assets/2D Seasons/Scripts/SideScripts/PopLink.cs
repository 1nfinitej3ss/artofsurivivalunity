using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public class PopLink : MonoBehaviour
{
    //Opens links in new tab for WebGL --- You prolly don't need this
    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

	public void Website() {
		#if !UNITY_EDITOR
		openWindow("http://negleft.com");
		#endif
	}

    public void Assetstore() {
		#if !UNITY_EDITOR
		openWindow("http://u3d.as/Uds");
		#endif
	}
    

	[DllImport("__Internal")]
	private static extern void openWindow(string url);
}
