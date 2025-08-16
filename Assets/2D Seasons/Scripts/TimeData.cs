using UnityEngine;

[CreateAssetMenu(fileName = "TimeData", menuName = "Game/Time Data")]
public class TimeData : ScriptableObject
{
    [SerializeField] private string m_CountdownText;
    [SerializeField] private int m_Years;
    [SerializeField] private int m_Months;
    [SerializeField] private int m_Days;

    public string CountdownText 
    { 
        get => m_CountdownText;
        set => m_CountdownText = value;
    }

    public int Years
    {
        get => m_Years;
        set => m_Years = value;
    }

    public int Months
    {
        get => m_Months;
        set => m_Months = value;
    }

    public int Days
    {
        get => m_Days;
        set => m_Days = value;
    }
} 