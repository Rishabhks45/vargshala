using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.UpdateFeeStructure;

public record UpdateFeeStructureCommand(UpdateFeeStructureRequest Request) : IRequest<ApiResponse<FeeStructureDto>>;
