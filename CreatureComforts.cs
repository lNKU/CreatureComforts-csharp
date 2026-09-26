using SPTarkov.Server.Core.Models.Spt.Mod;

namespace CreatureComforts;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.inku.creaturecomforts";
    public string Name { get; init; } = "CreatureComforts";
    public string Author { get; init; } = "INKU";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
}

[Injectable(TypePriority = OnLoadOrder.PostLoad)]
public class CreatureComfortsLoader(
    ISptLogger<CreatureComfortsLoader> logger,
    ModConfigLoader configLoader,
    GlobalsPatch globalsPatch,
    ItemsPatch itemsPatch,
    HideoutPatch hideoutPatch,
    TradersPatch tradersPatch) : IOnLoad
{
    private const string ModName = "CreatureComforts";

    private static readonly string[] FlavorText =
    [
        "Your gear is now legally obligated to last more than one raid.",
        "Did you forget to reticulate your splines again?",
        "Currents, Loathe, and Bad Omens are also acceptable raid playlist material.",
        "Your Discord messages are shameful, you heathen.",
        "Moodring are the GOAT for modern Nu-Metal.",
        "Scav cooldown shortened. Your alt is welcome.",
        "What bosses would like As I Lay Dying? I think Birdeye would.",
        "Rimworld colonists have better morale management than the average PMC.",
        "Tesseract's polyrhythms confuse me more than the flea market's search filters.",
        "Keys now color-coded for people who can't read tooltips.",
        "Say hi on Discord! I like talking to new people, sometimes.",
        "I'm still not maxxed in Old School RuneScape. Shameful, right?",
        "Periphery and Volumes... because djent enjoyers deserve representation too.",
        "Your stock now folds as easily as your confidence.",
        "You like good music? throatcut, Ocean Grove, and Northlane. You're welcome.",
        "Trader loyalty made slightly less soul-crushing.",
        "Weapons no longer refuse to fold out of spite.",
        "I brake for emo girls. You should too.",
        "Spiritbox, amirite?"
    ];

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        configLoader.LoadConfig();
        var config = configLoader.Config;

        globalsPatch.Apply(config);
        itemsPatch.Apply(config);
        hideoutPatch.Apply(config);
        tradersPatch.Apply(config);

        string flavor = FlavorText[Random.Shared.Next(FlavorText.Length)];
        logger.Success($"{ModName} loaded! {flavor}");

        return Task.CompletedTask;
    }
}