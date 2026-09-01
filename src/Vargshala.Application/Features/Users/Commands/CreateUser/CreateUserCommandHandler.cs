using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;
using Vargshala.Domain.Entities;
using Vargshala.Domain.Enums;

namespace Vargshala.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IVargshalaDbContext db,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<UserDto>.FailureResponse("You must belong to an organization to create users.");
        }

        if (!Enum.TryParse<Role>(request.Role, ignoreCase: true, out var role))
        {
            return ApiResponse<UserDto>.FailureResponse($"Invalid role: {request.Role}. Valid roles: {string.Join(", ", Enum.GetNames<Role>())}");
        }

        // Prevent creating SuperAdmin or OrganizationAdmin through this endpoint
        if (role is Role.SuperAdmin)
        {
            return ApiResponse<UserDto>.FailureResponse("Cannot create a SuperAdmin user through this endpoint.");
        }

        // Check for duplicate email within the organization
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email == request.Email
                    && u.OrganizationId == _currentUser.OrganizationId
                    && !u.IsDeleted, cancellationToken);

            if (emailExists)
            {
                return ApiResponse<UserDto>.FailureResponse("A user with this email already exists in your organization.");
            }
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = _currentUser.OrganizationId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Mobile = request.Mobile,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = user.Adapt<UserDto>();
        dto.Role = user.Role.ToString();

        return ApiResponse<UserDto>.SuccessResponse(dto, "User created successfully.");
    }
}
