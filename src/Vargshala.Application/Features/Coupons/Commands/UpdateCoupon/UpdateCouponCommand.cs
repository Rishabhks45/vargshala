using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Commands.UpdateCoupon;

public record UpdateCouponCommand(UpdateCouponRequest Request) : IRequest<ApiResponse<bool>>;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body cannot be null.")
            .SetValidator(new UpdateCouponRequestValidator());
    }
}
