using MediatR;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, ApiResponse<BranchDto>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public CreateBranchCommandHandler(
        IBranchRepository branchRepository,
        ICurrentUser currentUser,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public async Task<ApiResponse<BranchDto>> Handle(
        CreateBranchCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BranchDto>.FailureResponse("No active organization context found.");
        }

        var code = req.Code.Trim();

        // Check duplicate code
        var exists = await _branchRepository.ExistsByCodeAsync(code, null, cancellationToken);
        if (exists)
        {
            return ApiResponse<BranchDto>.FailureResponse($"A branch with code '{code}' already exists in your institute.");
        }

        // If Branch Admin information is provided, verify email uniqueness via Repository
        var hasAdminInfo = !string.IsNullOrWhiteSpace(req.AdminEmail) && !string.IsNullOrWhiteSpace(req.AdminFirstName);
        string? adminEmailLower = null;
        if (hasAdminInfo)
        {
            adminEmailLower = req.AdminEmail!.Trim().ToLowerInvariant();
            var emailExists = await _branchRepository.IsUserEmailTakenAsync(adminEmailLower, null, cancellationToken);
            if (emailExists)
            {
                return ApiResponse<BranchDto>.FailureResponse($"A user with email '{req.AdminEmail}' already exists in the system.");
            }
        }

        // If this branch is marked as main branch, demote any other main branch in this organization via Repository
        if (req.IsMainBranch)
        {
            await _branchRepository.DemoteOtherMainBranchesAsync(orgId.Value, Guid.Empty, _currentUser.UserId, cancellationToken);
        }

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId.Value,
            Name = req.Name.Trim(),
            Code = code,
            LogoUrl = string.IsNullOrWhiteSpace(req.LogoUrl) ? null : req.LogoUrl.Trim(),
            Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim().ToLowerInvariant(),
            Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim(),
            AlternateMobile = string.IsNullOrWhiteSpace(req.AlternateMobile) ? null : req.AlternateMobile.Trim(),
            Address = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim(),
            City = string.IsNullOrWhiteSpace(req.City) ? null : req.City.Trim(),
            State = string.IsNullOrWhiteSpace(req.State) ? null : req.State.Trim(),
            Pincode = string.IsNullOrWhiteSpace(req.Pincode) ? null : req.Pincode.Trim(),
            Country = string.IsNullOrWhiteSpace(req.Country) ? null : req.Country.Trim(),
            IsMainBranch = req.IsMainBranch,
            UseBranchName = req.UseBranchName,
            IsActive = req.IsActive,
            CreatedBy = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _branchRepository.AddAsync(branch, cancellationToken);

        User? branchAdmin = null;
        if (hasAdminInfo && !string.IsNullOrWhiteSpace(adminEmailLower))
        {
            var password = !string.IsNullOrWhiteSpace(req.AdminPassword) ? req.AdminPassword : "Password@123";
            branchAdmin = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                FirstName = req.AdminFirstName!.Trim(),
                LastName = string.IsNullOrWhiteSpace(req.AdminLastName) ? "" : req.AdminLastName.Trim(),
                Email = adminEmailLower,
                Mobile = string.IsNullOrWhiteSpace(req.AdminMobile) ? null : req.AdminMobile.Trim(),
                PasswordHash = _encryptionService.Encrypt(password, _encryptionSettings.MasterKey),
                Role = UserRole.BranchAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            await _branchRepository.CreateBranchAdminAsync(branchAdmin, branch.Id, _currentUser.UserId, cancellationToken);
        }

        await _branchRepository.SaveChangesAsync(cancellationToken);

        var dto = branch.ToDto();
        if (branchAdmin != null)
        {
            dto.BranchAdminId = branchAdmin.Id;
            dto.BranchAdminName = $"{branchAdmin.FirstName} {branchAdmin.LastName}".Trim();
            dto.BranchAdminEmail = branchAdmin.Email;
            dto.BranchAdminMobile = branchAdmin.Mobile;
        }

        return ApiResponse<BranchDto>.SuccessResponse(dto, "Branch created successfully.");
    }
}
