using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Users.Commands.CreateControlPanelUser;

public class CreateControlPanelUserCommandHandler : IRequestHandler<CreateControlPanelUserCommand, ApiResponse<UserDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public CreateControlPanelUserCommandHandler(
        IVargshalaDbContext db,
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _db = db;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public async Task<ApiResponse<UserDto>> Handle(
        CreateControlPanelUserCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // 1. Check duplicate email across active users
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<UserDto>.FailureResponse("A user with this email address already exists.");
        }

        Organization? org = null;

        // 2. Conditional organization validation
        if (request.Role == UserRole.OrganizationAdmin)
        {
            if (!request.OrganizationId.HasValue || request.OrganizationId.Value == Guid.Empty)
            {
                return ApiResponse<UserDto>.FailureResponse("Please select an organization for the OrganizationAdmin.");
            }

            org = await _db.Organizations
                .FirstOrDefaultAsync(o => o.Id == request.OrganizationId.Value && !o.IsDeleted, cancellationToken);

            if (org is null)
            {
                return ApiResponse<UserDto>.FailureResponse("The selected organization was not found.");
            }
        }

        // 3. Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = (request.Role == UserRole.SuperAdmin || request.Role == UserRole.BackOffice) ? null : request.OrganizationId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            Mobile = string.IsNullOrWhiteSpace(request.Mobile) ? null : request.Mobile.Trim(),
            PasswordHash = _encryptionService.Encrypt(request.Password, _encryptionSettings.MasterKey),
            Role = request.Role,
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 4. Map to response DTO
        var dto = user.Adapt<UserDto>();
        dto.Role = user.Role;
        dto.OrganizationName = org?.Name;
        dto.OrganizationCode = org?.Code;

        var roleLabel = user.Role.ToString();
        return ApiResponse<UserDto>.SuccessResponse(dto, $"{roleLabel} account created successfully.");
    }
}
