using SPTarkov.Server.Core.Models.Spt.Tables;

namespace CreatureComforts;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class HideoutPatch(
    ISptLogger<HideoutPatch> logger,
    HideoutTable hideoutTable)
{
    private const string ModName = "CreatureComforts";

    public void Apply(ModConfig config)
    {
        var hideoutZones = hideoutTable.Areas;
        var hideoutProds = hideoutTable.Production.Recipes;

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
                    stage.ConstructionTime = Random.Shared.Next(config.HideoutBuildMinSeconds, config.HideoutBuildMaxSeconds);
                }
            }
        }

        foreach (var production in hideoutProds)
        {
            if (production.Id == "5d5c205bd582a50d042a3c0e") // bitcoin production id
            {
                production.ProductionTime = config.BitcoinProductionTimeSeconds;
            }
            else
            {
                production.ProductionTime = Random.Shared.Next(config.HideoutCraftMinSeconds, config.HideoutCraftMaxSeconds);
            }
        }

        logger.Success($"[{ModName}] Hideout construction stages randomized to be between {config.HideoutBuildMinSeconds}s and {config.HideoutBuildMaxSeconds}s ({config.HideoutBuildMinSeconds / 60.0:0.##} - {config.HideoutBuildMaxSeconds / 60.0:0.##} minutes).");
        logger.Success($"[{ModName}] Hideout production timers randomized to be between {config.HideoutCraftMinSeconds}s and {config.HideoutCraftMaxSeconds}s ({config.HideoutCraftMinSeconds / 60.0:0.##} - {config.HideoutCraftMaxSeconds / 60.0:0.##} minutes).");
    }
}