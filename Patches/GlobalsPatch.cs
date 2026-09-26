using SPTarkov.Server.Core.Models.Spt.Tables;

namespace CreatureComforts;

[Injectable]
public class GlobalsPatch(
    ISptLogger<GlobalsPatch> logger,
    GlobalTable globalTable)
{
    private const string ModName = "CreatureComforts";
    
    private static double GetXpMultiplier(int level, List<XpBracket> sortedBrackets)
    {
        foreach (var bracket in sortedBrackets)
        {
            if (level <= bracket.MaxLevel)
            {
                return bracket.Multiplier;
            }
        }

        return 1.0;
    }

    public void Apply(ModConfig config)
    {
        var globals = globalTable.Configuration;
        var globalsXP = globals.Exp.Level.ExperienceTable;
        var globalsStamina = globals.Stamina;
        var ragfair = globals.RagFair;
        var overweight = config.OverweightFactor;

        globals.SavagePlayCooldown = Random.Shared.Next(config.ScavCooldownMinSeconds, config.ScavCooldownMaxSeconds);
        ragfair.MinUserLevel = config.FleaMarketMinLevel;

        long totalExp = 0;
        var sortedBrackets = config.ExperienceMultiplier.OrderBy(b => b.MaxLevel).ToList();

        for (int i = 0; i < globalsXP.Length; i++)
        {
            var expLvl = globalsXP[i];
            var reqExp = expLvl.Experience;
            int level = i + 1;

            double multiplier = GetXpMultiplier(level, sortedBrackets);

            expLvl.Experience = (int)Math.Ceiling(reqExp * multiplier);
            totalExp += expLvl.Experience;
            ModLog.If(logger, config, ModName, $" *** DEBUG***  Lv.{level} - Base Required EXP: {reqExp:N0}", ConsoleColor.Cyan);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  Lv.{level} - Modified Required EXP: {expLvl.Experience:N0}", ConsoleColor.Yellow);
            ModLog.If(logger, config, ModName, $" *** DEBUG***  Lv.{level} - Total Required EXP: {totalExp:N0}\n", ConsoleColor.Yellow);
        }
        
        globals.Health.Effects.Existence.HydrationDamage *= (float)config.HydrationDrainFactor;
        globals.Health.Effects.Existence.EnergyDamage *= (float)config.EnergyDrainFactor;
        
        globalsStamina.BaseOverweightLimits = globalsStamina.BaseOverweightLimits with
        {
            X = globalsStamina.BaseOverweightLimits.X * (float)overweight.BaseX,
            Y = globalsStamina.BaseOverweightLimits.Y * (float)overweight.BaseY
        };

        globalsStamina.SprintOverweightLimits = globalsStamina.SprintOverweightLimits with
        {
            X = globalsStamina.SprintOverweightLimits.X * (float)overweight.SprintX,
            Y = globalsStamina.SprintOverweightLimits.Y * (float)overweight.SprintY
        };

        globalsStamina.WalkOverweightLimits = globalsStamina.WalkOverweightLimits with
        {
            X = globalsStamina.WalkOverweightLimits.X * (float)overweight.WalkX,
            Y = globalsStamina.WalkOverweightLimits.Y * (float)overweight.WalkY
        };

        globalsStamina.WalkSpeedOverweightLimits = globalsStamina.WalkSpeedOverweightLimits with
        {
            X = globalsStamina.WalkSpeedOverweightLimits.X * (float)overweight.WalkSpeedX,
            Y = globalsStamina.WalkSpeedOverweightLimits.Y * (float)overweight.WalkSpeedY
        };
        
        ModLog.If(logger, config, ModName, $" *** DEBUG***  BaseOverweightLimits -> {globalsStamina.BaseOverweightLimits.X}kg / {globalsStamina.BaseOverweightLimits.Y}kg", ConsoleColor.Cyan);
        ModLog.If(logger, config, ModName, $" *** DEBUG***  SprintOverweightLimits -> {globalsStamina.SprintOverweightLimits.X}kg / {globalsStamina.SprintOverweightLimits.Y}kg\n", ConsoleColor.Yellow);
    }
}