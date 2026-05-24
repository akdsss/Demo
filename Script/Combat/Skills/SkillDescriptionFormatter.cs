using Godot;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public static class SkillDescriptionFormatter
{
    private static readonly Regex AttackPowerPattern = new(
        @"[（(]\s*([-+]?\d+(?:\.\d+)?)\s*[）)]\s*攻击力\s*的?",
        RegexOptions.None);

    public static string Format(CommandData commandData, CharacterData source)
    {
        if (commandData == null)
        {
            return string.Empty;
        }

        return Format(SkillDefinition.FromCommandData(commandData), source);
    }

    public static string Format(SkillDefinition skill, CharacterData source)
    {
        string description = skill?.Description ?? string.Empty;
        if (string.IsNullOrEmpty(description) || source == null)
        {
            return description;
        }

        List<float> powerAmounts = BuildPowerAmounts(skill, source.atk);
        int replacementIndex = 0;

        return AttackPowerPattern.Replace(description, match =>
        {
            float amount = replacementIndex < powerAmounts.Count
                ? powerAmounts[replacementIndex]
                : CalculateFallbackAmount(match, source.atk);
            replacementIndex++;
            return $"{FormatAmount(amount)}点";
        });
    }

    private static List<float> BuildPowerAmounts(SkillDefinition skill, float attack)
    {
        List<float> amounts = new();
        if (skill == null)
        {
            return amounts;
        }

        foreach (SkillEffectDefinition effect in skill.Effects)
        {
            AppendPowerAmount(amounts, attack, effect);
        }

        return amounts;
    }

    private static void AppendPowerAmount(List<float> amounts, float attack, SkillEffectDefinition effect)
    {
        if (effect == null)
        {
            return;
        }

        if (effect.EffectType == SkillEffectType.Damage || effect.EffectType == SkillEffectType.Heal)
        {
            amounts.Add(SkillEffectMath.CalculatePowerAmount(attack, effect));
            return;
        }

        if (effect.EffectType != SkillEffectType.ApplyStatus || string.IsNullOrEmpty(effect.StatusId))
        {
            return;
        }

        foreach (SkillEffectDefinition triggerEffect in StatusCatalog.Create(effect.StatusId).TriggerEffects)
        {
            AppendPowerAmount(amounts, attack, triggerEffect);
        }
    }

    private static float CalculateFallbackAmount(Match match, float attack)
    {
        if (match?.Groups.Count > 1 &&
            float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float multiplier))
        {
            return SkillEffectMath.CalculatePowerAmount(attack, multiplier);
        }

        return 0;
    }

    private static string FormatAmount(float amount)
    {
        float rounded = Mathf.Round(amount);
        if (Mathf.Abs(amount - rounded) < 0.01f)
        {
            return ((int)rounded).ToString(CultureInfo.InvariantCulture);
        }

        return amount.ToString("0.#", CultureInfo.InvariantCulture);
    }
}
