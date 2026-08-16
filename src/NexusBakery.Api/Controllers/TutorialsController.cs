using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBakery.Application.DTOs.Tutorials;
using NexusBakery.Application.Services;

namespace NexusBakery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TutorialsController : ControllerBase
{
    private readonly ITutorialService _tutorialService;

    public TutorialsController(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTutorials([FromQuery] TutorialSearchRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tutorialService.GetTutorialsAsync(request, userId);
        return Ok(result);
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular([FromQuery] int limit = 6)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tutorialService.GetPopularTutorialsAsync(limit, userId);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tutorial = await _tutorialService.GetTutorialBySlugAsync(slug, userId);
        return tutorial == null ? NotFound(new { message = $"Tutorial with slug '{slug}' not found." }) : Ok(tutorial);
    }

    [Authorize(Roles = "Admin,SystemAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateTutorial([FromBody] CreateTutorialRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var created = await _tutorialService.CreateTutorialAsync(request, userId);
        return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
    }
}
