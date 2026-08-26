using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using Vector3 = SPTarkov.Server.Core.Models.Eft.Common.Vector3;

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

// We want to load after PostDBModLoader is complete, so we set our type priority to that, plus 1.
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class EditDatabaseValues(
    ISptLogger<EditDatabaseValues> logger,
    GlobalTable globalTable,
    TemplateTable templateTable,
    HideoutConfig hideoutConfig,
    HideoutTable hideoutTable) // We are injecting a logger similar to example 1, but notice the class inside < > is different
    : IOnLoad // Implement the `IOnLoad` interface so that this mod can do something
{

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        // When SPT starts, it stores all the data found in (SPT_Data\Server\database) in memory
        // We can use the 'databaseService' we injected to access this data, this includes files from EFT and SPT

        // Lets edit some globals settings to make the game easier
        EditGlobals();

        // Let's edit some items for storage space
        EditItems();
        
        // Let's edit the hideout so it's easier to upgrade the lavatory
        EditHideout();

        // lets write a nice log message to the server console so players know our mod has made changes
        logger.Success("Finished Editing Database!");
        
        // Inform server we have finished
        return Task.CompletedTask;
    }
    
    private void EditGlobals()
    {
        var globals = globalTable.Configuration;
        var globalsXP = globalTable.Configuration.Exp.Level.ExperienceTable;
        var globalsStamina = globals.Stamina;
        var ragfair = globalTable.Configuration.RagFair;
        Random rnd = new Random(); //instance new random class for time-based things
        
        // Let's edit the scav cooldown
        globals.SavagePlayCooldown = rnd.Next(300, 900); 
        
        // the level to access flea
        ragfair.MinUserLevel = 25;
        
        for (int i = 0; i < globalsXP.Length; i++)
        {
            var expLvl = globalsXP[i]; //get levels from list
            var reqExp = expLvl.Experience; //get unmodified experience to level up
            int level = i + 1;

            double multiplier = level switch
            {
                <= 10 => 1.45, //if below level 10, base exp required to be increased by 145%
                <= 70 => 1.55, //if between level 10 and 70, base exp required to be increased by 155%
                71 => 1.35, //if over level 70, base exp required to be increased by 135%
                _ => 1.25
            };

            expLvl.Experience = (int)Math.Ceiling(reqExp * multiplier);
            
            logger.LogWithColor($"[CreatureComforts]: Lv{level} - Original Required EXP: {reqExp}", ConsoleColor.Gray);
            logger.LogWithColor($"[CreatureComforts]: Lv{level} - Modified Required EXP: {expLvl.Experience}\n", ConsoleColor.DarkYellow);
        }
        
        // Increase Carry Weight limits
        var baseOverweightLimit = globalsStamina.BaseOverweightLimits;
        var sprintOverweightLimit = globalsStamina.SprintOverweightLimits;
        var walkOverweightLimit = globalsStamina.WalkOverweightLimits;
        var walkSpeedOverweightLimit = globalsStamina.WalkSpeedOverweightLimits;
        
        // Adjust Energy and Hydration Drain
        globals.Health.Effects.Existence.EnergyDamage /= 2f;
        globals.Health.Effects.Existence.EnergyDamage /= 2f;
        
        globalsStamina.BaseOverweightLimits = new Vector3
        {
            X = baseOverweightLimit.X * 1.55f, //1.92f,
            Y = baseOverweightLimit.Y * 1.175f, //1.375f,
            Z = baseOverweightLimit.Z
        };
        
        globalsStamina.SprintOverweightLimits = new Vector3
        {
            X = sprintOverweightLimit.X * 1.725f, //2.175f,
            Y = sprintOverweightLimit.Y * 1.175f, //1.375f,
            Z = sprintOverweightLimit.Z
        };
        
        globalsStamina.WalkOverweightLimits = new Vector3
        {
            X = walkOverweightLimit.X * 1.55f, //1.77f,
            Y = walkOverweightLimit.Y * 1.35f, //1.57f,
            Z = walkOverweightLimit.Z
        };
        
        globalsStamina.WalkSpeedOverweightLimits = new Vector3
        {
            X = walkSpeedOverweightLimit.X * 1.55f, //1.775f,
            Y = walkSpeedOverweightLimit.Y * 1.2f, //1.475f,
            Z = walkSpeedOverweightLimit.Z
        };
        
        logger.LogWithColor($"[CreatureComforts]: ### DEBUG ### BaseOverweightLimits are now ${globalsStamina.BaseOverweightLimits.X}kg and ${globalsStamina.BaseOverweightLimits.Y}kg.", ConsoleColor.Cyan);
        logger.LogWithColor($"[CreatureComforts]: ### DEBUG ### SprintOverweightLimits are now ${globalsStamina.SprintOverweightLimits.X}kg and ${globalsStamina.SprintOverweightLimits.Y}kg.", ConsoleColor.Yellow);
        logger.LogWithColor($"[CreatureComforts]: ### DEBUG ### WalkSpeedOverweightLimits are now ${globalsStamina.WalkSpeedOverweightLimits.X}kg and ${globalsStamina.WalkSpeedOverweightLimits.Y}kg.", ConsoleColor.Cyan);
        logger.LogWithColor($"[CreatureComforts]: ### DEBUG ### WalkOverweightLimits are now ${globalsStamina.WalkOverweightLimits.X}kg and ${globalsStamina.WalkOverweightLimits.Y}kg.\n", ConsoleColor.Yellow);
    }
        
    private void EditItems()
    {
        var items = templateTable.Items;

        foreach (var item in items.Values)
        {
            if (item.Properties?.BlocksFolding == true)
            {
                item.Properties.BlocksFolding = false;
            }
        }
    }

    private void EditHideout()
    {
        // We want the areas, they're stored in a list

        var hideoutZones = hideoutTable.Areas;
        var hideoutProds = hideoutTable.Production;
        Random rnd = new Random(); //instance new random class for time-based things
        
        hideoutConfig.OverrideBuildTimeSeconds = rnd.Next(300, 1500);

        // We find the toilet, we use 'firstOrDefault', if we cant find the watercloset, 'waterclosetArea' will be null
        // var waterclosetArea = hideoutZones.FirstOrDefault(area => area.Type == HideoutAreas.WaterCloset);


        // Now we have the toilet, we can find the requirements to craft, all data is stored by stage
        // var toiletStages = waterclosetArea.Stages;

        // Stages are stored in a dictionary, a dictionary has a 'key' and a 'value'
        // In this case, the 'key' is the upgrade stage, e.g. "1", or "2"
        // We reference to each stage as a 'stageKvP' this means 'Key value Pair', every key has a value (key = stage number, value = data for that stage)
        // foreach (var (stageKey, stageValue) in toiletStages)
        {
            // while we're here, we can make the stages craft really fast (60 seconds)
            // stageValue.ConstructionTime = 60;

            // Let's get the stage requirements, they're a list
            // var stageRequirements = stageValue.Requirements;

            // We empty the requirements out, now it can be built straight away
            // stageRequirements.Clear();
        }
    }
}