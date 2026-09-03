using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<UserDto>.FailureResponse("You must belong to an organization to create users.");
        }

        // Prevent creating SuperAdmin or OrganizationAdmin through this endpoint
        if (request.Role is UserRole.SuperAdmin)
        {
            return ApiResponse<UserDto>.FailureResponse("Cannot create a SuperAdmin user through this endpoint.");
        }

        // Check for duplicate email within the organization
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _userRepository.ExistsByEmailAndOrgAsync(
                request.Email, _currentUser.OrganizationId.Value, cancellationToken);

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
            PasswordHash = _encryptionService.Encrypt(request.Password, _encryptionSettings.MasterKey),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var dto = user.Adapt<UserDto>();
        dto.Role = user.Role;

        return ApiResponse<UserDto>.SuccessResponse(dto, "User created successfully.");
    }
}
