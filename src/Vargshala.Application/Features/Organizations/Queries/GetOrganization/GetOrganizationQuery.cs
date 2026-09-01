using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Queries.GetOrganization;

public record GetOrganizationQuery() : IRequest<ApiResponse<OrganizationDto>>;
