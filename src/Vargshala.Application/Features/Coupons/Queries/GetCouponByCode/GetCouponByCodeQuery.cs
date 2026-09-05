using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Queries.GetCouponByCode;

public record GetCouponByCodeQuery(string Code) : IRequest<ApiResponse<CouponDto>>;
