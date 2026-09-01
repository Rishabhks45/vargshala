using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? Mobile,
    string Password,
    string Role
) : IRequest<ApiResponse<UserDto>>;
