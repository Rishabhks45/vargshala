using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Users.Commands.UpdateControlPanelUser;

public class UpdateControlPanelUserCommandHandler : IRequestHandler<UpdateControlPanelUserCommand, ApiResponse<UserDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly IUserRepository _userRepository;

    public UpdateControlPanelUserCommandHandler(
        IVargshalaDbContext db,
        IUserRepository userRepository)
    {
        _db = db;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<UserDto>> Handle(
        UpdateControlPanelUserCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var user = await _userRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);

        if (user == null)
        {
            return ApiResponse<UserDto>.FailureResponse("Administrator not found.");
        }

        // Check if email changed and duplicates
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            var normalizedEmail = req.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Users
                .AnyAsync(u => u.Id != req.Id && u.Email != null && u.Email.ToLower() == normalizedEmail && !u.IsDeleted, cancellationToken);

            if (emailExists)
            {
                return ApiResponse<UserDto>.FailureResponse("A user with this email address already exists.");
            }

            user.Email = normalizedEmail;
        }

        Organization? org = null;

        // Role & Organization validation
        if (req.Role == UserRole.OrganizationAdmin)
        {
            if (!req.OrganizationId.HasValue || req.OrganizationId.Value == Guid.Empty)
            {
                return ApiResponse<UserDto>.FailureResponse("Please select an organization for the OrganizationAdmin.");
            }

            org = await _db.Organizations
                .FirstOrDefaultAsync(o => o.Id == req.OrganizationId.Value && !o.IsDeleted, cancellationToken);

            if (org is null)
            {
                return ApiResponse<UserDto>.FailureResponse("The selected organization was not found.");
            }

            user.OrganizationId = req.OrganizationId;
        }
        else
        {
            user.OrganizationId = null;
        }

        user.FirstName = req.FirstName.Trim();
        user.LastName = req.LastName.Trim();
        user.Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim();
        user.Role = req.Role;
        user.IsActive = req.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var dto = user.Adapt<UserDto>();
        dto.Role = user.Role;
        dto.OrganizationName = org?.Name;
        dto.OrganizationCode = org?.Code;

        return ApiResponse<UserDto>.SuccessResponse(dto, "Administrator updated successfully.");
    }
}
