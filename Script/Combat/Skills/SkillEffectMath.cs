using System;

public static class SkillEffectMath
{
    public static float ResolvePowerMultiplier(SkillEffectDefinition effect)
    {
        return effect?.Value > 0 ? effect.Value : 1.0f;
    }

    public static float CalculatePowerAmount(float attack, SkillEffectDefinition effect)
    {
        return CalculatePowerAmount(attack, ResolvePowerMultiplier(effect));
    }

    public static float CalculatePowerAmount(float attack, float multiplier)
    {
        return Math.Max(0, attack * multiplier);
    }
}
