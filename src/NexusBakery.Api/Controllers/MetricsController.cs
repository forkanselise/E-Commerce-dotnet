using Microsoft.AspNetCore.Mvc;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsRepository _metricsRepository;

    public MetricsController(IMetricsRepository metricsRepository)
    {
        _metricsRepository = metricsRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetGlobalMetrics()
    {
        var metrics = await _metricsRepository.GetGlobalMetricsAsync();
        return Ok(metrics);
    }
}
