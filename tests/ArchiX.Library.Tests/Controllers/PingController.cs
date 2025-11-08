// File: tests/ArchiX.Library.Tests/Controllers/PingController.cs
#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Tests.Controllers;

[ApiController]
public sealed class PingController(ArchiX.Library.Abstractions.External.IPingAdapter adapter, ILogger<PingController> logger) : ControllerBase
{
    private readonly ArchiX.Library.Abstractions.External.IPingAdapter _adapter = adapter;
    private readonly ILogger<PingController> _logger = logger;

    [HttpGet("/ping/status")]
    public Task<IActionResult> GetStatus(CancellationToken _)
        => Task.FromResult<IActionResult>(new ContentResult
        {
            Content = "pong",
            ContentType = "text/plain; charset=utf-8",
            StatusCode = 200
        });

    [HttpGet("/ping/status.json")]
    public async Task<IActionResult> GetStatusJson(CancellationToken ct)
    {
        var json = await _adapter.GetStatusTextAsync(ct);
        return new ContentResult
        {
            Content = json,
            ContentType = "application/json; charset=utf-8",
            StatusCode = 200
        };
    }
}
