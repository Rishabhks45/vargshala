using FluentValidation;
using MediatR;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(UpdateBranchRequest Request) : IRequest<ApiResponse<BranchDto>>;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new UpdateBranchRequestValidator());
    }
}
