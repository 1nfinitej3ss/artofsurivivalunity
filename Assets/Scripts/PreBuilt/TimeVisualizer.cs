using UnityEngine;
using UnityEngine.UI;

public class TimeVisualizer : MonoBehaviour {
    public RectTransform[] yearBarsRect = new RectTransform[5];
    public RectTransform[][] monthBarsRect = new RectTransform[5][];
    private int totalMonthsPassed = 0;  // Counter for the total number of months passed.

    private void Awake() {
        for (int i = 0; i < 5; i++) {
            monthBarsRect[i] = new RectTransform[12];
            RectTransform[] children = yearBarsRect[i].GetComponentsInChildren<RectTransform>();
            for (int j = 1; j < children.Length; j++) {
                monthBarsRect[i][j-1] = children[j];
            }
        }
    }

    private void OnEnable() {
        TimeManager.OnMonthPassed += HandleMonthPassed;
    }

    private void OnDisable() {
        TimeManager.OnMonthPassed -= HandleMonthPassed;
    }

    // This method will be called every time a month passes.
    private void HandleMonthPassed() {
        int yearIndex = totalMonthsPassed / 12;
        int monthIndex = totalMonthsPassed % 12;

        if (yearIndex < 5) {
            //Debug.Log($"Setting Month {monthIndex + 1} of Year {yearIndex + 1} to zero height.");
            monthBarsRect[yearIndex][monthIndex].sizeDelta = new Vector2(monthBarsRect[yearIndex][monthIndex].sizeDelta.x, 0);
        }

        totalMonthsPassed++;  // Increase the count of total months passed.
    }
}
