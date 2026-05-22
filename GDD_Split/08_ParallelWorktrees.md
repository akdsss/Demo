# 并行开发切分建议

## Worktree A：战斗内核

负责：

- 时间轴数据结构。
- 行动计划。
- 技能合法性检查。
- 战斗结算器。
- 战斗事件列表。

主要目录：

- `Scripts/Combat/Timeline`
- `Scripts/Combat/Resolution`
- `Scripts/Combat/Skills`
- `Scripts/Combat/Statuses`
- `Tests/Combat`

避免修改：

- UI 场景。
- 美术资源。
- 关卡内容表。

## Worktree B：数据与配置

负责：

- 角色、敌人、技能、状态、区域、关卡配置。
- 数据 schema。
- 编码检查脚本。
- 本地化 key。

主要目录：

- `Data`
- `Scripts/Data`
- `Tools/Data`
- `Tools/Json`
- `Tools/Localization`
- `Tests/Data`

避免修改：

- 战斗 UI 场景。
- 演出动画。

## Worktree C：战斗 UI

负责：

- 时间轴 UI。
- 技能列表和详情。
- 目标选择。
- 战斗记录。
- 百科界面。

主要目录：

- `Scenes/UI`
- `Scenes/Battle`
- `Scripts/UI`

与战斗内核的接口：

- 读取 `BattleStateViewModel`。
- 提交 `PlayerActionRequest`。
- 消费 `CombatEvent`。

## Worktree D：演出与美术接入

负责：

- 角色头像、状态图标、区域图标；技能不制作图标，UI 中只显示技能名称。
- 水墨风静态资源生成与导入。
- 战斗播片占位资源接入；正式播片当前不生成。
- 行动动画。
- 伤害/治疗飘字。
- 状态图标。
- UI 音效触发接口；攻击/受击类战斗音效暂不制作。

主要目录：

- `Art`
- `Audio`
- `Scenes/Characters`
- `Scripts/Presentation`

避免修改：

- 战斗结算逻辑。
- 技能和敌人配置表中的数值。

## Worktree E：关卡、教程与成长

负责：

- 教学关流程。
- 关卡配置。
- 胜利奖励。
- 成长界面。
- 技能解锁。

主要目录：

- `Data/Levels`
- `Data/Tutorials`
- `Scripts/Progression`
- `Scenes/Levels`

## 推荐接口冻结顺序

1. 先冻结 `PlannedAction`、`CombatEvent`、`SkillDefinition`。
2. 再冻结 `BattleState` 和 UI view model。
3. 最后冻结数据表字段。

## 合并策略

- 先合并战斗内核和数据 schema。
- UI 使用假数据并行开发。
- 美术使用占位资源路径并行接入。
- 关卡先只引用已存在技能和敌人 ID。

## 最小集成里程碑

1. 空白战斗场景能加载一关。
2. 玩家能放置一个行动。
3. 敌人能生成一个行动。
4. 点击开始结算后生成日志。
5. UI 根据事件播放最简动画。
6. 胜负条件可触发。
7. 教学关、第二关、第三关均能按配置进入并结算。
