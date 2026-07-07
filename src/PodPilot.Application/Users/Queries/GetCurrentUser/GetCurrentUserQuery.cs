using MediatR;
using PodPilot.Contracts.Users;

namespace PodPilot.Application.Users.Queries.GetCurrentUser;

/// <summary>
/// Query to retrieve the current authenticated user.
/// </summary>
public sealed class GetCurrentUserQuery : IRequest<UserResponse>
{
}
