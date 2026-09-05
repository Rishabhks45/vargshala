using FluentValidation;
using MediatR;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.CreateBranch;

public record CreateBranchCommand(CreateBranchRequest Request) : IRequest<ApiResponse<BranchDto>>;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new CreateBranchRequestValidator());
    }
}
