using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Application.Features.Messages.Security;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Queries.GetEligibleRecipients;

public class GetEligibleRecipientsQueryHandler 
    : IRequestHandler<GetEligibleRecipientsQuery, ApiResponse<PagedResponse<EligibleUserDto>>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IConversationAuthorizationService _authService;

    public GetEligibleRecipientsQueryHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser,
        IConversationAuthorizationService authService)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<ApiResponse<PagedResponse<EligibleUserDto>>> Handle(
        GetEligibleRecipientsQuery query, 
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == Guid.Empty || !_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<PagedResponse<EligibleUserDto>>.FailureResponse("User identity could not be verified.");
        }

        var req = query.Request ?? new GetEligibleRecipientsRequest();
        var eligibleIds = await _authService.GetEligibleDirectMessageRecipientUserIdsAsync(currentUserId, cancellationToken);

        if (!eligibleIds.Any())
        {
            var emptyResult = PagedResponse<EligibleUserDto>.Create(new List<EligibleUserDto>(), 0, req.PageNumber, req.PageSize);
            return ApiResponse<PagedResponse<EligibleUserDto>>.SuccessResponse(emptyResult);
        }

        var page = req.PageNumber <= 0 ? 1 : req.PageNumber;
        var pageSize = req.PageSize <= 0 ? 20 : req.PageSize;

        var (users, totalRecords) = await _messageRepository.GetEligibleUsersPagedAsync(
            eligibleIds, 
            req.SearchTerm, 
            page, 
            pageSize, 
            cancellationToken);

        var dtos = users.Select(u => new EligibleUserDto
        {
            UserId = u.Id,
            FullName = $"{u.FirstName} {u.LastName}".Trim(),
            Role = u.Role.ToString(),
            Email = u.Email,
            AvatarUrl = u.ProfilePictureUrl,
            Subtitle = $"{u.Role}"
        }).ToList();

        var pagedResponse = PagedResponse<EligibleUserDto>.Create(dtos, totalRecords, page, pageSize);
        return ApiResponse<PagedResponse<EligibleUserDto>>.SuccessResponse(pagedResponse);
    }
}
