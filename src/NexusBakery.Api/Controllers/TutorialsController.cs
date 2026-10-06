using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusBakery.Application.DTOs.Tutorials;
using NexusBakery.Application.Interfaces;

namespace NexusBakery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TutorialsController : ControllerBase
{
    private readonly ITutorialService _tutorialService;
    private readonly IFileStorageService _fileStorageService;

    public TutorialsController(ITutorialService tutorialService, IFileStorageService fileStorageService)
    {
        _tutorialService = tutorialService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTutorials()
    {
        var tutorials = await _tutorialService.GetAllAsync();
        return Ok(tutorials);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTutorialById(string id)
    {
        var tutorial = await _tutorialService.GetByIdAsync(id);
        if (tutorial == null) return NotFound(new { message = "Tutorial not found" });
        return Ok(tutorial);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, SystemAdmin")]
    public async Task<IActionResult> CreateTutorial([FromBody] CreateTutorialDto dto)
    {
        var tutorial = await _tutorialService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTutorialById), new { id = tutorial.Id }, tutorial);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, SystemAdmin")]
    public async Task<IActionResult> UpdateTutorial(string id, [FromBody] UpdateTutorialDto dto)
    {
        var updated = await _tutorialService.UpdateAsync(id, dto);
        if (updated == null) return NotFound(new { message = "Tutorial not found" });
        return Ok(updated);
    }

    [HttpPost("{id}/media")]
    [Authorize(Roles = "Admin, SystemAdmin")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> UploadMedia(string id, IFormFile file, [FromQuery] bool isVideo = false)
    {
        var url = await _fileStorageService.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType, isVideo ? "videos" : "thumbnails");
        await _tutorialService.AddMediaAsync(id, url, isVideo);
        return Ok(new { Url = url });
    }
}
