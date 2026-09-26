using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace CreatureComforts;

[Injectable]
public class ItemsPatch(
    ISptLogger<ItemsPatch> logger,
    TemplateTable templateTable)
{
    private const string ModName = "CreatureComforts";
    private const string FuelParentId = "5d650c3e815116009f6201d2";
    private const string MechanicalKeyParentId = "5c99f98d86f7745c314214b3";
    private const string SilencerParentId = "550aa4cd4bdc2dd8348b456c";
    private const string KeycardParentId = "5c164d2286f774194c5e69fa";
    private const string AssaultScopeParentId = "55818add4bdc2d5b648b456f";
    private const string ReflexSightParentId = "55818ad54bdc2ddc698b4569";
    private const string CompactReflexSightParentId = "55818acf4bdc2dde698b456b";
    private const string ScopeSightParentId = "55818ae44bdc2dde698b456c";
    private const string SpecialSightParentId = "55818aeb4bdc2ddc698b456a";

    private static bool TrySetColor(TemplateItem item, HashSet<string> set, string color, ref int counter)
    {
        if (!set.Contains(item.Id)) return false;
        item.Properties!.BackgroundColor = color;
        counter++;
        return true;
    }

    public void Apply(ModConfig config)
    {
        var items = templateTable.Items;
        int modifiedDurabilityCount = 0;
        int markedKeysCount = 0;
        int valuableKeysCount = 0;
        int midKeysCount = 0;
        int mehhKeysCount = 0;
        int questKeysCount = 0;
        int defaultKeysCount = 0;

        var questKeysSet = new HashSet<string>(config.QuestKeys ?? []);
        var mehhKeysSet = new HashSet<string>(config.MehhKeys ?? []);
        var midKeysSet = new HashSet<string>(config.MidKeys ?? []);
        var valuableKeysSet = new HashSet<string>(config.ValuableKeys ?? []);
        var markedKeysSet = new HashSet<string>(config.MarkedKeys ?? []);

        foreach (var item in items.Values)
        {
            bool durabilityModified = false;

            if (item.Properties == null) continue;

            item.Properties.BlocksFolding = false;

            if (item.Properties.DurabilityBurnModificator is > 1)
            {
                item.Properties.DurabilityBurnModificator = 1f;
                durabilityModified = true;
            }

            if (item.Properties.DurabilityBurnRatio is > 1)
            {
                item.Properties.DurabilityBurnRatio = 1f;
                durabilityModified = true;
            }

            if (durabilityModified)
            {
                modifiedDurabilityCount++;
            }

            if (item.Id == "5751a25924597722c463c472")
            {
                if (item.Properties.EffectsDamage != null)
                {
                    item.Properties.EffectsDamage[DamageEffectType.HeavyBleeding] = new EffectsDamageProperties
                    {
                        Cost = 2,
                        Delay = 0,
                        Duration = 0,
                        FadeOut = 0
                    };
                }
            }

            if (item.Parent == AssaultScopeParentId || item.Parent == ReflexSightParentId ||
                item.Parent == CompactReflexSightParentId || item.Parent == ScopeSightParentId ||
                item.Parent == SpecialSightParentId)
            {
                if (item.Properties.Ergonomics.HasValue && item.Properties.Ergonomics.Value < -3)
                {
                    item.Properties.Ergonomics = Math.Truncate(item.Properties.Ergonomics.Value / 2.0);
                }
            }

            if (item.Parent == SilencerParentId && item.Properties.Ergonomics.HasValue)
            {
                item.Properties.Ergonomics = Math.Truncate(item.Properties.Ergonomics.Value / 2.0);
            }

            if (config.EnableKeyColorChanges)
            {
                bool isKeyByParent = item.Parent == MechanicalKeyParentId || item.Parent == KeycardParentId;

                bool matched =
                    TrySetColor(item, questKeysSet, "red", ref questKeysCount) ||
                    TrySetColor(item, markedKeysSet, "yellow", ref markedKeysCount) ||
                    TrySetColor(item, valuableKeysSet, "violet", ref valuableKeysCount) ||
                    TrySetColor(item, midKeysSet, "blue", ref midKeysCount) ||
                    TrySetColor(item, mehhKeysSet, "green", ref mehhKeysCount);

                if (!matched && isKeyByParent)
                {
                    item.Properties.BackgroundColor = "grey";
                    defaultKeysCount++;
                }
            }

            if (item.Parent == FuelParentId)
            {
                if (item.Id == "5d1b36a186f7742523398433")
                {
                    item.Properties.Resource = config.MetalFuelTankResource;
                    item.Properties.MaxResource = config.MetalFuelTankResource;
                }
                else if (item.Id == "5d1b371186f774253763a656")
                {
                    item.Properties.Resource = config.ExpeditionaryFuelTankResource;
                    item.Properties.MaxResource = config.ExpeditionaryFuelTankResource;
                }
            }
        }

        logger.LogWithColor($"[{ModName}] Removed negative durability burn modifier from {modifiedDurabilityCount} items.", ConsoleColor.Green);

        if (config.EnableKeyColorChanges)
        {
            int totalKeysCount = markedKeysCount + valuableKeysCount + midKeysCount + mehhKeysCount + defaultKeysCount;

            ModLog.If(logger, config, ModName, " *** DEBUG***  Key Color Breakdown:", ConsoleColor.Cyan);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  - Quest (Red): {questKeysCount}", ConsoleColor.DarkRed);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  - Marked (Yellow): {markedKeysCount}", ConsoleColor.Yellow);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  - Valuable (Violet): {valuableKeysCount}", ConsoleColor.Magenta);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  - Mid Value (Blue): {midKeysCount}", ConsoleColor.Blue);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  - Low Value (Green): {mehhKeysCount}", ConsoleColor.Green);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  - Other (Grey): {defaultKeysCount}\n", ConsoleColor.White);
            logger.LogWithColor($"[{ModName}] Updated background colors for {totalKeysCount} keys and keycards.", ConsoleColor.Green);
        }
    }
}