using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SetTimeSliderControl : MonoBehaviour {
    //Get values and set time
    public Slider sliderTimeSetter;
    public DayNightCycle2D dayNightCycle;

    public void SendTimeUpdate() {
        if (!sliderTimeSetter || !dayNightCycle) {
            Debug.Log("Variables missing");
            return;
        }
        dayNightCycle.SetNewTime(sliderTimeSetter.value);
    } 
}
