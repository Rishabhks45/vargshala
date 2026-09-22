using System.ComponentModel.DataAnnotations;

namespace Vargshala.SharedKernel.Enums;

public enum Designation
{
    [Display(Name = "Teacher")]
    Teacher = 1,

    [Display(Name = "Senior Teacher")]
    SeniorTeacher = 2,

    [Display(Name = "HOD")]
    HOD = 3,

    [Display(Name = "Assistant Teacher")]
    AssistantTeacher = 4,

    [Display(Name = "Guest Lecturer")]
    GuestLecturer = 5,

    [Display(Name = "Principal")]
    Principal = 6,

    [Display(Name = "Vice Principal")]
    VicePrincipal = 7,

    [Display(Name = "Lab Assistant")]
    LabAssistant = 8,

    [Display(Name = "Other")]
    Other = 9
}

public static class DesignationExtensions
{
    public static string GetDisplayName(this Designation designation)
        => EnumHelper.GetDisplayName(designation);

    public static Designation? ParseDesignation(string? val)
        => EnumHelper.TryParseFromDisplayName<Designation>(val, out var result) ? result : null;
}

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
    public static string GetDisplayName(this HighestQualification qualification)
        => EnumHelper.GetDisplayName(qualification);

    public static HighestQualification? ParseQualification(string? val)
        => EnumHelper.TryParseFromDisplayName<HighestQualification>(val, out var result) ? result : null;
}

public enum Department
{
    // Science & Technology
    [Display(Name = "Computer Science")]
    ComputerScience = 1,

    [Display(Name = "Information Technology")]
    InformationTechnology = 2,

    [Display(Name = "Mathematics")]
    Mathematics = 3,

    [Display(Name = "Physics")]
    Physics = 4,

    [Display(Name = "Chemistry")]
    Chemistry = 5,

    [Display(Name = "Biology")]
    Biology = 6,

    [Display(Name = "Botany")]
    Botany = 7,

    [Display(Name = "Zoology")]
    Zoology = 8,

    [Display(Name = "General Science")]
    GeneralScience = 9,

    [Display(Name = "Biotechnology")]
    Biotechnology = 10,

    // Commerce & Management
    [Display(Name = "Commerce")]
    Commerce = 11,

    [Display(Name = "Accountancy")]
    Accountancy = 12,

    [Display(Name = "Business Studies")]
    BusinessStudies = 13,

    [Display(Name = "Economics")]
    Economics = 14,

    [Display(Name = "Statistics")]
    Statistics = 15,

    // Languages & Literature
    [Display(Name = "English")]
    English = 16,

    [Display(Name = "Hindi")]
    Hindi = 17,

    [Display(Name = "Sanskrit")]
    Sanskrit = 18,

    [Display(Name = "Foreign Languages")]
    ForeignLanguages = 19,

    [Display(Name = "Regional Languages")]
    RegionalLanguages = 20,

    // Humanities & Social Sciences
    [Display(Name = "Social Studies")]
    SocialStudies = 21,

    [Display(Name = "History")]
    History = 22,

    [Display(Name = "Geography")]
    Geography = 23,

    [Display(Name = "Political Science")]
    PoliticalScience = 24,

    [Display(Name = "Psychology")]
    Psychology = 25,

    [Display(Name = "Sociology")]
    Sociology = 26,

    [Display(Name = "Philosophy")]
    Philosophy = 27,

    // Arts, Sports & Activities
    [Display(Name = "Physical Education")]
    PhysicalEducation = 28,

    [Display(Name = "Fine Arts & Drawing")]
    FineArts = 29,

    [Display(Name = "Music & Performing Arts")]
    MusicAndPerformingArts = 30,

    // Coaching, Competitive & Foundations
    [Display(Name = "Competitive Exams / Foundation")]
    CompetitiveExams = 31,

    [Display(Name = "Reasoning & Aptitude")]
    ReasoningAndAptitude = 32,

    [Display(Name = "General Knowledge")]
    GeneralKnowledge = 33,

    // Administration & General
    [Display(Name = "Academics & Curriculum")]
    Academics = 34,

    [Display(Name = "Administration")]
    Administration = 35,

    [Display(Name = "Other")]
    Other = 36
}

public static class DepartmentExtensions
{
    public static string GetDisplayName(this Department department)
        => EnumHelper.GetDisplayName(department);

    public static Department? ParseDepartment(string? val)
        => EnumHelper.TryParseFromDisplayName<Department>(val, out var result) ? result : null;
}

