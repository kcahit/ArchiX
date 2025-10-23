// File: tests/ArchiXTest.ApiWeb/Controllers/PingController.cs
#nullable enable
using ArchiX.Library.External;

using Microsoft.AspNetCore.Mvc;

namespace ArchiXTest.ApiWeb.Controllers;

[ApiController]
public sealed class PingController(IPingAdapter adapter, ILogger<PingController> logger) : ControllerBase
{
    private readonly IPingAdapter _adapter = adapter;
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
