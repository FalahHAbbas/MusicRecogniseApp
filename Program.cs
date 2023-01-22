using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MusicRecogniseApp.Hubs;
using MusicRecogniseApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IYoutubeService, YoutubeService>();
builder.Services.AddScoped<IAuddService, AuddService>();
builder.Services.AddScoped<IFfmpegService, FfmpegService>();
builder.Services.AddScoped<IMusicService, MusicService>();
builder.Services.AddSingleton<MusicHub>();

var app = builder.Build();
app.UseCors("CorsPolicy");

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseRouting();

app.UseEndpoints(builder => { builder.MapHub<MusicHub>("/music"); });
app.Run();