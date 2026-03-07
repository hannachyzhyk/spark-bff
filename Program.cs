using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Spark.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins("https://localhost:5173")
		  .AllowAnyMethod()
		  .AllowAnyHeader()
		  .AllowCredentials();
	});
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
	loggingBuilder.ClearProviders();
	loggingBuilder.AddSerilog();
});

builder.Services.AddGrpcClient<User.V1.UserService.UserServiceClient>(o =>
{
	o.Address = new Uri(builder.Configuration.GetValue<string>("Services:UserService:BaseUrl"));
});
builder.Services.AddControllers();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	Console.WriteLine("Running in development environment");
}

app.UseMiddleware<JwtValidationMiddleware>();
app.UseMiddleware<RefreshCookieMiddleware>();

// app.UseAuthentication(); not needed since we're using custom middleware for authentication
app.UseAuthorization();

app.MapControllers();

app.Run();
