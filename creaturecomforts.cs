using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    public bool RefreshConfig { get; set; } = false;
    public bool EnableKeyColorChanges { get; set; } = true;
    public int FleaMarketMinLevel { get; set; } = 25;
    public int ScavCooldownMinSeconds { get; set; } = 300;
    public int ScavCooldownMaxSeconds { get; set; } = 900;
    public int HideoutBuildMinSeconds { get; set; } = 300;
    public int HideoutBuildMaxSeconds { get; set; } = 900;
    public int HideoutCraftMinSeconds { get; set; } = 15;
    public int HideoutCraftMaxSeconds { get; set; } = 120;
    public int MetalFuelTankResource { get; set; } = 220;
    public int ExpeditionaryFuelTankResource { get; set; } = 100;
    public List<string> QuestKeys { get; set; } =
    [
        ""
    ];
    public List<string> MarkedKeys { get; set; } =
    [
        "5780cf7f2459777de4559322", "5d80c62a86f7744036212b3f", "5d80c60f86f77440373c4ece", "62987dfc402c7f69bf010923",
        "64ccc25f95763a1ae376e447", "63a3a93f8a56922e82001f5d", "64d4b23dc1b37504b41ac2b6"
    ];
    public List<string> ValuableKeys { get; set; } =
    [
        "5448ba0b4bdc2d02308b456c", "5780d0532459777a5108b9a2", "5913877a86f774432f15d444", "5780d0652459777df90dcb74", "591383f186f7744a4c5edcf3", 
        "591382d986f774465a6413a7", "59136e1e86f77432f15d133", "59387a4986f77401cc236e62", "5672c92d4bdc2d180f8b4567", "59148c8a86f774197930e983", 
        "5780cf942459777df90dcb72", "5780cfa52459777dfb276eb1", "5ad5d64486f774079b080af8", "5e42c71586f7747f245e1343", "5ad5cfbd86f7742c825d6104", 
        "5addaffe86f77470b455f900", "5ad5d7d286f77450166e0a89", "5e42c81886f7742a01529f57", "5e42c83786f7742a021fdf3c", "5ad5db3786f7743568421cce", 
        "5c1d0f4986f7744bb01837fa", "5c1d0efb86f7744baf2e7b7b", "5c1d0c5f86f7744bb2683cf0", "5c1d0dc586f7744baf2e7b79", "5c1e495a86f7743109743dfb", 
        "5c1d0d6d86f7744bb2683e1f", "5c1e2a1e86f77431ea0ea84c", "5c1e2d1f86f77431e9280bee", "5c1f79a086f7746ed066fb8f", "5d947d4e86f774447b415895", 
        "5d947d3886f774447b415893", "5d8e0e0e86f774321140eb56", "5d80cb3886f77440556dbf09", "5d95d6fa86f77424484aa5e9", "5d80cb5686f77440545d1286", 
        "5d95d6be86f77424444eb3a7", "5d80c6c586f77440351beef1", "5d80ccac86f77470841ff452", "5d80ccdd86f77474f7575e02", "5d80cd1a86f77402aa362f42", 
        "5d80c66d86f774405611c7d6", "5d80c6fc86f774403a401e3c", "5d80c88d86f77440556dbf07", "61aa5b7db225ac1ead7957c1", "61aa5ba8018e9821b7368da9", 
        "61aa5b518f5e7a39b41416e2", "61a6444b8c141d68246e2d2f", "62987da96188c076bc0d8c51", "62987c658081af308d7558c6", "5a0f08bc86f77478f33b84c2", 
        "5d8e15b686f774445103b190", "5a0eb6ac86f7743124037a28", "5a0f068686f7745b0d4ea242", "5a0f0f5886f7741c4e32a472", "5a0dc45586f7742f6b0b73e3", 
        "5a0dc95c86f77452440fc675", "5a144dfd86f77445cb5a0982", "5a0ec6d286f7742c0b518fb5", "5a0ee30786f774023b6ee08f", "5a0ee34586f774023b6ee092", 
        "5a13eebd86f7746fd639aa93", "5a0ee37f86f774023657a86f", "5a1452ee86f7746f33111763", "5a13ef7e86f7741290491063", "5a13f46386f7741dd7384b04", 
        "5a0eff2986f7741fd654e684", "5a0ea64786f7741707720468", "5eff09cd30a7dc22fd1ddfed", "5a144bdb86f7741d374bbde0", "5a0ee4b586f7743698200d22", 
        "5a13f24186f77410e57c5626", "5a13f35286f77413ef1436b0", "5a145d4786f7744cbb6f4a12", "5a145d7b86f7744cbb6f4a13", "5a0eec9686f77402ac5c39f2", 
        "5a0eee1486f77402aa773226", "5a0ea79b86f7741d4a35298e", "63a39c7964283b5e9c56b280", "64ccc1ec1779ad6ba200a137", "63a71e781031ac76fe773c7d", 
        "64ccc1d4a0f13c24561edf27", "64ccc1f4ff54fb38131acf27", "63a71e922b25f7513905ca20", "63a71e86b7f4570d3a293169", "63a39fc0af870e651d58e6ae", 
        "63a39fd1c9b3aa4b61683efb", "63a39f6e64283b5e9c56b289", "63a39667c9b3aa4b61683e98", "63a71ed21031ac76fe773c7f", "64ccc246ff54fb38131acf29", 
        "6582dbe43a2e5248357dbe9a", "6582dc4b6ba9e979af6b79f4", "6582dbf0b8d7830efc45016f", "6582dc5740562727a654ebb1", "64ccc24de61ea448b507d34d", 
        "64ccc206793ca11c8f450a38", "64ccc1fe088064307e14a6f7", "63a39f08cd6db0635c197600", "63a399193901f439517cafb6", "63a397d3af870e651d58e65b", 
        "64ccc2111779ad6ba200a139", "5c94bbff86f7747ee735c08f", "591afe0186f77431bd616a11", "6761a6ccd9bbb27ad703c48a", "68e95f4fa4a577e907015787",
        "59136e1e86f774432f15d133"
    ];
    public List<string> MehhKeys { get; set; } =
    [
        "658199972dc4e60f6d556a2f", "6581998038c79576a2569e11", "5d08d21286f774736e7c94c3", "5da743f586f7744014504f72", "5913611c86f77479e0084092"
    ];
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class EditDatabaseValues(
    ISptLogger<EditDatabaseValues> logger,
    GlobalTable globalTable,
    TemplateTable templateTable,
    BotTable botTable,
    HideoutTable hideoutTable)
    : IOnLoad
{
    private const string ModName = "CreatureComforts";
    private const string SilencerParentId = "550aa4cd4bdc2dd8348b456c";
    private const string AmmoParentId = "5485a8684bdc2da71d8b4567";
    private const string FuelParentId = "5d650c3e815116009f6201d2";
    private const string MechanicalKeyParentId = "5c99f98d86f7745c314214b3";
    private const string KeycardParentId = "5c164d2286f774194c5e69fa";

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
        
        Directory.CreateDirectory(configDir);
        var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
        
        string[] pathSegments = configPath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        string displayPath = pathSegments.Length >= 4 
            ? $"./{string.Join('/', pathSegments.TakeLast(4))}" 
            : configPath;

        if (!File.Exists(configPath))
        {
            _config = new ModConfig();
            string defaultJson = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, defaultJson);
            logger.Warning($"[{ModName}] Created default config in {displayPath}.");
            return;
        }

        try
        {
            string existingJson = File.ReadAllText(configPath);
            _config = JsonSerializer.Deserialize<ModConfig>(existingJson) ?? new ModConfig();
            
            
            // compare the existing config file and the current defined _config
            var existingConfigNode = JsonNode.Parse(existingJson)?.AsObject();
            var updatedConfigNode = JsonSerializer.SerializeToNode(_config)?.AsObject();
            
            if (existingConfigNode != null && updatedConfigNode != null)
            {
                // compare the updated ModConfig and identify keys that don't exist in current config file
                List<string> addedKeys = updatedConfigNode
                    .Where(kvp => !existingConfigNode.Any(e => string.Equals(e.Key, kvp.Key, StringComparison.OrdinalIgnoreCase)))
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (addedKeys.Count > 0)
                {
                    // update the config file
                    string updatedJson = JsonSerializer.Serialize(_config, serializeOptions);
                    File.WriteAllText(configPath, updatedJson);

                    // list newly added keys
                    string keyList = string.Join(", ", addedKeys);
                    logger.Warning($"[{ModName}] Config updated with new properties: {keyList}");
                }
                else
                {
                    logger.Success($"[{ModName}] Successfully loaded config.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[{ModName}] Failed to read config file.\nNON-FATAL ERROR: {ex.Message}");
            logger.Warning($"[{ModName}] Using default settings for this session.\n");
            
            _config = new ModConfig();

            if (_config.RefreshConfig)
            {
                logger.Warning($"[{ModName}] Recreating config file at {displayPath}.\n");
                
                string defaultJson = JsonSerializer.Serialize(_config, serializeOptions);
                File.WriteAllText(configPath, defaultJson);
            }
        }
    }
    
    private void Log(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        if (_config.EnableLogging)
        {
            logger.LogWithColor($"[{ModName}] {message}", color);
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

        // scale experience required for new levels
        long totalExp = 0;
        
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
            totalExp += expLvl.Experience;
            Log($"Lv.{level} - Base Required EXP: {reqExp:N0}", ConsoleColor.Cyan);
            Log($"Lv.{level} - Modified Required EXP: {expLvl.Experience:N0}", ConsoleColor.Yellow);
            Log($"Lv.{level} - Total Required EXP: {totalExp:N0}\n", ConsoleColor.Yellow);
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
        Log($"SprintOverweightLimits -> {globalsStamina.SprintOverweightLimits.X}kg / {globalsStamina.SprintOverweightLimits.Y}kg\n", ConsoleColor.Yellow);
    }

    private void EditItems()
    {
        var items = templateTable.Items;
        int modifiedDurabilityCount = 0;
        int markedKeysCount = 0;
        int valuableKeysCount = 0;
        int mehhKeysCount = 0;
        int questKeysCount = 0;
        int defaultKeysCount = 0;
        
        var questKeysSet = new HashSet<string>(_config.QuestKeys ?? []);
        var mehhKeysSet = new HashSet<string>(_config.MehhKeys ?? []);
        var valuableKeysSet = new HashSet<string>(_config.ValuableKeys ?? []);
        var markedKeysSet = new HashSet<string>(_config.MarkedKeys ?? []);


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
            
            // Key & Keycard recoloring logic
            if (_config.EnableKeyColorChanges)
            {
                bool isKeyByParent = string.Equals(item.Parent, MechanicalKeyParentId, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(item.Parent, KeycardParentId, StringComparison.OrdinalIgnoreCase);

                if (questKeysSet.Contains(item.Id))
                {
                    item.Properties.BackgroundColor = "red"; // Quest-related keys/keycards
                    questKeysCount++;
                }
                else if (markedKeysSet.Contains(item.Id))
                {
                    item.Properties.BackgroundColor = "yellow"; // Extremely High-value keys/keycards
                    markedKeysCount++;
                }
                else if (valuableKeysSet.Contains(item.Id))
                {
                    item.Properties.BackgroundColor = "violet"; // High-value keys/keycards
                    valuableKeysCount++;
                }
                else if (mehhKeysSet.Contains(item.Id))
                {
                    item.Properties.BackgroundColor = "green"; // Low/Mid-value keys/keycards
                    mehhKeysCount++;
                }
                else if (isKeyByParent)
                {
                    item.Properties.BackgroundColor = "black"; // All other keys
                    defaultKeysCount++;
                }
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

        logger.LogWithColor($"[{ModName}] Removed durability burn modifier from {modifiedDurabilityCount} silencers & ammo items.", ConsoleColor.Green);
        if (_config.EnableKeyColorChanges)
        {
            int totalKeysCount = markedKeysCount + valuableKeysCount + mehhKeysCount + defaultKeysCount;
            
            Log("Key Color Breakdown:", ConsoleColor.Cyan);
            Log($"  - Quest (Red): {questKeysCount}", ConsoleColor.DarkRed);
            Log($"  - Marked (Yellow): {markedKeysCount}", ConsoleColor.Yellow);
            Log($"  - Valuable (Violet): {valuableKeysCount}", ConsoleColor.Magenta);
            Log($"  - Low Value (Green): {mehhKeysCount}", ConsoleColor.Green);
            Log($"  - Useless (Grey): {defaultKeysCount}", ConsoleColor.White);
            logger.LogWithColor($"[{ModName}] Updated background colors for {totalKeysCount} keys and keycards.", ConsoleColor.Green);
        }
    }
    
    private void EditHideout()
    {
        var hideoutZones = hideoutTable.Areas;
        var hideoutProds = hideoutTable.Production.Recipes;

        // Modify Hideout Area build times
        foreach (var zone in hideoutZones)
        {
            if (zone.Stages == null) continue;

            foreach (var (key, stage) in zone.Stages)
            {
                if (int.TryParse(key, out int level) && level <= 1)
                {
                    stage.ConstructionTime = 0;
                }
                else
                {
                    stage.ConstructionTime = Random.Shared.Next(_config.HideoutBuildMinSeconds, _config.HideoutBuildMaxSeconds);
                }
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

        logger.Success($"[{ModName}] Hideout construction stages randomized to be between {_config.HideoutBuildMinSeconds} and {_config.HideoutBuildMaxSeconds} seconds.");
        logger.Success($"[{ModName}] Hideout production timers randomized to be between {_config.HideoutCraftMinSeconds} and {_config.HideoutCraftMaxSeconds} seconds.");

    }
}