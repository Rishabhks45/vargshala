using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Common;

public enum UserRole
{
    [Display(Name = "Super Admin")]
    SuperAdmin = 1001,

    [Display(Name = "BackOffice Staff")]
    BackOffice = 1002,

    [Display(Name = "Institute Admin")]
    OrganizationAdmin = 1,

    [Display(Name = "Teacher")]
    Teacher = 2,

    [Display(Name = "Student")]
    Student = 3,

    [Display(Name = "Branch Admin")]
    BranchAdmin = 4
}

public static class RoleNames
{
    public const string SuperAdmin = nameof(UserRole.SuperAdmin);
    public const string BackOffice = nameof(UserRole.BackOffice);
    public const string OrganizationAdmin = nameof(UserRole.OrganizationAdmin);
    public const string Teacher = nameof(UserRole.Teacher);
    public const string Student = nameof(UserRole.Student);
    public const string BranchAdmin = nameof(UserRole.BranchAdmin);
}

public static class UserRoleExtensions
{
    public static string GetDisplayName(this UserRole role) => role switch
    {
        UserRole.SuperAdmin => "Super Admin",
        UserRole.BackOffice => "BackOffice Staff",
        UserRole.OrganizationAdmin => "Institute Admin",
        UserRole.BranchAdmin => "Branch Admin",
        UserRole.Teacher => "Teacher",
        UserRole.Student => "Student",
        _ => role.ToString()
    };
}
