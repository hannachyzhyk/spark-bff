namespace Spark.Gateway.Middleware;

public class RefreshCookieMiddleware
{
	private readonly RequestDelegate _next;

	public RefreshCookieMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async System.Threading.Tasks.Task Invoke(HttpContext context)
	{
		if (context.Request.Cookies.TryGetValue("access_token", out var token))
		{
			var options = new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,
				Expires = DateTimeOffset.UtcNow.AddHours(1) // new expiry
			};

			context.Response.Cookies.Append("access_token", token, options);
		}

		await _next(context);
	}
}
