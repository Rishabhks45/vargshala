using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Commands.CreateCoupon;

public record CreateCouponCommand(CreateCouponRequest Request) : IRequest<ApiResponse<Guid>>;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body cannot be null.")
            .SetValidator(new CreateCouponRequestValidator());
    }
}
