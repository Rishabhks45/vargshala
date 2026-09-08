using MediatR;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, ApiResponse<BranchDto>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public UpdateBranchCommandHandler(
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
        UpdateBranchCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BranchDto>.FailureResponse("No active organization context found.");
        }

        var branch = await _branchRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<BranchDto>.FailureResponse("Branch not found.");
        }

        var code = req.Code.Trim();

        // Check duplicate code
        var exists = await _branchRepository.ExistsByCodeAsync(code, req.Id, cancellationToken);
        if (exists)
        {
            return ApiResponse<BranchDto>.FailureResponse($"Another branch with code '{code}' already exists in your institute.");
        }

        // If this branch is being promoted to main branch, demote any other main branch in this organization
        if (req.IsMainBranch && !branch.IsMainBranch)
        {
            await _branchRepository.DemoteOtherMainBranchesAsync(orgId.Value, req.Id, _currentUser.UserId, cancellationToken);
        }

        branch.Name = req.Name.Trim();
        branch.Code = code;
        branch.LogoUrl = string.IsNullOrWhiteSpace(req.LogoUrl) ? null : req.LogoUrl.Trim();
        branch.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim().ToLowerInvariant();
        branch.Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim();
        branch.AlternateMobile = string.IsNullOrWhiteSpace(req.AlternateMobile) ? null : req.AlternateMobile.Trim();
        branch.Address = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim();
        branch.City = string.IsNullOrWhiteSpace(req.City) ? null : req.City.Trim();
        branch.State = string.IsNullOrWhiteSpace(req.State) ? null : req.State.Trim();
        branch.Pincode = string.IsNullOrWhiteSpace(req.Pincode) ? null : req.Pincode.Trim();
        branch.Country = string.IsNullOrWhiteSpace(req.Country) ? null : req.Country.Trim();
        branch.IsMainBranch = req.IsMainBranch;
        branch.UseBranchName = req.UseBranchName;
        branch.IsActive = req.IsActive;
        branch.UpdatedBy = _currentUser.UserId;
        branch.UpdatedAt = DateTime.UtcNow;

        _branchRepository.Update(branch);

        // Handle Branch Administrator user update or create via Repository
        User? branchAdmin = null;
        var hasAdminInfo = !string.IsNullOrWhiteSpace(req.AdminEmail) || !string.IsNullOrWhiteSpace(req.AdminFirstName);
        if (hasAdminInfo)
        {
            var adminEmailLower = req.AdminEmail?.Trim().ToLowerInvariant();
            var existingAdmin = await _branchRepository.GetBranchAdminForUpdateAsync(branch.Id, cancellationToken);

            if (existingAdmin != null)
            {
                branchAdmin = existingAdmin;

                // Check email uniqueness if email changed
                if (!string.IsNullOrWhiteSpace(adminEmailLower) && branchAdmin.Email?.ToLower() != adminEmailLower)
                {
                    var emailTaken = await _branchRepository.IsUserEmailTakenAsync(adminEmailLower, branchAdmin.Id, cancellationToken);
                    if (emailTaken)
                    {
                        return ApiResponse<BranchDto>.FailureResponse($"A user with email '{req.AdminEmail}' already exists in the system.");
                    }
                    branchAdmin.Email = adminEmailLower;
                }

                if (!string.IsNullOrWhiteSpace(req.AdminFirstName))
                    branchAdmin.FirstName = req.AdminFirstName.Trim();

                if (req.AdminLastName != null)
                    branchAdmin.LastName = req.AdminLastName.Trim();

                if (req.AdminMobile != null)
                    branchAdmin.Mobile = string.IsNullOrWhiteSpace(req.AdminMobile) ? null : req.AdminMobile.Trim();

                if (!string.IsNullOrWhiteSpace(req.AdminPassword))
                    branchAdmin.PasswordHash = _encryptionService.Encrypt(req.AdminPassword, _encryptionSettings.MasterKey);

                branchAdmin.UpdatedAt = DateTime.UtcNow;
                branchAdmin.UpdatedBy = _currentUser.UserId;
                _branchRepository.UpdateUser(branchAdmin);
            }
            else if (!string.IsNullOrWhiteSpace(adminEmailLower))
            {
                var emailTaken = await _branchRepository.IsUserEmailTakenAsync(adminEmailLower, null, cancellationToken);
                if (emailTaken)
                {
                    return ApiResponse<BranchDto>.FailureResponse($"A user with email '{req.AdminEmail}' already exists in the system.");
                }

                var password = !string.IsNullOrWhiteSpace(req.AdminPassword) ? req.AdminPassword : "Password@123";
                branchAdmin = new User
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgId.Value,
                    FirstName = !string.IsNullOrWhiteSpace(req.AdminFirstName) ? req.AdminFirstName.Trim() : "Branch",
                    LastName = string.IsNullOrWhiteSpace(req.AdminLastName) ? "Admin" : req.AdminLastName.Trim(),
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
        }
        else
        {
            branchAdmin = await _branchRepository.GetBranchAdminAsync(branch.Id, cancellationToken);
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

        return ApiResponse<BranchDto>.SuccessResponse(dto, "Branch updated successfully.");
    }
}
