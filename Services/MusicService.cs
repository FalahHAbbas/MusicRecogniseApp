namespace MusicRecogniseApp.Services;

public interface IMusicService {
    public Task<string> Process(string url);
}
public class MusicService : IMusicService {
    private readonly IYoutubeService _youtubeService;
    private readonly IAuddService _auddService;
    private readonly IFfmpegService _ffmpegService;

    public MusicService(IYoutubeService youtubeService, IAuddService auddService, IFfmpegService ffmpegService) {
        _youtubeService = youtubeService;
        _auddService = auddService;
        _ffmpegService = ffmpegService;
    }

    public async Task<string> Process(string url) {
        var id = Guid.NewGuid();
        Task.Run(async () => {
            var path = await _youtubeService.Fetch(url, id);
            var mp3Path = await _ffmpegService.ClipAndConvertToMp3(path, id);
            var artist = await _auddService.Recognise(mp3Path, id);
            if (artist != null) await _youtubeService.GetSongs(artist, id);
        });
        return id.ToString();
    }

}