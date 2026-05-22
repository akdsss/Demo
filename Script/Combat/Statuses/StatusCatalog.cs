public static class StatusCatalog
{
    public const string Mark = "刻印";
    public const string Burn = "燃烧";
    public const string Dodge = "闪避";
    public const string Shield = "护盾";
    public const string Gale = "罡风";
    public const string GaleImmune = "罡风免疫";
    public const string MoveBlocked = "湍扼";
    public const string GenMeleeCarryover = "压顶残留";
    public const string KunMeleeDrainCarryover = "丰壤残留";
    public const string Rage = "狂怒";

    public static StatusDefinition Create(string statusId)
    {
        return statusId switch
        {
            Mark => new StatusDefinition
            {
                Id = Mark,
                DisplayName = Mark,
                Description = "克制闪避。",
                DurationType = StatusDurationType.Slots,
                DurationValue = 6,
                StackMode = StatusStackMode.RefreshDuration
            },
            Burn => new StatusDefinition
            {
                Id = Burn,
                DisplayName = Burn,
                Description = "回合结束时受到最大生命值百分比伤害；位于坎时移除，位于乾、兑时伤害加重。",
                DurationType = StatusDurationType.Rounds,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration
            },
            Dodge => new StatusDefinition
            {
                Id = Dodge,
                DisplayName = Dodge,
                Description = "闪避单体远程攻击；被刻印克制。",
                DurationType = StatusDurationType.Rounds,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration
            },
            Shield => new StatusDefinition
            {
                Id = Shield,
                DisplayName = Shield,
                Description = "吸收伤害的护盾。",
                DurationType = StatusDurationType.Rounds,
                DurationValue = 1,
                StackMode = StatusStackMode.RefreshDuration
            },
            Gale => new StatusDefinition
            {
                Id = Gale,
                DisplayName = Gale,
                Description = "叠加至3层时受到最大生命值40%的伤害。",
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
