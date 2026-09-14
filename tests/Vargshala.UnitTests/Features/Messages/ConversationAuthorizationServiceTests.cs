using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages.Enums;
using Vargshala.Domain.Entities;
using Vargshala.Infrastructure.Persistence;
using Vargshala.Infrastructure.Services.Messages;

namespace Vargshala.UnitTests.Features.Messages;

public class ConversationAuthorizationServiceTests : IDisposable
{
    private readonly VargshalaDbContext _db;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly ConversationAuthorizationService _authService;

    private readonly Guid _org1Id = Guid.NewGuid();
    private readonly Guid _org2Id = Guid.NewGuid();

    private readonly Guid _branch1Id = Guid.NewGuid();
    private readonly Guid _branch2Id = Guid.NewGuid();

    public ConversationAuthorizationServiceTests()
    {
        var options = new DbContextOptionsBuilder<VargshalaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new VargshalaDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();

        _authService = new ConversationAuthorizationService(_db, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task Student_To_SameBatchStudent_IsAllowed()
    {
        // Arrange
        var studentAUser = CreateUser("Student", "A", UserRole.Student, _org1Id);
        var studentBUser = CreateUser("Student", "B", UserRole.Student, _org1Id);
        await _db.Users.AddRangeAsync(studentAUser, studentBUser);

        var studentA = new Student { Id = Guid.NewGuid(), UserId = studentAUser.Id, IsActive = true };
        var studentB = new Student { Id = Guid.NewGuid(), UserId = studentBUser.Id, IsActive = true };
        await _db.Students.AddRangeAsync(studentA, studentB);

        var batch1 = new Batch { Id = Guid.NewGuid(), Name = "Batch 10A", IsActive = true };
        await _db.Batches.AddAsync(batch1);

        await _db.BatchStudents.AddRangeAsync(
            new BatchStudent { Id = Guid.NewGuid(), BatchId = batch1.Id, StudentId = studentA.Id, Student = studentA, Batch = batch1, IsActive = true },
            new BatchStudent { Id = Guid.NewGuid(), BatchId = batch1.Id, StudentId = studentB.Id, Student = studentB, Batch = batch1, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(studentAUser.Id, studentBUser.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Student_To_OtherBatchStudent_IsDenied()
    {
        // Arrange
        var studentAUser = CreateUser("Student", "A", UserRole.Student, _org1Id);
        var studentBUser = CreateUser("Student", "B", UserRole.Student, _org1Id);
        await _db.Users.AddRangeAsync(studentAUser, studentBUser);

        var studentA = new Student { Id = Guid.NewGuid(), UserId = studentAUser.Id, IsActive = true };
        var studentB = new Student { Id = Guid.NewGuid(), UserId = studentBUser.Id, IsActive = true };
        await _db.Students.AddRangeAsync(studentA, studentB);

        var batch1 = new Batch { Id = Guid.NewGuid(), Name = "Batch 10A", IsActive = true };
        var batch2 = new Batch { Id = Guid.NewGuid(), Name = "Batch 10B", IsActive = true };
        await _db.Batches.AddRangeAsync(batch1, batch2);

        await _db.BatchStudents.AddRangeAsync(
            new BatchStudent { Id = Guid.NewGuid(), BatchId = batch1.Id, StudentId = studentA.Id, Student = studentA, Batch = batch1, IsActive = true },
            new BatchStudent { Id = Guid.NewGuid(), BatchId = batch2.Id, StudentId = studentB.Id, Student = studentB, Batch = batch2, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(studentAUser.Id, studentBUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Student_To_AssignedTeacher_IsAllowed()
    {
        // Arrange
        var studentUser = CreateUser("Student", "1", UserRole.Student, _org1Id);
        var teacherUser = CreateUser("Teacher", "1", UserRole.Teacher, _org1Id);
        await _db.Users.AddRangeAsync(studentUser, teacherUser);

        var student = new Student { Id = Guid.NewGuid(), UserId = studentUser.Id, IsActive = true };
        var teacher = new Teacher { Id = Guid.NewGuid(), UserId = teacherUser.Id, IsActive = true };
        await _db.Students.AddAsync(student);
        await _db.Teachers.AddAsync(teacher);

        var batch = new Batch { Id = Guid.NewGuid(), Name = "Physics Batch", IsActive = true };
        await _db.Batches.AddAsync(batch);

        await _db.BatchStudents.AddAsync(new BatchStudent { Id = Guid.NewGuid(), BatchId = batch.Id, StudentId = student.Id, Student = student, Batch = batch, IsActive = true });
        await _db.BatchTeachers.AddAsync(new BatchTeacher { Id = Guid.NewGuid(), BatchId = batch.Id, TeacherId = teacher.Id, Teacher = teacher, Batch = batch, IsActive = true });
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(studentUser.Id, teacherUser.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Student_To_UnassignedTeacher_IsDenied()
    {
        // Arrange
        var studentUser = CreateUser("Student", "1", UserRole.Student, _org1Id);
        var teacherUser = CreateUser("Teacher", "Unassigned", UserRole.Teacher, _org1Id);
        await _db.Users.AddRangeAsync(studentUser, teacherUser);

        var student = new Student { Id = Guid.NewGuid(), UserId = studentUser.Id, IsActive = true };
        var teacher = new Teacher { Id = Guid.NewGuid(), UserId = teacherUser.Id, IsActive = true };
        await _db.Students.AddAsync(student);
        await _db.Teachers.AddAsync(teacher);

        var batch1 = new Batch { Id = Guid.NewGuid(), Name = "Batch 1", IsActive = true };
        var batch2 = new Batch { Id = Guid.NewGuid(), Name = "Batch 2", IsActive = true };
        await _db.Batches.AddRangeAsync(batch1, batch2);

        await _db.BatchStudents.AddAsync(new BatchStudent { Id = Guid.NewGuid(), BatchId = batch1.Id, StudentId = student.Id, Student = student, Batch = batch1, IsActive = true });
        await _db.BatchTeachers.AddAsync(new BatchTeacher { Id = Guid.NewGuid(), BatchId = batch2.Id, TeacherId = teacher.Id, Teacher = teacher, Batch = batch2, IsActive = true });
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(studentUser.Id, teacherUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Student_To_OtherOrg_IsDenied()
    {
        // Arrange
        var studentAUser = CreateUser("Student", "A", UserRole.Student, _org1Id);
        var studentBUser = CreateUser("Student", "B", UserRole.Student, _org2Id);
        await _db.Users.AddRangeAsync(studentAUser, studentBUser);
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(studentAUser.Id, studentBUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Teacher_To_SameBranchTeacher_IsAllowed()
    {
        // Arrange
        var teacher1User = CreateUser("Teacher", "One", UserRole.Teacher, _org1Id);
        var teacher2User = CreateUser("Teacher", "Two", UserRole.Teacher, _org1Id);
        await _db.Users.AddRangeAsync(teacher1User, teacher2User);

        await _db.UserBranchAccesses.AddRangeAsync(
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = teacher1User.Id, BranchId = _branch1Id, IsActive = true },
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = teacher2User.Id, BranchId = _branch1Id, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(teacher1User.Id, teacher2User.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Teacher_To_OtherBranchTeacher_IsDenied()
    {
        // Arrange
        var teacher1User = CreateUser("Teacher", "Branch1", UserRole.Teacher, _org1Id);
        var teacher2User = CreateUser("Teacher", "Branch2", UserRole.Teacher, _org1Id);
        await _db.Users.AddRangeAsync(teacher1User, teacher2User);

        await _db.UserBranchAccesses.AddRangeAsync(
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = teacher1User.Id, BranchId = _branch1Id, IsActive = true },
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = teacher2User.Id, BranchId = _branch2Id, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(teacher1User.Id, teacher2User.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Teacher_To_AssignedStudent_IsAllowed()
    {
        // Arrange
        var teacherUser = CreateUser("Teacher", "Math", UserRole.Teacher, _org1Id);
        var studentUser = CreateUser("Student", "Math", UserRole.Student, _org1Id);
        await _db.Users.AddRangeAsync(teacherUser, studentUser);

        var teacher = new Teacher { Id = Guid.NewGuid(), UserId = teacherUser.Id, IsActive = true };
        var student = new Student { Id = Guid.NewGuid(), UserId = studentUser.Id, IsActive = true };
        await _db.Teachers.AddAsync(teacher);
        await _db.Students.AddAsync(student);

        var batch = new Batch { Id = Guid.NewGuid(), Name = "Maths Batch", IsActive = true };
        await _db.Batches.AddAsync(batch);

        await _db.BatchTeachers.AddAsync(new BatchTeacher { Id = Guid.NewGuid(), BatchId = batch.Id, TeacherId = teacher.Id, Teacher = teacher, Batch = batch, IsActive = true });
        await _db.BatchStudents.AddAsync(new BatchStudent { Id = Guid.NewGuid(), BatchId = batch.Id, StudentId = student.Id, Student = student, Batch = batch, IsActive = true });
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(teacherUser.Id, studentUser.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Teacher_To_OtherBatchStudent_IsDenied()
    {
        // Arrange
        var teacherUser = CreateUser("Teacher", "Math", UserRole.Teacher, _org1Id);
        var studentUser = CreateUser("Student", "History", UserRole.Student, _org1Id);
        await _db.Users.AddRangeAsync(teacherUser, studentUser);

        var teacher = new Teacher { Id = Guid.NewGuid(), UserId = teacherUser.Id, IsActive = true };
        var student = new Student { Id = Guid.NewGuid(), UserId = studentUser.Id, IsActive = true };
        await _db.Teachers.AddAsync(teacher);
        await _db.Students.AddAsync(student);

        var batch1 = new Batch { Id = Guid.NewGuid(), Name = "Maths", IsActive = true };
        var batch2 = new Batch { Id = Guid.NewGuid(), Name = "History", IsActive = true };
        await _db.Batches.AddRangeAsync(batch1, batch2);

        await _db.BatchTeachers.AddAsync(new BatchTeacher { Id = Guid.NewGuid(), BatchId = batch1.Id, TeacherId = teacher.Id, Teacher = teacher, Batch = batch1, IsActive = true });
        await _db.BatchStudents.AddAsync(new BatchStudent { Id = Guid.NewGuid(), BatchId = batch2.Id, StudentId = student.Id, Student = student, Batch = batch2, IsActive = true });
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(teacherUser.Id, studentUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BranchAdmin_To_SameBranchTeacher_IsAllowed()
    {
        // Arrange
        var branchAdmin = CreateUser("Admin", "Branch1", UserRole.BranchAdmin, _org1Id);
        var teacher = CreateUser("Teacher", "Branch1", UserRole.Teacher, _org1Id);
        await _db.Users.AddRangeAsync(branchAdmin, teacher);

        await _db.UserBranchAccesses.AddRangeAsync(
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = branchAdmin.Id, BranchId = _branch1Id, IsActive = true },
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = teacher.Id, BranchId = _branch1Id, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(branchAdmin.Id, teacher.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task BranchAdmin_To_OtherBranchTeacher_IsDenied()
    {
        // Arrange
        var branchAdmin = CreateUser("Admin", "Branch1", UserRole.BranchAdmin, _org1Id);
        var teacher = CreateUser("Teacher", "Branch2", UserRole.Teacher, _org1Id);
        await _db.Users.AddRangeAsync(branchAdmin, teacher);

        await _db.UserBranchAccesses.AddRangeAsync(
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = branchAdmin.Id, BranchId = _branch1Id, IsActive = true },
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = teacher.Id, BranchId = _branch2Id, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(branchAdmin.Id, teacher.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BranchAdmin_To_OrgAdmin_IsAllowed()
    {
        // Arrange
        var branchAdmin = CreateUser("Admin", "Branch1", UserRole.BranchAdmin, _org1Id);
        var orgAdmin = CreateUser("Org", "Admin", UserRole.OrganizationAdmin, _org1Id);
        await _db.Users.AddRangeAsync(branchAdmin, orgAdmin);

        await _db.UserBranchAccesses.AddAsync(
            new UserBranchAccess { Id = Guid.NewGuid(), UserId = branchAdmin.Id, BranchId = _branch1Id, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(branchAdmin.Id, orgAdmin.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OrgAdmin_To_AnyOwnOrgUser_IsAllowed()
    {
        // Arrange
        var orgAdmin = CreateUser("Org", "Admin", UserRole.OrganizationAdmin, _org1Id);
        var student = CreateUser("Student", "Any", UserRole.Student, _org1Id);
        await _db.Users.AddRangeAsync(orgAdmin, student);
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(orgAdmin.Id, student.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OrgAdmin_To_OtherOrgUser_IsDenied()
    {
        // Arrange
        var orgAdmin = CreateUser("Org", "Admin", UserRole.OrganizationAdmin, _org1Id);
        var studentOtherOrg = CreateUser("Student", "OtherOrg", UserRole.Student, _org2Id);
        await _db.Users.AddRangeAsync(orgAdmin, studentOtherOrg);
        await _db.SaveChangesAsync();

        // Act
        var result = await _authService.CanMessageUserAsync(orgAdmin.Id, studentOtherOrg.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GroupAdmin_CanPromoteAdmin_And_ChangePhoto()
    {
        // Arrange
        var adminUser = CreateUser("Admin", "User", UserRole.Teacher, _org1Id);
        var memberUser = CreateUser("Member", "User", UserRole.Teacher, _org1Id);
        await _db.Users.AddRangeAsync(adminUser, memberUser);

        var conv = new Conversation
        {
            Id = Guid.NewGuid(),
            OrganizationId = _org1Id,
            CreatedBy = adminUser.Id,
            Type = ConversationType.Group,
            Name = "Study Group",
            IsActive = true
        };
        await _db.Conversations.AddAsync(conv);

        await _db.ConversationParticipants.AddRangeAsync(
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = adminUser.Id, OrganizationId = _org1Id, IsAdmin = true, Role = ConversationParticipantRole.Admin, IsActive = true },
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = memberUser.Id, OrganizationId = _org1Id, IsAdmin = false, Role = ConversationParticipantRole.Member, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var canPromote = await _authService.CanPromoteAdminAsync(adminUser.Id, conv.Id, memberUser.Id);
        var canChangePhoto = await _authService.CanChangeGroupPhotoAsync(adminUser.Id, conv.Id);

        // Assert
        canPromote.Should().BeTrue();
        canChangePhoto.Should().BeTrue();
    }

    [Fact]
    public async Task NormalMember_CannotPromoteAdmin_Or_ChangePhoto()
    {
        // Arrange
        var normalMember = CreateUser("Normal", "Member", UserRole.Student, _org1Id);
        var anotherMember = CreateUser("Another", "Member", UserRole.Student, _org1Id);
        await _db.Users.AddRangeAsync(normalMember, anotherMember);

        var conv = new Conversation
        {
            Id = Guid.NewGuid(),
            OrganizationId = _org1Id,
            CreatedBy = normalMember.Id,
            Type = ConversationType.Group,
            Name = "Student Group",
            IsActive = true
        };

        await _db.Conversations.AddAsync(conv);

        await _db.ConversationParticipants.AddRangeAsync(
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = normalMember.Id, OrganizationId = _org1Id, IsAdmin = false, Role = ConversationParticipantRole.Member, IsActive = true },
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = anotherMember.Id, OrganizationId = _org1Id, IsAdmin = false, Role = ConversationParticipantRole.Member, IsActive = true }
        );
        await _db.SaveChangesAsync();

        // Act
        var canPromote = await _authService.CanPromoteAdminAsync(normalMember.Id, conv.Id, anotherMember.Id);
        var canChangePhoto = await _authService.CanChangeGroupPhotoAsync(normalMember.Id, conv.Id);

        // Assert
        canPromote.Should().BeFalse();
        canChangePhoto.Should().BeFalse();
    }

    [Fact]
    public async Task Student_EligibleRecipients_OnlyIncludes_SameBatchStudents_And_AssignedTeachers()
    {
        // Arrange
        var studentAUser = CreateUser("Student", "A", UserRole.Student, _org1Id);
        var sameBatchStudentUser = CreateUser("Student", "SameBatch", UserRole.Student, _org1Id);
        var otherBatchStudentUser = CreateUser("Student", "OtherBatch", UserRole.Student, _org1Id);
        var assignedTeacherUser = CreateUser("Teacher", "Assigned", UserRole.Teacher, _org1Id);
        var unassignedTeacherUser = CreateUser("Teacher", "Unassigned", UserRole.Teacher, _org1Id);

        await _db.Users.AddRangeAsync(studentAUser, sameBatchStudentUser, otherBatchStudentUser, assignedTeacherUser, unassignedTeacherUser);

        var studentA = new Student { Id = Guid.NewGuid(), UserId = studentAUser.Id, IsActive = true };
        var studentSame = new Student { Id = Guid.NewGuid(), UserId = sameBatchStudentUser.Id, IsActive = true };
        var studentOther = new Student { Id = Guid.NewGuid(), UserId = otherBatchStudentUser.Id, IsActive = true };
        var teacherAssigned = new Teacher { Id = Guid.NewGuid(), UserId = assignedTeacherUser.Id, IsActive = true };
        var teacherUnassigned = new Teacher { Id = Guid.NewGuid(), UserId = unassignedTeacherUser.Id, IsActive = true };

        await _db.Students.AddRangeAsync(studentA, studentSame, studentOther);
        await _db.Teachers.AddRangeAsync(teacherAssigned, teacherUnassigned);

        var batch1 = new Batch { Id = Guid.NewGuid(), Name = "Batch 1", IsActive = true };
        var batch2 = new Batch { Id = Guid.NewGuid(), Name = "Batch 2", IsActive = true };
        await _db.Batches.AddRangeAsync(batch1, batch2);

        // Student A and studentSame in Batch 1
        await _db.BatchStudents.AddRangeAsync(
            new BatchStudent { Id = Guid.NewGuid(), BatchId = batch1.Id, StudentId = studentA.Id, Student = studentA, Batch = batch1, IsActive = true },
            new BatchStudent { Id = Guid.NewGuid(), BatchId = batch1.Id, StudentId = studentSame.Id, Student = studentSame, Batch = batch1, IsActive = true },
            // studentOther in Batch 2
            new BatchStudent { Id = Guid.NewGuid(), BatchId = batch2.Id, StudentId = studentOther.Id, Student = studentOther, Batch = batch2, IsActive = true }
        );

        // teacherAssigned in Batch 1, teacherUnassigned in Batch 2
        await _db.BatchTeachers.AddRangeAsync(
            new BatchTeacher { Id = Guid.NewGuid(), BatchId = batch1.Id, TeacherId = teacherAssigned.Id, Teacher = teacherAssigned, Batch = batch1, IsActive = true },
            new BatchTeacher { Id = Guid.NewGuid(), BatchId = batch2.Id, TeacherId = teacherUnassigned.Id, Teacher = teacherUnassigned, Batch = batch2, IsActive = true }
        );

        await _db.SaveChangesAsync();

        // Act
        var eligibleIds = await _authService.GetEligibleDirectMessageRecipientUserIdsAsync(studentAUser.Id);

        // Assert: MUST contain sameBatchStudent and assignedTeacher ONLY. MUST NOT contain otherBatchStudent or unassignedTeacher.
        eligibleIds.Should().Contain(sameBatchStudentUser.Id);
        eligibleIds.Should().Contain(assignedTeacherUser.Id);
        eligibleIds.Should().NotContain(otherBatchStudentUser.Id);
        eligibleIds.Should().NotContain(unassignedTeacherUser.Id);
        eligibleIds.Should().NotContain(studentAUser.Id);
    }


    private static User CreateUser(string first, string last, UserRole role, Guid orgId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = first,
            LastName = last,
            Email = $"{first.ToLower()}.{last.ToLower()}@test.com",
            Role = role,
            OrganizationId = orgId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
