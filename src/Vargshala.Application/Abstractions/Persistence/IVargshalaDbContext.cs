using Microsoft.EntityFrameworkCore;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Abstractions.Persistence;

public interface IVargshalaDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<EmailTemplate> EmailTemplates { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Student> Students { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Branch> Branches { get; }
    DbSet<UserBranchAccess> UserBranchAccesses { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<Class> Classes { get; }
    DbSet<Batch> Batches { get; }
    DbSet<BatchTeacher> BatchTeachers { get; }
    DbSet<BatchStudent> BatchStudents { get; }
    DbSet<BatchSchedule> BatchSchedules { get; }
    DbSet<ClassSession> ClassSessions { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<FeeStructure> FeeStructures { get; }
    DbSet<StudentFee> StudentFees { get; }
    DbSet<FeeDiscount> FeeDiscounts { get; }
    DbSet<FeeInstallment> FeeInstallments { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentAllocation> PaymentAllocations { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationParticipant> ConversationParticipants { get; }
    DbSet<ConversationAdmin> ConversationAdmins { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageAttachment> MessageAttachments { get; }
    DbSet<MessageRead> MessageReads { get; }
    DbSet<AnnouncementReplyPermission> AnnouncementReplyPermissions { get; }
    DbSet<MessageReaction> MessageReactions { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<OrganizationSubscription> OrganizationSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

