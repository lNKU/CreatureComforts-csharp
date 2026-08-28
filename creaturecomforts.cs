using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace CreatureComforts;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.inku.creaturecomforts";
    public string Name { get; init; } = "CreatureComforts";
    public string Author { get; init; } = "INKU";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
}

public class ModConfig
{
    public bool EnableLogging { get; set; } = true;
    public int FleaMarketMinLevel { get; set; } = 25;
    public int ScavCooldownMinSeconds { get; set; } = 300;
    public int ScavCooldownMaxSeconds { get; set; } = 900;
    public int HideoutBuildMinSeconds { get; set; } = 900;
    public int HideoutBuildMaxSeconds { get; set; } = 900;
    public int HideoutCraftMinSeconds { get; set; } = 15;
    public int HideoutCraftMaxSeconds { get; set; } = 120;
    public int MetalFuelTankResource { get; set; } = 220;
    public int ExpeditionaryFuelTankResource { get; set; } = 100;
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class EditDatabaseValues(
    ISptLogger<EditDatabaseValues> logger,
    GlobalTable globalTable,
    TemplateTable templateTable,
    HideoutTable hideoutTable)
    : IOnLoad
{
    private const string ModName = "CreatureComforts";
    private const string SilencerParentId = "550aa4cd4bdc2dd8348b456c";
    private const string AmmoParentId = "5485a8684bdc2da71d8b4567";
    private const string FuelParentId = "5d650c3e815116009f6201d2";

    private ModConfig _config = new();

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        LoadConfig();

        EditGlobals();
        EditItems();
        EditHideout();

        logger.Success($"{ModName} loaded!");
        return Task.CompletedTask;
    }

    private void LoadConfig()
    {
        string modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string configDir = Path.Combine(modFolder, "config");
        string configPath = Path.Combine(configDir, "config.json");
        
        string[] pathSegments = configPath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        string displayPath = pathSegments.Length >= 4 
            ? $"./{string.Join('/', pathSegments.TakeLast(4))}" 
            : configPath;

        if (!File.Exists(configPath))
        {
            _config = new ModConfig();
            string defaultJson = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, defaultJson);
            logger.Warning($"[{ModName}]: Created default config in {displayPath}.");
            return;
        }

