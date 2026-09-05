using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Coupons.Commands.DeleteCoupon;

public record DeleteCouponCommand(Guid Id) : IRequest<ApiResponse<bool>>;
