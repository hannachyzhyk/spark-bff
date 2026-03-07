using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Spark.Gateway.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskService.TaskServiceClient _taskClient;
    private readonly IConfiguration _configuration;

    public TasksController(IConfiguration configuration)
    {
        _configuration = configuration;
        _taskClient = new TaskService.TaskServiceClient(GrpcChannel.ForAddress(_configuration["Services:TaskService:BaseUrl"]));
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllTasks()
    {
        // var response = await _taskClient.GetTasksAsync(new GetTasksRequest());
        return Ok("Tasks gonna be here");
    }
}
