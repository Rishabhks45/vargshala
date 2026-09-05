using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Queries.GetCouponById;

public record GetCouponByIdQuery(Guid Id) : IRequest<ApiResponse<CouponDto>>;
