using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.CreateFeeStructure;

public record CreateFeeStructureCommand(CreateFeeStructureRequest Request) : IRequest<ApiResponse<FeeStructureDto>>;
