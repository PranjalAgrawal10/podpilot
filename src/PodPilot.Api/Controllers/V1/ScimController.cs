using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Models.Security;
using PodPilot.Application.Security;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// SCIM 2.0 user and group provisioning endpoints.
/// Requires authenticated callers with Security.Manage.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/scim/v2")]
[Produces("application/json")]
public sealed class ScimController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScimController"/> class.
    /// </summary>
    public ScimController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Creates a SCIM user.</summary>
    [HttpPost("Users")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var request = MapUser(body);
        var result = await mediator.Send(new UpsertScimUserCommand { Request = request }, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = result.ExternalId }, result);
    }

    /// <summary>Updates a SCIM user (PUT).</summary>
    [HttpPut("Users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PutUser(string id, [FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var request = MapUser(body);
        request = new ScimUserRequest
        {
            ExternalId = string.IsNullOrWhiteSpace(request.ExternalId) ? id : request.ExternalId,
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Active = request.Active,
            Groups = request.Groups,
        };
        var result = await mediator.Send(new UpsertScimUserCommand { Request = request }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Patches a SCIM user (active flag / disable).</summary>
    [HttpPatch("Users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PatchUser(string id, [FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var active = true;
        if (body.TryGetProperty("active", out var activeEl) && activeEl.ValueKind is JsonValueKind.False)
        {
            active = false;
        }

        if (body.TryGetProperty("Operations", out var ops) && ops.ValueKind == JsonValueKind.Array)
        {
            foreach (var op in ops.EnumerateArray())
            {
                if (op.TryGetProperty("path", out var path) &&
                    string.Equals(path.GetString(), "active", StringComparison.OrdinalIgnoreCase) &&
                    op.TryGetProperty("value", out var value) &&
                    value.ValueKind == JsonValueKind.False)
                {
                    active = false;
                }
            }
        }

        if (!active)
        {
            await mediator.Send(new DisableScimUserCommand { ExternalUserId = id }, cancellationToken);
            return Ok(new { id, active = false });
        }

        return Ok(new { id, active = true });
    }

    /// <summary>Gets a SCIM user by external id (metadata only).</summary>
    [HttpGet("Users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUser(string id) =>
        Ok(new { id, schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" } });

    /// <summary>Creates or syncs a SCIM group.</summary>
    [HttpPost("Groups")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGroup([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var request = MapGroup(body);
        await mediator.Send(new SyncScimGroupCommand { Request = request }, cancellationToken);
        return CreatedAtAction(nameof(GetGroup), new { id = request.ExternalGroupId }, request);
    }

    /// <summary>Updates a SCIM group.</summary>
    [HttpPut("Groups/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PutGroup(string id, [FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var request = MapGroup(body, id);
        await mediator.Send(new SyncScimGroupCommand { Request = request }, cancellationToken);
        return Ok(request);
    }

    /// <summary>Gets a SCIM group.</summary>
    [HttpGet("Groups/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetGroup(string id) =>
        Ok(new { id, schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:Group" } });

    private static ScimUserRequest MapUser(JsonElement body)
    {
        var email = body.TryGetProperty("userName", out var userName) ? userName.GetString() ?? string.Empty : string.Empty;
        if (body.TryGetProperty("emails", out var emails) && emails.ValueKind == JsonValueKind.Array)
        {
            foreach (var e in emails.EnumerateArray())
            {
                if (e.TryGetProperty("value", out var value))
                {
                    email = value.GetString() ?? email;
                    break;
                }
            }
        }

        string? firstName = null;
        string? lastName = null;
        if (body.TryGetProperty("name", out var name))
        {
            firstName = name.TryGetProperty("givenName", out var gn) ? gn.GetString() : null;
            lastName = name.TryGetProperty("familyName", out var fn) ? fn.GetString() : null;
        }

        var active = !body.TryGetProperty("active", out var activeEl) || activeEl.ValueKind != JsonValueKind.False;
        var externalId = body.TryGetProperty("externalId", out var ext) ? ext.GetString() ?? string.Empty : string.Empty;
        if (string.IsNullOrWhiteSpace(externalId) && body.TryGetProperty("id", out var idEl))
        {
            externalId = idEl.GetString() ?? string.Empty;
        }

        return new ScimUserRequest
        {
            ExternalId = externalId,
            UserName = body.TryGetProperty("userName", out var un) ? un.GetString() ?? email : email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Active = active,
            Groups = [],
        };
    }

    private static ScimGroupRequest MapGroup(JsonElement body, string? fallbackId = null)
    {
        var id = body.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        id ??= body.TryGetProperty("externalId", out var ext) ? ext.GetString() : null;
        id ??= fallbackId ?? Guid.NewGuid().ToString("N");

        var members = new List<string>();
        if (body.TryGetProperty("members", out var membersEl) && membersEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var m in membersEl.EnumerateArray())
            {
                if (m.TryGetProperty("value", out var value) && value.GetString() is { } memberId)
                {
                    members.Add(memberId);
                }
            }
        }

        return new ScimGroupRequest
        {
            ExternalGroupId = id,
            DisplayName = body.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
            OrganizationRole = body.TryGetProperty("organizationRole", out var role)
                ? role.GetString() ?? "Viewer"
                : "Viewer",
            MemberExternalIds = members,
        };
    }
}
