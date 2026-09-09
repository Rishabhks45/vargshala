using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.ToggleBatchStatus;

public record ToggleBatchStatusCommand(Guid Id) : IRequest<ApiResponse<bool>>;
