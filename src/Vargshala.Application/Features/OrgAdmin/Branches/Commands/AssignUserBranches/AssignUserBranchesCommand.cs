using FluentValidation;
using MediatR;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.AssignUserBranches;

public record AssignUserBranchesCommand(AssignUserBranchesRequest Request) : IRequest<ApiResponse<bool>>;

public class AssignUserBranchesCommandValidator : AbstractValidator<AssignUserBranchesCommand>
{
    public AssignUserBranchesCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new AssignUserBranchesRequestValidator());
    }
}
