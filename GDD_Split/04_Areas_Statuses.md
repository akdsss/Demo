# 区域与状态

## 区域系统

原 GDD 使用八卦区域作为战斗站位和区域修正来源。可辨认区域包括乾、坤、震、巽、坎、离、艮、兑，以及阴/阳相关表述。

区域承担三类功能：

- 站位：角色当前处于哪个区域。
- 修正：对技能、伤害、治疗、优先级等产生影响。
- 触发：在回合开始、回合结束、造成伤害、离开区域等时点触发效果。

## 推荐数据模型

`AreaDefinition`：

- `id`
- `displayName`
- `description`
- `modifiers`
- `triggers`
- `encyclopediaText`

`AreaModifier`：

- `condition`
- `target`
- `operation`
- `value`
- `durationPolicy`

`AreaTrigger`：

- `timing`
- `condition`
- `effect`

## 区域保留效果

文档中出现“离开区域后，某加成可持续一次近战攻击”等规则。建议不要把它当作区域仍在生效，而是在离开区域时施加一个临时状态。

例如：

- `AreaCarryoverMeleeBonus`
- 持续到下一次近战命中或回合结束。

## 状态系统

可辨认状态包括：

- 闪避：可被部分位移技能施加，持续若干时点。
- 刻印。
- 护盾。
- 燃烧。
- 罡风或类似延迟爆发状态。
- 狂怒。
- 禁止移动或禁止远程等控制效果。

## 状态字段

建议每个状态至少包含：

- `id`
- `displayName`
- `description`
- `stackMode`
- `durationType`
- `durationValue`
- `timingTriggers`
- `tagsBlocked`
- `modifiers`
- `removeCondition`

## 触发时机

建议统一枚举：

- `RoundStart`
- `RoundEnd`
- `BeforeAction`
- `AfterAction`
- `BeforeDamage`
- `AfterDamage`
- `OnMove`
- `OnLeaveArea`
- `OnEnterArea`
- `OnDefeated`

## 需要补充的问题

- 同名状态是否可叠加。
- 状态持续时间以回合计，还是以触发次数计。
- 角色战败后状态是否清空。
- 区域状态和角色状态发生冲突时谁优先。
- 区域修正是否影响敌人和玩家双方。
- “进入区域或回合开始”类触发应统一排序，例如艮区域要求回合开始时先移除所有护盾，再结算回合开始触发效果。
