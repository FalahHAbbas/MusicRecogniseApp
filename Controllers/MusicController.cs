using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MusicRecogniseApp.Hubs;
using MusicRecogniseApp.Services;

namespace MusicRecogniseApp.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase {
    private readonly IMusicService _musicService;
    private readonly VideoHub _videoHub;

    public MusicController(IMusicService musicService, VideoHub videoHub) {
        _musicService = musicService;
        _videoHub = videoHub;
    }

    [HttpGet]
    public async Task<ActionResult<string>> Upload(string url) {
        var id = await _musicService.Process(url);
        return Ok(id);
    }
}