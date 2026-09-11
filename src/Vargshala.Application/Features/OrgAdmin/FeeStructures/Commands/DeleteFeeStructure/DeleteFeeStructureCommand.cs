using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.DeleteFeeStructure;

public record DeleteFeeStructureCommand(Guid Id) : IRequest<ApiResponse<bool>>;
