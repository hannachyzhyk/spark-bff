using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Spark.Gateway.Middleware;

public class JwtValidationMiddleware
{
	private readonly RequestDelegate _next;

	public JwtValidationMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async System.Threading.Tasks.Task Invoke(
	    HttpContext context,
	    User.V1.UserService.UserServiceClient userClient)
	{
		var endpoint = context.GetEndpoint();

		if (endpoint == null)
		{
			await _next(context);
			return;
		}

		var allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>();

		if (allowAnonymous != null)
		{
			await _next(context);
			return;
		}

		var authorize = endpoint.Metadata.GetMetadata<IAuthorizeData>();

		if (authorize == null)
		{
			await _next(context);
			return;
		}

		var authHeader = context.Request.Cookies["access_token"];

		if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
		{
			context.Response.StatusCode = 401;
			return;
		}

		var token = authHeader.Substring("Bearer ".Length);

		var response = await userClient.validateTokenAsync(
		    new User.V1.ValidateTokenRequest { Token = token });

		if (!response.Valid)
		{
			context.Response.StatusCode = 401;
			return;
		}

		var claims = new List<Claim>
	{
	    new Claim(ClaimTypes.NameIdentifier, response.UserId)
	};

		foreach (var role in response.Roles)
		{
			claims.Add(new Claim(ClaimTypes.Role, role));
		}

		var identity = new ClaimsIdentity(claims, "GrpcJwt");
		context.User = new ClaimsPrincipal(identity);

		await _next(context);
	}
}
