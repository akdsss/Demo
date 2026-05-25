# 区域与状态

## 区域设计核心支柱

1. 战场分为十个区域：中间圆形分为阴阳两个区域，map.png相关部分为传统阴阳太极图，左阳右阴。紧贴中间圆形的是一个圆环，分为八个区域，对应八卦：北为坎、东北为艮、东为震、东南为巽、南为离、西南为坤、西为兑、西北为乾
2. 每个区域都会对处于其中的所有单位施加独特修正。若无特殊说明，离开区域后原修正立刻失效。
3. 代码层面的角色站位必须保存为 `CurrentAreaId`，不得再用旧棋盘坐标推导区域。旧 `(row,col)` 坐标只能作为迁移前兼容数据，不可作为新区域系统的真实数据源。
4. 区域范围技能必须按 `CurrentAreaId == TargetAreaId` 命中整个目标区域内的单位，且不得命中其他区域单位。范围结算不读取头像屏幕位置、不读取锚点编号、不读取背景图像素。
5. 视觉层每个区域拥有 9 个固定透明头像锚点。锚点只用于显示同一区域内多个角色的位置，不参与区域判断、范围伤害、技能合法性或 AI 决策。

## 区域系统

原 GDD 使用八卦区域作为战斗站位和区域修正来源。可辨认区域包括乾、坤、震、巽、坎、离、艮、兑，以及阴/阳相关表述。

区域承担三类功能：

- 站位：角色当前处于哪个区域。
- 修正：对技能、伤害、治疗、优先级等产生影响。
- 触发：在回合开始、回合结束、造成伤害、离开区域等时点触发效果。

## 区域逻辑与视觉分离

未来我阅读此文档时必须按以下规则理解区域系统：

- `CombatAreaId` / `CurrentAreaId` 是战斗真相。
- `AreaAnchorIndex` 是视觉槽位，不是战斗位置。
- `TargetAreaId` 是区域技能目标，不是点击到的地图坐标。
- 角色移动只改变该角色自己的 `CurrentAreaId` 和视觉锚点，不触发同区域其他角色重排。
- 角色进入新区域时，在目标区域 9 个透明锚点中找空锚点；角色离开时释放旧锚点。
- 如果目标区域没有空锚点，应视为视觉容量异常；不要把角色挤到其他区域显示。
- 范围技能结算必须从当前战斗角色集合中过滤 `CurrentAreaId == TargetAreaId` 的存活单位。
- 是否排除施法者由技能效果字段决定，不由区域系统隐式决定。
- UI、动画、锚点、背景图、点击菜单都不能改变结算命中边界。

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

- 同名状态是否可叠加：无特殊说明，均不可叠加
- 角色战败后状态是否清空：是
- 区域状态和角色状态发生冲突时谁优先：设计上没有冲突，代码里也不应该有
