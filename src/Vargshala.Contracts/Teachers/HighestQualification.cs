using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Teachers;

public enum HighestQualification
{
    [Display(Name = "B.Ed.")]
    BEd = 1,

    [Display(Name = "M.Ed.")]
    MEd = 2,

    [Display(Name = "B.Sc.")]
    BSc = 3,

    [Display(Name = "M.Sc.")]
    MSc = 4,

    [Display(Name = "B.A.")]
    BA = 5,

    [Display(Name = "M.A.")]
    MA = 6,

    [Display(Name = "B.Tech.")]
    BTech = 7,

    [Display(Name = "M.Tech.")]
    MTech = 8,

    [Display(Name = "B.Com.")]
    BCom = 9,

    [Display(Name = "M.Com.")]
    MCom = 10,

    [Display(Name = "BCA")]
    BCA = 11,

    [Display(Name = "MCA")]
    MCA = 12,

    [Display(Name = "MBA")]
    MBA = 13,

    [Display(Name = "Ph.D.")]
    PhD = 14,

    [Display(Name = "Other")]
    Other = 15
}

public static class HighestQualificationExtensions
{
    public static string GetDisplayName(this HighestQualification qualification) => qualification switch
    {
        HighestQualification.BEd => "B.Ed.",
        HighestQualification.MEd => "M.Ed.",
        HighestQualification.BSc => "B.Sc.",
        HighestQualification.MSc => "M.Sc.",
        HighestQualification.BA => "B.A.",
        HighestQualification.MA => "M.A.",
        HighestQualification.BTech => "B.Tech.",
        HighestQualification.MTech => "M.Tech.",
        HighestQualification.BCom => "B.Com.",
        HighestQualification.MCom => "M.Com.",
        HighestQualification.BCA => "BCA",
        HighestQualification.MCA => "MCA",
        HighestQualification.MBA => "MBA",
        HighestQualification.PhD => "Ph.D.",
        HighestQualification.Other => "Other",
        _ => qualification.ToString()
    };

    public static HighestQualification? ParseQualification(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;

        if (int.TryParse(val, out var intVal) && Enum.IsDefined(typeof(HighestQualification), intVal))
            return (HighestQualification)intVal;

        var clean = val.Trim().Replace(".", "").Replace(" ", "");
        foreach (HighestQualification q in Enum.GetValues(typeof(HighestQualification)))
        {
            if (string.Equals(q.ToString(), clean, StringComparison.OrdinalIgnoreCase))
                return q;
            if (string.Equals(q.GetDisplayName().Replace(".", "").Replace(" ", ""), clean, StringComparison.OrdinalIgnoreCase))
                return q;
        }

        return null;
    }
}
