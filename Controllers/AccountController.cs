using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Spark.Gateway.Models;

namespace Spark.Gateway.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
	private readonly ILogger<AccountController> _logger;
	private readonly IConfiguration _configuration;
	private readonly User.V1.UserService.UserServiceClient _userClient;

	public AccountController(ILogger<AccountController> logger, IConfiguration configuration, User.V1.UserService.UserServiceClient userClient)
	{
		_logger = logger;
		_configuration = configuration;
		_userClient = userClient;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterModel model)
	{
		// Example of appending a JWT to a response cookie
		try
		{

			var response = await _userClient.registerAsync(new User.V1.RegisterRequest { Username = model.Username, Password = model.Password });
			HttpContext.Response.Cookies.Append("access_token", $"Bearer {response.Token}", new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,
				Expires = DateTimeOffset.UtcNow.AddHours(1)
			});
			return Ok(new { message = "User registered successfully" });
		}
		catch (RpcException ex)
		{
			_logger.LogError(ex, "Error registering user: {Username}", model.Username);
			return BadRequest(new { message = "Registration failed" });
		}
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginModel model)
	{
		try
		{
			var response = await _userClient.loginAsync(new User.V1.LoginRequest { Username = model.Username, Password = model.Password });

			HttpContext.Response.Cookies.Append("access_token", $"Bearer {response.Token}", new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,
				Expires = DateTimeOffset.UtcNow.AddHours(1)
			});
			return Ok(new { message = "Login successful" });

		}
		catch (RpcException ex)
		{
			_logger.LogWarning("Invalid login attempt for user: {Username}", model.Username);
			return Unauthorized(new { message = "Invalid credentials" });

		}
	}
}
