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

public interface IAuddService {
    public Task<string> Recognise(string path, Guid id);
}

public class AuddService : IAuddService {
    private readonly MusicHub _musicHub;

    public AuddService(MusicHub musicHub) { _musicHub = musicHub; }


    private record RecogniseResponse(string status, RecogniseResult result);

    private record RecogniseResult(string artist,
        string title,
        string album,
        string label
    );

    public async Task<string> Recognise(string path, Guid id) {
        await _musicHub.SendMessageToGroup(id.ToString(), "Recognising song");
        string url = "https://api.audd.io/recognize";
        string apiKey = API_KEY;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("apikey", apiKey);
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(File.OpenRead(path)), "file", "file.mp3");
        var response = await client.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<RecogniseResponse>(responseString);
        if (result.status == "success") {
            await _musicHub.SendMessageToGroup(id.ToString(),
                "Song recognised successfully " + result.result.artist + " " + result.result.title);
            return result.result.artist;
        }
        else {
            await _musicHub.SendMessageToGroup(id.ToString(), "Couldn't recognise song", "error");
        }

        return null;
    }
}