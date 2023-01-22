using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MusicRecogniseApp.Hubs;

namespace MusicRecogniseApp.Services;

public interface IYoutubeService {
    public Task<string> Fetch(string url, Guid id);
    public Task<List<Song>> GetSongs(string artist, Guid id);
}

public record Song(string title, string artist, string image, string url);

public class YoutubeService : IYoutubeService {
    private readonly YouTubeService _service =
        new(new BaseClientService.Initializer() {
            ApiKey = API_KEY
        });

    private readonly MusicHub _musicHub;

    public YoutubeService(MusicHub musicHub) { _musicHub = musicHub; }

    public async Task<string> Fetch(string url, Guid id) {
        string savePath = $"{System.IO.Path.GetTempPath()}Videos";
        if (!Directory.Exists(savePath)) {
            Directory.CreateDirectory(savePath);
        }

        string fileName = $"{savePath}/{Guid.NewGuid()}.mp4";
        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = "youtube-dl",
            Arguments = $"--merge-output-format mp4 -o  \"{fileName}\" {url}",
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        using Process process = new Process {StartInfo = startInfo};
        process.Start();
        while (!process.StandardOutput.EndOfStream) {
            string line = process.StandardOutput.ReadLine();
            // Console.WriteLine(line);
            await _musicHub.SendMessageToGroup(id.ToString(), Format(line));
        }

        await process.WaitForExitAsync();
        return fileName;

        string Format(string line) {
            var percentage = Regex.Match(line, @"(\d{1,3}\.\d)%")
                .Groups[1]
                .Value;
            var speed = Regex.Match(line, @"(\d{1,3}\.\d{1,3}(K|M)iB/s)")
                .Groups[1]
                .Value;
            var eta = Regex.Match(line, @"ETA (\d{1,2}:\d{1,2})")
                .Groups[1]
                .Value;
            return $"Downloading {percentage}% at {speed} ETA {eta}";
        }
    }


    public async Task<List<Song>> GetSongs(string artist, Guid id) {
        await _musicHub.SendMessageToGroup(id.ToString(), "Fetching songs");
        var searchListRequest = _service.Search.List("snippet");
        searchListRequest.Q = artist;
        searchListRequest.MaxResults = 30;
        searchListRequest.Type = "video";

        var searchListResponse = await searchListRequest.ExecuteAsync();
        if (searchListResponse.Items.Count == 0) {
            await _musicHub.SendMessageToGroup(id.ToString(), "couldn't get songs", "error");
        }
        else {
            await _musicHub.SendMessageToGroup(id.ToString(), "Songs fetched successfully");
        }

        List<Song> songs = searchListResponse.Items.Select(item => new Song(
                item.Snippet.Title,
                artist,
                item.Snippet.Thumbnails.High.Url,
                $"https://www.youtube.com/watch?v={item.Id.VideoId}"
            ))
            .ToList();

        string json = JsonSerializer.Serialize(songs);
        await _musicHub.SendMessageToGroup(id.ToString(), json, "data");
        return songs;
    }
}