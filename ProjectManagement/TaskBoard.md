# TaskBoard

## Todo

### M2R 结算与演出分离

- [x] 定义 `CombatEvent` 类型。
- [x] 实现 `CombatResolver`，输入行动计划，输出事件列表。
- [x] 将移动、近战伤害、远程伤害、治疗、防御迁移到结算器。
- [x] 把 `CommandExecuteInfo.ExecuteInPlay()` 改为兼容层或废弃入口。
- [x] 准备阶段结束后先后台结算。
- [x] 演出阶段只播放 `CombatEvent`。
- [x] 战斗记录 UI 改为消费 `CombatEvent`。
- [x] 保留占位播片播放接口。

### M3R GDD 时间轴 UI 对齐

- [x] 复用现有 `CmdQueueUIControl`，改造成我方/敌方六时点时间轴。
- [x] 增加怪物“还剩 X 次”“思考中”“完成”显示。
- [x] 增加怪物行动未揭示/已揭示两种显示状态。
- [x] 实现空白时点长按 2 秒放置指令和进度条。
- [x] 确认指令后不可撤回。
- [x] 已设置指令的时点不可覆盖。
- [x] 实现“设置指令”“检视详情”“开始结算”三种交互控件。
- [x] 实现左键确定、右键返回 InputMap。
- [x] 技能 hover 显示优先级、MP、效果说明。

### M4R 教学关可玩

- [ ] 基于现有 `LevelData` 配置教学关。
- [ ] 实现基础敌人 AI 行动生成，不再固定使用第一个敌人技能。
- [ ] 建立教程步骤数据结构。
- [ ] 实现教程遮罩、高亮、提示文本和等待条件。
- [ ] 教程步骤 1：说明时间点和区域是战斗核心。
- [ ] 教程步骤 2：展示 6 个时间点。
- [ ] 教程步骤 3：展示八卦区域修正。
- [ ] 教程步骤 4：引导打开百科。
- [ ] 教程步骤 5：引导选择技能。
- [ ] 教程步骤 6：引导长按放置指令。
- [ ] 教程步骤 7：引导检视详情。
- [ ] 教程步骤 8：揭示怪物行动并解释优先级。
- [ ] 教程步骤 9：点击开始结算。
- [ ] 教程步骤 10：解释战斗记录。
- [ ] 教程步骤 11：演示进入/离开区域的修正变化。
- [ ] 教程步骤 12：胜利后展示成长界面。
- [ ] 实现百科入口和至少两个条目集合：区域修正、状态效果。
- [ ] 教学关完整通关测试。

### M5R 三关 Demo

- [ ] 配置第二关小型敌方团体。
- [ ] 配置第三关双精英战。
- [ ] 实现刺客/近战/远程/支援敌人 AI profile。
- [ ] 实现狂怒状态和狂怒行动逻辑。
- [ ] 实现关卡胜利后的成长奖励。
- [ ] 实现技能解锁和升级。
- [ ] 三关连续游玩测试。

### M6R 资源与打磨

- [ ] 生成并接入水墨风角色头像。
- [ ] 生成并接入敌人头像。
- [ ] 生成并接入区域图标。
- [ ] 生成并接入技能/状态图标。
- [ ] 接入战斗播片占位资源。
- [ ] 预留 UI 音效接口。
- [ ] 检查百科滚动或分页。
- [ ] 修复三关 demo 阻塞性问题。

## Doing

暂无。

## Review

暂无。

## Done

