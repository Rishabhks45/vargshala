using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Coupons.Commands.ToggleCouponStatus;

public record ToggleCouponStatusCommand(Guid Id) : IRequest<ApiResponse<bool>>;
