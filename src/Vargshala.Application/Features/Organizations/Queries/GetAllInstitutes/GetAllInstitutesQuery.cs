using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Queries.GetAllInstitutes;

public record GetAllInstitutesQuery : IRequest<ApiResponse<List<InstituteSummaryDto>>>;
