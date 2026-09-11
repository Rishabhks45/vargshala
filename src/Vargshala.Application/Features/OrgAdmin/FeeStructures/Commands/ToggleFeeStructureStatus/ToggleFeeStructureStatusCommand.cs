using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.ToggleFeeStructureStatus;

public record ToggleFeeStructureStatusCommand(Guid Id) : IRequest<ApiResponse<bool>>;
