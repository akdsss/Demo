public static class StatusCatalog
{
    public const string Mark = "刻印";
    public const string Burn = "燃烧";
    public const string Dodge = "闪避";
    public const string Defense = "防御";
    public const string CounterSingleMelee = "断锋";
    public const string CounterMelee = "千军断";
    public const string PowerUp = "天机加护";
    public const string Shield = "护盾";
    public const string Gale = "罡风";
    public const string GaleImmune = "罡风免疫";
    public const string MoveBlocked = "湍扼";
    public const string GenMeleeCarryover = "压顶残留";
    public const string KunMeleeDrainCarryover = "丰壤残留";
    public const string Rage = "血煞";

    public static StatusDefinition Create(string statusId)
    {
        return statusId switch
        {
            Mark => new StatusDefinition
            {
                Id = Mark,
                DisplayName = Mark,
                Description = "无视闪避。",
                DurationType = StatusDurationType.Slots,
                DurationValue = 6,
                StackMode = StatusStackMode.RefreshDuration
            },
            Burn => new StatusDefinition
            {
                Id = Burn,
                DisplayName = Burn,
                Description = "回合结束时，受到最大生命值6%的伤害。特别的，若位于坎，移除该状态；若位于乾、兑，则受到最大生命值12%的伤害。",
                DurationType = StatusDurationType.Rounds,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration
            },
            Dodge => new StatusDefinition
            {
                Id = Dodge,
                DisplayName = Dodge,
                Description = "闪避单体远程攻击。",
                DurationType = StatusDurationType.Rounds,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration
            },
            Defense => new StatusDefinition
            {
                Id = Defense,
                DisplayName = Defense,
                Description = "该时点期间受到的伤害减半。",
                DurationType = StatusDurationType.Slots,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration
            },
            CounterSingleMelee => new StatusDefinition
            {
                Id = CounterSingleMelee,
                DisplayName = CounterSingleMelee,
                Description = "格挡1次单体近战攻击，并对攻击者造成1.0攻击力的近战伤害。",
                DurationType = StatusDurationType.Slots,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration,
                RemoveCondition = StatusRemoveCondition.NextMeleeHit,
                TriggerEffects = { SkillEffectDefinition.Damage(1.0f) }
            },
            CounterMelee => new StatusDefinition
            {
                Id = CounterMelee,
                DisplayName = CounterMelee,
                Description = "格挡1次近战攻击，并对攻击者造成1.0攻击力的近战伤害。",
                DurationType = StatusDurationType.Slots,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration,
                RemoveCondition = StatusRemoveCondition.NextMeleeHit,
                TriggerEffects = { SkillEffectDefinition.Damage(1.0f) }
            },
            PowerUp => new StatusDefinition
            {
                Id = PowerUp,
                DisplayName = PowerUp,
                Description = "造成伤害+50%，持续3时点。",
                DurationType = StatusDurationType.Slots,
                DurationValue = 3,
                StackMode = StatusStackMode.RefreshDuration
            },
            Shield => new StatusDefinition
            {
                Id = Shield,
                DisplayName = Shield,
                Description = "受伤时优先扣除护盾以代替生命值。回合开始时清零。",
                DurationType = StatusDurationType.Rounds,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration
            },
            Gale => new StatusDefinition
            {
                Id = Gale,
                DisplayName = Gale,
                Description = "叠加至3层时，受到最大生命值40%的伤害。触发后，免疫罡风。",
                DurationType = StatusDurationType.UntilRemoved,
                StackMode = StatusStackMode.StackCount
            },
            GaleImmune => new StatusDefinition
            {
                Id = GaleImmune,
                DisplayName = GaleImmune,
                Description = "触发罡风爆发后免疫罡风。",
                DurationType = StatusDurationType.UntilRemoved
            },
            MoveBlocked => new StatusDefinition
            {
                Id = MoveBlocked,
                DisplayName = MoveBlocked,
                Description = "本回合移动类技能失效。",
                DurationType = StatusDurationType.Rounds,
                DurationValue = 1,
                TagsBlocked = SkillTag.Move
            },
            GenMeleeCarryover => new StatusDefinition
            {
                Id = GenMeleeCarryover,
                DisplayName = GenMeleeCarryover,
                Description = "离开艮后，下一次近战攻击仍获得压顶加成。",
                DurationType = StatusDurationType.TriggerCount,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration,
                RemoveCondition = StatusRemoveCondition.NextMeleeHit
            },
            KunMeleeDrainCarryover => new StatusDefinition
            {
                Id = KunMeleeDrainCarryover,
                DisplayName = KunMeleeDrainCarryover,
                Description = "离开坤后，下一次近战攻击仍获得丰壤吸血。",
                DurationType = StatusDurationType.TriggerCount,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration,
                RemoveCondition = StatusRemoveCondition.NextMeleeHit
            },
            Rage => new StatusDefinition
            {
                Id = Rage,
                DisplayName = Rage,
                Description = "造成伤害+50%，受到伤害+50%，每回合可用技能次数+1，持续至战斗结束。",
                DurationType = StatusDurationType.UntilRemoved,
                StackMode = StatusStackMode.Unique
            },
            _ => new StatusDefinition { Id = statusId, DisplayName = statusId }
        };
    }
}
