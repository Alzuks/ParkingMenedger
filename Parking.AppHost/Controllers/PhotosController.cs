using Microsoft.AspNetCore.Mvc;
using Parking.Infrastructure.Persistence;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/photos")]

public sealed class PhotosController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public PhotosController(IConfiguration cfg) => _cfg = cfg;

    // Пример:
    // GET /api/photos/file?name=20260213_183348_000_T851OX_Reverse_c94.jpg
    [HttpGet("file")]
    public IActionResult Get([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("name is empty");

        // запрет на папки и ".."
        if (name.Contains("..") || name.Contains('\\') || name.Contains('/'))
            return BadRequest("invalid name");

        var root = _cfg.GetValue<string>("Parking:SnapshotsRoot");
        if (string.IsNullOrWhiteSpace(root))
            return StatusCode(500, "SnapshotsRoot not configured");

        var fullPath = Path.Combine(root, name);

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, "image/jpeg");
    }
    // GET /api/photos/by-passage?id=123
}
