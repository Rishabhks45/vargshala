using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Vargshala.Contracts.Messages.Enums;

public enum ConversationType
{
    [Display(Name = "Direct Message")]
    Direct = 1,

    [Display(Name = "Channel / Broadcast")]
    Channel = 2,

    [Display(Name = "Custom Group")]
    Group = 3,

    [Display(Name = "Batch Group")]
    BatchGroup = 4,

    [Display(Name = "Branch Group")]
    BranchGroup = 5,

    [Display(Name = "Organization Announcement")]
    OrganizationGroup = 6
}

public enum ChannelPostingPermission
{
    [Display(Name = "All Members Can Reply")]
    AllMembers = 1,

    [Display(Name = "Teachers & Admins Only")]
    AdminsAndTeachersOnly = 2,

    [Display(Name = "Admins Only")]
    AdminsOnly = 3
}

public enum WhoCanReply
{
    [Display(Name = "Everyone")]
    Everyone = 1,

    [Display(Name = "Admins Only")]
    AdminsOnly = 2,

    [Display(Name = "Teachers & Admins")]
    TeachersAndAdmins = 3
}

public enum MessageType
{
    [Display(Name = "Text Message")]
    Text = 1,

    [Display(Name = "Image / Photo")]
    Image = 2,

    [Display(Name = "Document / File")]
    File = 3,

    [Display(Name = "System Event")]
    System = 4,

    [Display(Name = "Announcement")]
    Announcement = 5
}

public enum SystemEventType
{
    [Display(Name = "Member Added")]
    MemberAdded = 1,

    [Display(Name = "Member Removed")]
    MemberRemoved = 2,

    [Display(Name = "Admin Promoted")]
    AdminPromoted = 3,

    [Display(Name = "Admin Demoted")]
    AdminDemoted = 4,

    [Display(Name = "Member Left")]
    MemberLeft = 5,

    [Display(Name = "Group Created")]
    GroupCreated = 6,

    [Display(Name = "Group Photo Changed")]
    GroupPhotoChanged = 7,

    [Display(Name = "Group Name Changed")]
    GroupNameChanged = 8
}

public enum ConversationParticipantRole
{
    [Display(Name = "Member")]
    Member = 1,

    [Display(Name = "Group Admin")]
    Admin = 2
}

public static class MessageEnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetField(enumValue.ToString())?
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? enumValue.ToString();
    }
}