        try
        {
            string json = File.ReadAllText(configPath);
            _config = JsonSerializer.Deserialize<ModConfig>(json) ?? new ModConfig();
            logger.Info($"[{ModName}]: Successfully loaded config.");
        }
        catch (Exception ex)
        {
            logger.Error($"[{ModName}]: Failed to read config file. Using default settings. Exception: {ex.Message}");
            _config = new ModConfig();
        }
    }

    private void Log(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        if (_config.EnableLogging)
        {
            logger.LogWithColor($"[{ModName}]: {message}", color);
        }
    }

    private void EditGlobals()
    {
        var globals = globalTable.Configuration;
        var globalsXP = globals.Exp.Level.ExperienceTable;
        var globalsStamina = globals.Stamina;
        var ragfair = globals.RagFair;

        globals.SavagePlayCooldown = Random.Shared.Next(_config.ScavCooldownMinSeconds, _config.ScavCooldownMaxSeconds);
        ragfair.MinUserLevel = _config.FleaMarketMinLevel;

        // Scale experience required for new levels
        for (int i = 0; i < globalsXP.Length; i++)
        {
            var expLvl = globalsXP[i];
            var reqExp = expLvl.Experience;
            int level = i + 1;

            double multiplier = level switch
            {
                <= 10 => 1.45, // if below level 10, increase exp per level by 45%
                <= 70 => 1.55, // if between levels 10 and 70, increase exp per level by 55%
                71 => 1.35, // if over level 70, increase exp per level by 35%
                _ => 1.25
            };

            expLvl.Experience = (int)Math.Ceiling(reqExp * multiplier);
        }

        // Halve Energy and Hydration drain
        globals.Health.Effects.Existence.HydrationDamage /= 2f;
        globals.Health.Effects.Existence.EnergyDamage /= 2f;
        
        // Increase weight limits slightly
        globalsStamina.BaseOverweightLimits = globalsStamina.BaseOverweightLimits with
        {
            X = globalsStamina.BaseOverweightLimits.X * 1.55f,
            Y = globalsStamina.BaseOverweightLimits.Y * 1.175f
        };

        globalsStamina.SprintOverweightLimits = globalsStamina.SprintOverweightLimits with
        {
            X = globalsStamina.SprintOverweightLimits.X * 1.725f,
            Y = globalsStamina.SprintOverweightLimits.Y * 1.175f
        };

        globalsStamina.WalkOverweightLimits = globalsStamina.WalkOverweightLimits with
        {
            X = globalsStamina.WalkOverweightLimits.X * 1.55f,
            Y = globalsStamina.WalkOverweightLimits.Y * 1.35f
        };

        globalsStamina.WalkSpeedOverweightLimits = globalsStamina.WalkSpeedOverweightLimits with
        {
            X = globalsStamina.WalkSpeedOverweightLimits.X * 1.55f,
            Y = globalsStamina.WalkSpeedOverweightLimits.Y * 1.2f
        };

        Log($"BaseOverweightLimits -> {globalsStamina.BaseOverweightLimits.X}kg / {globalsStamina.BaseOverweightLimits.Y}kg", ConsoleColor.Cyan);
        Log($"SprintOverweightLimits -> {globalsStamina.SprintOverweightLimits.X}kg / {globalsStamina.SprintOverweightLimits.Y}kg", ConsoleColor.Yellow);
    }

    private void EditItems()
    {
        var items = templateTable.Items;
        int modifiedDurabilityCount = 0;

        foreach (var item in items.Values)
        {
            // Remove folding blocks for weapons
            if (item.Properties == null) continue;

            if (item.Properties.BlocksFolding == true)
            {
                item.Properties.BlocksFolding = false;
            }

            // Remove durability burn for suppressors & ammo
            if (item.Parent == SilencerParentId || item.Parent == AmmoParentId)
            {
                item.Properties.DurabilityBurnModificator = 1f;
                modifiedDurabilityCount++;
            }

            // Adjust fuel resource values
            if (item.Parent == FuelParentId)
            {
                if (item.Id == "5d1b36a186f7742523398433") // Metal Fuel Tank
                {
                    item.Properties.Resource = _config.MetalFuelTankResource;
                    item.Properties.MaxResource = _config.MetalFuelTankResource;
                }
                else if (item.Id == "5d1b371186f774253763a656") // Expeditionary Fuel Tank
                {
                    item.Properties.Resource = _config.ExpeditionaryFuelTankResource;
                    item.Properties.MaxResource = _config.ExpeditionaryFuelTankResource;
                }
            }
        }

        logger.LogWithColor($"Removed durability burn modifier from {modifiedDurabilityCount} silencers & ammo items.", ConsoleColor.Green);
    }

    private void EditHideout()
    {
        var hideoutZones = hideoutTable.Areas;
        var hideoutProds = hideoutTable.Production.Recipes;

        // Modify Hideout Area build times
        foreach (var zone in hideoutZones)
        {
            if (zone.Stages == null) continue;

            foreach (var stage in zone.Stages.Values)
            {
                stage.ConstructionTime = Random.Shared.Next(_config.HideoutBuildMinSeconds, _config.HideoutBuildMaxSeconds);
            }
        }

        // Modify Hideout Area craft times
        foreach (var production in hideoutProds)
        {
            if (production.Id == "5d5c205bd582a50d042a3c0e")
            {
                production.ProductionTime = 12600; // Bitcoin set to 3.5hrs
            }
            else
            {
                production.ProductionTime = Random.Shared.Next(_config.HideoutCraftMinSeconds, _config.HideoutCraftMaxSeconds);
            }
        }

        Log("Hideout construction and production timers randomized.", ConsoleColor.Cyan);
    }
}