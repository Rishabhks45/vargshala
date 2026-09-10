using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Common;

public enum DayOfWeek
{
    [Display(Name = "Monday")]
    Monday = 1,

    [Display(Name = "Tuesday")]
    Tuesday = 2,

    [Display(Name = "Wednesday")]
    Wednesday = 3,

    [Display(Name = "Thursday")]
    Thursday = 4,

    [Display(Name = "Friday")]
    Friday = 5,

    [Display(Name = "Saturday")]
    Saturday = 6,

    [Display(Name = "Sunday")]
    Sunday = 7
}

public static class DayOfWeekExtensions
{
    public static string GetShortName(this DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Mon",
        DayOfWeek.Tuesday => "Tue",
        DayOfWeek.Wednesday => "Wed",
        DayOfWeek.Thursday => "Thu",
        DayOfWeek.Friday => "Fri",
        DayOfWeek.Saturday => "Sat",
        DayOfWeek.Sunday => "Sun",
        _ => day.ToString()
    };
}
