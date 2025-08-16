public static class GameCalendar
{
    #region Constants
    private const int c_DaysPerYear = 365;
    private const bool c_IncludeLeapYears = true; // Set this based on your game's needs
    #endregion

    #region Public Methods
    public static int GetDaysInYears(int _years)
    {
        if (!c_IncludeLeapYears)
        {
            return _years * c_DaysPerYear;
        }

        // Count leap years (every 4 years, except century years not divisible by 400)
        int leapYears = _years / 4;
        leapYears -= _years / 100;
        leapYears += _years / 400;

        return (_years * c_DaysPerYear) + leapYears;
    }
    #endregion
} 