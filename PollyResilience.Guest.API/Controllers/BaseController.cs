using Microsoft.AspNetCore.Mvc;

namespace PollyResilience.Client.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController<TController>(ILogger<TController> logger) : ControllerBase
{
    protected readonly ILogger<TController> Logger = logger;
}