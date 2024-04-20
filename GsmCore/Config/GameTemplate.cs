using System.Text.Json.Serialization;
using GsmCore.Model;

namespace GsmCore.Config;

public class GameTemplate
{
    // META
    public string DisplayName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? ImageUrl { get; set; }

    // CONTAINER
    public Platform Platform { get; set; } = Platform.WINDOWS;
    public string ContainerImage { get; set; } = null!;

    // GAME
    public string CommandLineArgs { get; set; } = null!;
    public bool HasReadableConsole { get; set; } = true;
    public bool HasWritableConsole { get; set; } = true;
    public bool HasRcon { get; set; } = true;
    public RconType RconType { get; set; } = RconType.BattlEye;

    [JsonPropertyName("ports")] public IEnumerable<GamePort> Ports { get; set; } = new List<GamePort>();

    public bool IsSteamGame { get; set; } = true;
    public int? SteamAppId { get; set; }
}