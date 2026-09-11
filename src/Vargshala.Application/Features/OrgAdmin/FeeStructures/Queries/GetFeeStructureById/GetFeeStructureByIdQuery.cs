using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructureById;

public record GetFeeStructureByIdQuery(Guid Id) : IRequest<ApiResponse<FeeStructureDto>>;
