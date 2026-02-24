namespace frontend.Helpers;

public static class WeekHelper
{
    /// <summary>
    /// Returns the start of the current payroll week (Sunday 00:00).
    /// The week resets every Saturday at 3:00 PM, meaning:
    /// - Before Saturday 3:00 PM → current week started last Sunday
    /// - After Saturday 3:00 PM → new week starts (next Sunday), so we advance to the next Sunday
    /// </summary>
    public static DateTime GetCurrentWeekStart()
    {
        var now = DateTime.Now;
        return GetWeekStart(now);
    }

    /// <summary>
    /// Returns the payroll week start (Sunday) for a given date/time.
    /// If it's Saturday at or after 3:00 PM, the week advances to next Sunday.
    /// </summary>
    public static DateTime GetWeekStart(DateTime dateTime)
    {
        var date = dateTime.Date;
        var dayOfWeek = (int)date.DayOfWeek;

        // Sunday = 0, Saturday = 6
        var weekStart = date.AddDays(-dayOfWeek);

        // If it's Saturday at 3PM or later, advance to next week (next Sunday)
        if (date.DayOfWeek == DayOfWeek.Saturday && dateTime.Hour >= 15)
        {
            weekStart = weekStart.AddDays(7);
        }

        return weekStart;
    }

    /// <summary>
    /// Returns the end of the payroll week (Saturday at 3:00 PM).
    /// </summary>
    public static DateTime GetWeekEnd(DateTime weekStart)
    {
        return weekStart.AddDays(6).AddHours(15); // Saturday 3:00 PM
    }

    /// <summary>
    /// Returns the week date cutoff for filtering records.
    /// Records from weekStart (Sunday 00:00) to before next weekStart (next Sunday 00:00).
    /// </summary>
    public static (DateTime Start, DateTime End) GetWeekRange(DateTime dateTime)
    {
        var start = GetWeekStart(dateTime);
        var end = start.AddDays(7);
        return (start, end);
    }
}
