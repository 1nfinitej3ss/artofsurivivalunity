using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StartupStateSetter : MonoBehaviour {

    public enum AllStates
    {
        ClearSky , KeepRaining , KeepStorming
    };
    public bool pauseTime;
    public AllStates state;
    RainController myRainController;
    DayNightCycle2D dayNightCycle;
	// Use this for initialization
	void Start () {
	if (transform.GetComponent<RainController>())
        {
            myRainController = transform.GetComponent<RainController>();

            switch (state) {
                case AllStates.ClearSky:myRainController.NoRainNoStorm(); break;
                case AllStates.KeepRaining : myRainController.KeepOnRaining(); break;
                case AllStates.KeepStorming: myRainController.KeepOnStorming(); break;
            }

        }

        if (transform.GetComponent<DayNightCycle2D>()) {
            dayNightCycle = transform.GetComponent<DayNightCycle2D>();
            if (pauseTime)
                dayNightCycle.timePaused = true;
        }
    
	}
	
}
