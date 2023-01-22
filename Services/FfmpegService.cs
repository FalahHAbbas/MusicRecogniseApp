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

public interface IFfmpegService {
    public Task<string> ClipAndConvertToMp3(string path, Guid id);
}

public class FfmpegService : IFfmpegService {
    private readonly MusicHub _musicHub;

    public FfmpegService(MusicHub musicHub) { _musicHub = musicHub; }

    public async Task<string> ClipAndConvertToMp3(string path, Guid id) {
        await _musicHub.SendMessageToGroup(id.ToString(), "Clipping and converting to mp3");
        if (!File.Exists(path)) {
            await _musicHub.SendMessageToGroup(id.ToString(), "File not found", "error");
        }

        string savePath = $"{System.IO.Path.GetTempPath()}Clips";
        if (!Directory.Exists(savePath)) {
            Directory.CreateDirectory(savePath);
        }

        string fileName = $"{savePath}/{Guid.NewGuid()}.mp3";
        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = "ffmpeg",
            Arguments = $"-i \"{path}\" -ss 00:00:00 -t 00:01:00 \"{fileName}\" -f worst ",
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        using Process process = new Process {StartInfo = startInfo};
        process.Start();
        process.BeginOutputReadLine();
        process.OutputDataReceived += (sender, args) => {
            if (args.Data != null) {
                Console.WriteLine(args.Data);
                _musicHub.SendMessageToGroup(id.ToString(), Format(args.Data));
            }
        };
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
            return $"Converting {percentage}% at {speed} ETA {eta}";
        }
    }
}