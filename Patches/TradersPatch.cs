using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace CreatureComforts;

[Injectable]
public class TradersPatch(
    ISptLogger<TradersPatch> logger,
    TradersTable tradersTable,
    TraderConfig traderConfig)
{
    private const string ModName = "CreatureComforts";
    private static readonly HashSet<string> ignoredTraders = new(StringComparer.OrdinalIgnoreCase)
    {
        "БТР", "caretaker", "Unknown", "Arena", "Storyteller"
    };
    
    public void Apply(ModConfig config)
    {
        EditTraders(config);
        EditTraderTimers(config);
    }

    private void EditTraders(ModConfig config)
    {
        foreach (var (traderId, trader) in tradersTable)
        {
            var traderBase = trader.Base;
            if (traderBase?.LoyaltyLevels == null) continue;

            string traderName = traderBase.Nickname ?? traderId;

            if (ignoredTraders.Contains(traderName)) continue;
            
            Dictionary<int, TraderTierSettings> tiers;
            
            // skip trader entirely if there is no tuning entry at all
            // if (!config.TraderTuning.TryGetValue(traderName, out var tierSettings)) continue;
            if (config.TraderTuning.TryGetValue(traderName, out var vanillaTiers))
            {
                // Base-game trader: always applied if listed, no toggle
                tiers = vanillaTiers;
            }
            else if (config.ModdedTraderTuning.TryGetValue(traderName, out var moddedEntry))
            {
                // if true, affects mod-added traders - currently only Scorpion is used
                if (!moddedEntry.Enabled) continue;
                tiers = moddedEntry.Tiers;
            }
            else
            {
                // Not configured anywhere — leave completely untouched
                continue;
            }

            ModLog.If(logger, config, ModName, $" *** DEBUG***  {traderName}({traderId})", ConsoleColor.Cyan);

            var loyaltyLevels = traderBase.LoyaltyLevels;
            
            for (int i = 0; i < loyaltyLevels.Count; i++)
            {
                var level = loyaltyLevels[i];
                int levelNum = i + 1;

                // skip specific tier if the trader's config doesn't define it
                // (e.g. a trader with only 3 loyalty levels, or a partially-configured entry)
                // if (!tierSettings.TryGetValue(levelNum, out var settings)) continue;
                if (!tiers.TryGetValue(levelNum, out var settings)) continue;
                
                level.MinSalesSum = (long)Math.Round((level.MinSalesSum ?? 0) * settings.SalesSumMultiplier);
                
                if (settings.StandingOverride.HasValue)
                {
                    level.MinStanding = settings.StandingOverride.Value;
                }
                else if (settings.StandingFactor.HasValue)
                {
                    level.MinStanding *= settings.StandingFactor.Value;
                }
                
                if (settings.LevelReduction > 0)
                {
                    level.MinLevel = Math.Max(1, (level.MinLevel ?? 1) - settings.LevelReduction);
                }

                ModLog.If(logger, config, ModName, $" *** DEBUG***  {traderName} - Tier{levelNum} is unlocked at Lv{level.MinLevel}", ConsoleColor.Blue);
                ModLog.If(logger, config, ModName, $" *** DEBUG***  {traderName} - Tier{levelNum} required minStanding is now {level.MinStanding:F2}", ConsoleColor.Yellow);
                ModLog.If(logger, config, ModName, $" *** DEBUG***  {traderName} - Tier{levelNum} required minSalesSum is now {level.MinSalesSum:N0}\n", ConsoleColor.Cyan);
            }
        }
    }

    private void EditTraderTimers(ModConfig config)
    {
        var tradersTimers = traderConfig.UpdateTime;
        var ignoredTraderTimers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "btr", "x", ""
        };
        if (tradersTimers == null) return;

        foreach (var trader in tradersTimers)
        {
            string traderName = trader.Name;

            if (ignoredTraderTimers.Contains(traderName)) continue;

            ModLog.If(logger, config, ModName, $" *** DEBUG***  {trader.Name} has a minimum refresh of {trader.Seconds.Min}s and maximum refresh of {trader.Seconds.Max}s. ({trader.Seconds.Min / 60.0:0.##} - {trader.Seconds.Max / 60.0:0.##} minutes)", ConsoleColor.Yellow);
        }
    }
}