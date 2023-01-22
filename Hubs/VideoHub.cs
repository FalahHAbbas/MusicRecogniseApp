using Microsoft.AspNetCore.SignalR;

namespace MusicRecogniseApp.Hubs;

public class VideoHub : Hub {
    public async Task SendMessage(string message) {
        await Clients.All.SendAsync("newMessage", "anonymous", message);
    }

    public async Task AddToGroup(string groupName) {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Groups.AddToGroupAsync(Context.ConnectionId, "All");
    }

    public async Task SendMessageToGroup(string groupName, string message,string type="") {
        try {
            await Clients.Groups(groupName)
                .SendAsync("newMessage", type, message);
            Console.WriteLine(groupName + " " + message);
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    public async Task SendMessageToAll(string message) {
        Console.WriteLine("SendToAll");
        await Clients.Group("All")
            .SendAsync("newMessage", "anonymous", message);
    }
}