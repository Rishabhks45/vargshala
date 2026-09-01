using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, ApiResponse<PagedResponse<UserDto>>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetUsersQueryHandler(IVargshalaDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<UserDto>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<PagedResponse<UserDto>>.FailureResponse(
                "No organization associated with this user.");
        }

        var query = _db.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId == _currentUser.OrganizationId && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = users.Select(u =>
        {
            var dto = u.Adapt<UserDto>();
            dto.Role = u.Role.ToString();
            return dto;
        }).ToList();

        var response = new PagedResponse<UserDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return ApiResponse<PagedResponse<UserDto>>.SuccessResponse(response);
    }
}