- [x] 拆分 GDD 到 `GDD_Split`。
- [x] 建立项目管理文档第一版。
- [x] 只读审计 `Demo` 现有结构。
- [x] 确认 `Demo` 已有 Godot C# 项目骨架。
- [x] 确认 `Demo` 已有主场景、UI 场景、基础角色/敌人/关卡数据资源。
- [x] 确认 `Demo` 已有六长度指令队列雏形。
- [x] 确认 `Demo` 已有准备阶段/演出阶段流程雏形。
- [x] 确认后续采用“兼容现状 + 内部分层”的结构策略，不立即大规模迁移目录。
- [x] 运行 `Demo` Godot 项目并记录当前可玩流程。
- [x] 确认 `Demo` 当前入口为开始菜单，点击“开始游戏”后进入 `Scene/MainScene.tscn` 并加载“测试关”。
- [x] 确认当前可玩流程为：选择玩家头像、选择攻击或移动、选择目标或棋盘格、点击六时点指令队列槽位、所有玩家行动设置完成后进入演出阶段。
- [x] 确认当前演出阶段仍由 `CommandExecuteInfo.ExecuteInPlay()` 直接执行攻击/移动，并非 GDD 要求的后台 `CombatEvent` 结算后播放。
- [x] 建立 `Demo` 当前结构到推荐结构的映射表：`ProjectManagement/DemoStructureMapping.md`。
- [x] 确认结构策略仍为保留 `Demo/Asset`、`Demo/Data`、`Demo/Scene`、`Demo/Script`，优先在现有目录下做内部分层，不立即迁移目录。
- [x] 检查 C# 文件是否存在真实编码损坏：33 个项目 C# 文件均可严格 UTF-8 解码，无 BOM，无常见乱码特征。
- [x] 检查现有 `.tres` 数据字段，列出可复用字段与 GDD 缺失字段：详见 `ProjectManagement/DemoM0RInspectionReport.md`。
- [x] 检查 `Scene/MainScene.tscn` 和 `Scene/MainUI.tscn` 的节点引用关系：详见 `ProjectManagement/DemoM0RInspectionReport.md`。
- [x] 明确哪些现有 UI prefab 可继续复用：详见 `ProjectManagement/DemoUIPrefabReuseReport.md`。
- [x] 确认 `CommandItem.tscn`、`action_listt.tscn`、`CharacterHeadButton.tscn`、`enemy_character_head_button.tscn`、`ActionButton.tscn`、`CmdHeadPrefab.tscn` 可继续作为 M3R UI 改造基础。
- [x] 确认 `ChessCellButton.tscn` 可短期复用为目标选择按钮，`chess_cell.tscn` 暂不直接复用。
- [x] 为后续重构建立最小回归清单：`ProjectManagement/DemoMinimalRegressionChecklist.md`。
- [x] 确认 M0R 审计与稳定化 Todo 全部完成，后续进入 M1R 战斗内核重构准备。
- [x] 在 `Demo/Script/Combat` 下建立战斗内核模型目录，不删除旧 `BattleManager` 流程。
- [x] 定义 `Timeline`、`TimelineSlot`、`PlannedAction`。
- [x] 定义 `SkillDefinition` 与 `SkillEffectDefinition`，并提供从现有 `CommandData` 生成技能定义的兼容入口。
- [x] 定义 `CharacterState`，可从现有 `CharacterData` 生成角色战斗快照。
- [x] 定义 `AreaDefinition`、区域修正与区域触发，为八卦区域替代当前棋盘坐标做准备，并保留旧坐标占位映射。
- [x] 定义 `StatusDefinition` 和 `StatusInstance`。
- [x] 实现 `SkillValidator`，覆盖行动机会、MP、目标战败、近战同区域、移动目标、时间轴槽位和状态禁止标签等基础失败原因。
- [x] 增加 `LegacyCommandAdapter`，提供旧 `CommandExecuteInfo` 到新 `PlannedAction` 的适配方案。
- [x] 定义 `CombatEvent` 与 `CombatEventType`，覆盖行动开始、失败、移动、伤害、治疗、防御、状态、战败和回合边界事件。
- [x] 实现 `CombatResolver`，可将旧队列适配出的 `PlannedAction` 按 1 到 6 时点和技能优先级结算为事件列表。
- [x] 新增 `CombatEventApplier` 与 `CombatEventLogFormatter`，当前状态标题/GD.Print 战斗记录输出改为消费 `CombatEvent`。
- [x] `BattleManager` 在准备阶段结束后后台生成 `pendingCombatEvents`，演出阶段只遍历事件并播放/应用事件。
- [x] `CommandExecuteInfo.ExecuteInPlay()` 已改为兼容入口，不再直接 switch 执行移动或伤害。
- [x] 保留 `PlayCombatEventPresentationPlaceholder()`，后续可接入战斗播片占位资源。
- [x] `CmdQueueUIControl` 复用现有六列指令队列，按玩家/敌人实际数量生成时间轴行，并用我方/敌方颜色区分。
- [x] 敌人头像状态支持“思考中”“还剩 X 次”“完成”。
- [x] 敌方时间轴行动支持未揭示标签显示与结算前揭示。
- [x] `CommandItemUIControl` 支持空白时点长按 2 秒放置指令、进度条、不可撤回和不可覆盖提示。
- [x] 动态生成“设置指令”“检视详情”“开始结算”控件；准备阶段后台结算后等待玩家点击“开始结算”再进入演出。
- [x] `MainUIControl` 运行时注册 `confirm`、`place_action_hold`、`back`、`inspect` InputMap，右键返回会取消当前指令选择。
- [x] 技能按钮 hover 时在时间轴工具条显示优先级、MP、目标、标签和效果说明。
