using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Queries.GetEligibleRecipients;

public record GetEligibleRecipientsQuery(GetEligibleRecipientsRequest Request) 
    : IRequest<ApiResponse<PagedResponse<EligibleUserDto>>>;
