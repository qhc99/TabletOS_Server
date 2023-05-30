using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

public class AuthEndpoints : IEndpointsMapper
{
    public record LoginRequest(string Username, string Password);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/attribute-protected",
            [Authorize] () => "This endpoint is protected using the Authorize attribute");
        app.MapGet(
            "/api/method-protected",
            () => "This endpoint is protected using the RequireAuthorization method").
            RequireAuthorization();

        app.MapGet("/api/me", [Authorize] (ClaimsPrincipal user) => $"Logged username: {user.Identity?.Name}");

        // role authorization
        app.MapGet(
            "/api/admin-attribute-protected",
            [Authorize(Roles = "Admin")] () =>
            { });
        app.MapGet(
            "/api/admin-method-protected",
            () => { }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        // role check
        app.MapGet(
            "/api/role-check",
            [Authorize] (ClaimsPrincipal user) =>
            {
                if (user.IsInRole("Administrator"))
                {
                    return "User is an Administrator";
                }
                return "This is a normal user";
            });

        // policy based auth
        app.MapGet("/api/policy-attribute-protected",
        [Authorize(Policy = "Tenant42")] () => { });

        app.MapGet("/api/policy-method-protected", () => { })
        .RequireAuthorization("Tenant42");

        // dynamic policy
        app.MapGet("/api/custom-policy-protected",
            [Authorize(Policy = "TimedAccessPolicy")] () => { });


        // token issuer
        app.MapPost(
            "/api/auth/login",
            [AllowAnonymous] (LoginRequest request) =>
            {
                if (request.Username == "marco" &&
                    request.Password == "P@$$w0rd")
                {
                    // Generate the JWT bearer...
                    var claims = new List<Claim>()
                        {
                            new(ClaimTypes.Name, request.Username),
                            new(ClaimTypes.Role, "Admin"),
                            new(ClaimTypes.Role, "User"),
                        };
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mysecuritystring"));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var jwtSecurityToken = new JwtSecurityToken(
                        issuer: "https://www.packtpub.com",
                        audience: "Minimal APIs Client",
                        claims: claims,
                        expires: DateTime.UtcNow.AddHours(1),
                        signingCredentials: credentials);
                    var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                    return Results.Ok(new { AccessToken = accessToken });
                }
                return Results.BadRequest();
            });
    }
}