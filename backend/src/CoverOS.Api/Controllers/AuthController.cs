using System.Security.Claims;
using CoverOS.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoverOS.Api.Controllers;
[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/auth")]
public sealed class AuthController(AccountService accounts) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("authentication")]
    public async Task<ActionResult<SessionDto>> Register(RegisterRequest request, CancellationToken ct)
    {
        try { return Ok(await accounts.RegisterAsync(request, ct)); }
        catch (AccountConflictException) { return Conflict(new ProblemDetails { Title = "Unable to register. Try signing in or use another email." }); }
        catch (ArgumentException) { return BadRequest(new ProblemDetails { Title = "Name and agency name are required." }); }
    }
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("authentication")]
    public async Task<ActionResult<SessionDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var session = await accounts.LoginAsync(request, ct);
        return session is null ? Unauthorized(new ProblemDetails { Title = "Email or password is incorrect." }) : Ok(session);
    }
    [Authorize(Roles = "AgencyAdmin,AgencyStaff,Contractor")]
    [HttpGet("me")]
    public async Task<ActionResult<AccountDto>> Me(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            !Guid.TryParse(User.FindFirstValue("organization_id"), out var organizationId)) return Unauthorized();
        var account = await accounts.GetAccountAsync(userId, organizationId, ct);
        return account is null ? Unauthorized() : Ok(account);
    }
}
