# ChangeLog

## 2026-05-21

- 创建 `ProjectManagement` 目录。
- 创建 `Roadmap.md`、`Milestones.md`、`TaskBoard.md`、`Decisions.md`、`Risks.md`、`ChangeLog.md`。
- 根据 `GDD_Split` 和更新后的 GDD 整理第一版项目路线。
- 将 M3 教学关补充为 12 步教学流程。
- 明确当前范围为 Windows 平台 3 关可玩 demo。
- 记录资源策略：战斗播片占位，静态美术优先水墨风，BGM 和攻击/受击音效暂不考虑。

## 2026-05-21 Demo 只读审计

- 只读检查 `Demo`，未修改其中任何文件。
- 确认 `Demo` 是 Godot 4.6 C# 项目，包含 `project.godot`、`Demo.csproj`、`Demo.sln`。
- 确认已有 `Asset`、`Data`、`Scene`、`Script` 目录，以及 `addons/godot_mcp`。
- 确认已有主场景、主 UI、开始菜单、角色场景、UI prefab。
- 确认已有 `BattleManager`、`ChessBoard`、`GameMain`、`SceneSingleton`、`EventManager` 等基础逻辑脚本。
- 确认已有 `CharacterData`、`PlayerData`、`EnemyData`、`CommandData`、`LevelData` 等资源脚本和 `.tres` 数据资源。
- 确认已有六长度指令队列雏形，与 GDD 六时点方向一致。
- 发现现有实现尚未满足 GDD 的关键要求：结算与演出未分离、缺少 `CombatEvent`、缺少技能标签/合法性检查/区域数据模型/百科/教程/成长。
- 更新 `Roadmap.md`：从“新建项目”调整为“基于现有 Demo 演进”。
- 更新 `TaskBoard.md`：新增 M0R 到 M6R，优先安排 Demo 稳定化、战斗内核重构准备、结算与演出分离、GDD UI 对齐。
- 确定后续结构策略：保留现有 `Asset/Data/Scene/Script`，先做内部子目录和适配层，不进行大规模目录迁移。

## 2026-05-21 Demo 运行与可玩流程记录

- 先复核 `GDD_Split/02_CoreLoop_Timeline.md`、`03_CombatRules_Resolution.md`、`06_UI_UX.md`、`07_GodotProjectStructure.md` 和 `DevGuidelines.md`，确认本次只做运行审计与流程记录，不改玩法代码、不迁移目录。
- 找到 Godot 可执行文件 `D:\Software\Godot_C\Godot_v4.6.2-stable_mono_win64.exe`，并用 `--headless --path D:\GodotProject\202605gamejam\Demo --quit-after 3` 做启动验证。
- 读取 `Demo/godot.log`，确认项目曾以 Godot `v4.6.2.stable.mono.official.71f334935` 启动，并输出“开始游戏”。
- 确认 `project.godot` 当前主场景为 `Scene/StartMenu.tscn`；开始菜单提供“开始游戏”“设置”“退出”三个入口。
- 当前可玩入口：点击“开始游戏”后执行 `StartMenuContol.StartGameButtonClicked()`，切换到 `Scene/MainScene.tscn`。
- `MainScene.tscn` 挂载 `GameMain`、`SceneSingleton` 和 `MainUI`，并引用 `Data/LevelData/Level_data0.tres`；当前加载关卡名为“测试关”。
- 当前关卡初始化流程：`LevelData.LevelInitialize()` 重置棋盘，将 2 名玩家和 2 名敌人放入现有 3 行 10 格棋盘，并更新顶部状态文本为“关卡已加载”。
- 当前战斗流程：`BattleManager.BattleStart()` 初始化玩家/敌人列表、头像列表、六长度指令队列和队列头像，然后进入回合循环。
- 当前准备阶段流程：敌人优先准备，但逻辑固定使用每个敌人的 `enemyCommandDataArray[0]` 放入第 1 个时点；随后玩家通过头像选择角色、通过技能按钮选择攻击或移动。
- 当前玩家操作流程：攻击会启用敌方头像作为目标，移动会启用棋盘格作为目标；目标确定后，点击六时点指令队列对应槽位即可设置指令。
- 当前演出阶段流程：所有玩家角色完成行动设置后，`PlayTurn()` 遍历 6 个时点和所有角色，直接调用 `CommandExecuteInfo.ExecuteInPlay()` 执行攻击或移动。
- 当前与 GDD 的主要差距：可启动并能走通“开始菜单 -> 测试关 -> 准备阶段 -> 放置指令 -> 演出阶段”的基础流程，但尚未实现后台 `CombatEvent` 结算、开始结算按钮、敌方行动揭示、长按 2 秒放置、不可覆盖检查、八卦区域、教程、百科和成长流程。
- 更新 `TaskBoard.md`：完成 M0R Todo“运行 Godot 项目并记录当前可玩流程”，下一个 M0R Todo 为“建立 `Demo` 当前结构到推荐结构的映射表”。

## 2026-05-21 Demo 结构映射表

- 先复核 `GDD_Split/07_GodotProjectStructure.md` 与当前 `TaskBoard.md`，确认本次任务是建立映射表，不做目录迁移、不改 Demo 代码或资源。
- 新增 `ProjectManagement/DemoStructureMapping.md`，记录 `Demo/Asset`、`Demo/Data`、`Demo/Scene`、`Demo/Script` 到推荐 `Art`、`Data`、`Scenes`、`Scripts` 的逻辑对应关系。
- 映射表明确 `Demo/project.godot`、`Demo/Demo.csproj`、`Demo/Demo.sln` 等项目根配置保持原位，不纳入运行期内容目录迁移。
- 映射表记录现有覆盖情况：`Asset` 部分覆盖 `Art`，`Data/CommandData` 部分覆盖 `Data/Combat`，`Data/CharacterData` 部分覆盖 `Data/Content`，`Data/LevelData` 部分覆盖 `Data/Levels`，`Scene` 和 `Script` 分别覆盖推荐 `Scenes` 与 `Scripts` 的若干子域。
- 映射表记录当前缺口：`Audio`、`Data/Localization`、`Data/Tutorials`、`Scripts/Presentation`、`Scripts/Progression`、`Tests`、`Tools`、`Shaders` 暂无明确对应目录。
- 明确后续新增 M1R 代码时优先采用现有单数目录下的内部分层，例如 `Demo/Script/Combat/Timeline`，以避免破坏已有 `res://` 引用。
- 更新 `TaskBoard.md`：完成 M0R Todo“建立 `Demo` 当前结构到推荐结构的映射表”，下一个 M0R Todo 为“检查 C# 文件是否存在真实编码损坏，区分终端显示乱码和文件内容损坏”。

## 2026-05-21 Demo M0R 三项检查

- 先复核 `GDD_Split/01_DesignReview.md`、`03_CombatRules_Resolution.md`、`05_Content_Data.md`、`06_UI_UX.md` 和 `ProjectManagement/DemoStructureMapping.md`，确认本次只做检查与记录，不改 Demo 代码、场景或资源。
- 新增 `ProjectManagement/DemoM0RInspectionReport.md`，集中记录 C# 编码检查、`.tres` 字段检查、`MainScene.tscn` / `MainUI.tscn` 节点引用检查。
- C# 编码检查：扫描 33 个项目 C# 文件，全部可严格 UTF-8 解码，无 UTF-8 BOM，无常见乱码特征；当前没有发现真实编码损坏。
- `.tres` 字段检查：扫描 `Demo/Data` 下 22 个 `.tres` 资源，确认当前角色、敌人、指令、关卡、坐标站位字段可复用为原型数据；同时记录 GDD 缺失字段，包括技能标签、MP、目标类型、结构化效果、区域、状态、教程、百科、成长、AI profile 等。
- 场景引用检查：`MainScene.tscn` 当前有 11 个节点、8 个外部资源、0 个信号连接、10 个 editable path；`MainUI.tscn` 当前有 206 个节点、23 个外部资源、2 个信号连接、9 个 editable path。
- 确认 `MainScene.tscn` 通过 `GameMain` 引用 `Level_data0.tres`，通过 `SceneSingleton` 深层引用 `MainUI` 内部状态标签与玩家选择面板；后续改 UI 层级需同步更新这些 NodePath。
- 确认 `MainUI.tscn` 已有可复用原型结构：玩家头像区、技能选择面板、10 格棋盘、敌人头像区、六时点指令队列；但尚未符合 GDD 的双时间轴、八卦区域、开始结算按钮、战斗记录和百科入口要求。
- 更新 `TaskBoard.md`：完成 M0R 三个 Todo“C# 编码检查”“`.tres` 数据字段检查”“主场景节点引用检查”，下一个 M0R Todo 为“明确哪些现有 UI prefab 可继续复用”。

## 2026-05-21 Demo UI prefab 复用审计

- 基于 `DemoM0RInspectionReport.md` 中的 `MainUI` 资源和 prefab 引用继续检查，未修改 `Demo` 代码、场景或资源。
- 新增 `ProjectManagement/DemoUIPrefabReuseReport.md`，记录现有 UI prefab 的复用等级、当前用途、改造点和 M3R 复用优先级。
- 确认 `CommandItem.tscn` 是最接近 GDD 六时点槽位的 prefab，应重点复用为时间轴槽位；后续需要补长按 2 秒、进度条、不可覆盖、敌方未揭示/已揭示状态和请求式提交。
- 确认 `action_listt.tscn` 可短期复用为时间轴容器，但当前语义仍是按角色行混排的指令列，M3R 需要重排为我方/敌方时间轴。
- 确认 `CharacterHeadButton.tscn`、`enemy_character_head_button.tscn`、`CmdHeadPrefab.tscn` 可继续支撑玩家/敌人头像、HP、行动状态和时间轴行头像显示。
- 确认 `ActionButton.tscn` 可继续作为技能列表按钮基础，但需要扩展 hover 详情、MP、优先级、目标类型、合法性状态。
- 确认 `ChessCellButton.tscn` 可短期复用为目标选择按钮；八卦区域系统落地后应改为区域按钮或区域热点。
- 确认 `chess_cell.tscn` 当前未被 `MainUI.tscn` 引用，且导出 NodePath 不完整，暂不直接复用。
- 更新 `TaskBoard.md`：完成 M0R Todo“明确哪些现有 UI prefab 可继续复用”，下一个 M0R Todo 为“为后续重构建立最小回归清单：启动、加载关卡、显示角色、放置指令、执行一回合”。

## 2026-05-21 Demo 最小回归清单

- 基于 `DemoM0RInspectionReport.md`、`DemoUIPrefabReuseReport.md`、`GDD_Split/02_CoreLoop_Timeline.md` 和 `GDD_Split/06_UI_UX.md` 建立最小回归基线。
- 新增 `ProjectManagement/DemoMinimalRegressionChecklist.md`，覆盖快速启动检查与 5 个当前 Demo 人工回归项。
- 快速启动检查记录 Godot 可执行文件路径、Demo 项目路径和 headless 启动命令，用于确认项目能被 Godot 加载。
- 人工回归项包括：REG-001 启动到开始菜单、REG-002 进入测试关并加载关卡、REG-003 显示角色与敌人、REG-004 放置玩家指令、REG-005 执行一回合。
- 清单明确当前基线仍按现有 Demo 行为验收：单击放置指令、旧 `CommandExecuteInfo.ExecuteInPlay()` 直接执行、无“开始结算”按钮、无后台 `CombatEvent`。
- 清单为每个回归项记录失败时优先检查的脚本、场景或数据文件，便于 M1R/M2R/M3R 修改后定位回归。
- 更新 `TaskBoard.md`：完成 M0R 最后一个 Todo，并确认 M0R 审计与稳定化 Todo 全部完成；下一个 Todo 进入 M1R“在 `Script/Combat` 下规划核心模型，不立即删除旧 `BattleManager` 流程”。

## 2026-05-21 M1R 战斗内核重构准备

- 先阅读 `GDD_Split` 与 `ProjectManagement` 下全部文档，确认本次 M1R 只新增兼容型内核模型，不删除旧 `BattleManager`、不迁移旧场景或数据资源。
- 新增 `Demo/Script/Combat` 内部分层：`Timeline`、`Skills`、`Characters`、`Areas`、`Statuses`、`Validation`、`Adapters`。
- 新增 `CombatEnums.cs`，集中定义技能标签、阵营、目标类型、效果类型、失败原因、区域 ID、触发时机、状态叠加和持续类型等基础枚举。
- 新增 `Timeline`、`TimelineSlot`、`PlannedAction`，对应 GDD 的六时点时间轴、槽位锁定/揭示状态和行动计划。
- 新增 `SkillDefinition` 与 `SkillEffectDefinition`，提供 `FromCommandData()` 兼容入口，可从现有 `PlayerCommandData` / `EnemyCommandData` 推导旧指令 ID、显示名、优先级、标签、目标类型和基础效果。
- 新增 `CharacterState`，可从现有 `CharacterData` 生成战斗快照，保留旧角色 ID、HP、攻击、行动次数、棋盘坐标、阵营和旧数据引用。
- 新增 `AreaDefinition`、`AreaModifier`、`AreaTrigger`，包含八卦区域默认列表，并用 `FromLegacyCoord()` 保留当前棋盘坐标到区域模型的占位兼容。
- 新增 `StatusDefinition` 与 `StatusInstance`，为后续闪避、刻印、护盾、燃烧、罡风、狂怒等状态进入统一触发与修正模型做准备。
- 新增 `SkillValidator`、`ValidationContext`、`SkillValidationResult`，覆盖 GDD 基础失败原因：缺技能/来源、来源战败、行动机会不足、MP 不足、缺目标、目标战败、近战不同区域、移动目标为当前位置、突袭类目标同区域、时间轴槽不可用、状态禁止技能标签。
- 新增 `LegacyCommandAdapter`，提供 `CommandExecuteInfo.ToPlannedAction()`、`CharacterData.ToTimeline()` 与旧队列转新行动列表的适配方案，后续 M2R 可在不立即废弃旧执行入口的情况下接入结算器。
- 验证记录：普通 Godot headless 启动可退出；完整 `dotnet build` 与 Godot `--build-solutions` 在当前沙箱内受 `Godot.NET.Sdk` / NuGet 用户缓存与网络访问限制影响，无法完成正式构建。额外用 Roslyn 本地编译做语法/类型抽查，新增 `Demo/Script/Combat` 文件未出现新增编译错误；抽查命中的是旧 `BattleManager` 依赖 Godot SourceGenerator 生成信号名导致的直接 `csc` 误报。
- 更新 `TaskBoard.md`：M1R 战斗内核重构准备 Todo 全部完成；下一个 Todo 进入 M2R“定义 `CombatEvent` 类型”。

## 2026-05-21 M2R 结算与演出分离

- 先阅读 `GDD_Split` 与 `ProjectManagement` 下全部文档，确认 M2R 目标是先完成后台事件结算和旧流程兼容，不迁移 UI 场景、不改数据资源。
- 在 `Demo/Script/Combat/CombatEnums.cs` 中新增 `CombatEventType`，覆盖回合开始/结束、行动开始、技能失败、移动、伤害、治疗、防御、MP、状态、延迟效果和战败事件。
- 新增 `Demo/Script/Combat/Resolution/CombatEvent.cs`，作为结算器输出给演出阶段、日志和后续占位播片使用的结构化事件。
- 新增 `CombatResolver`，输入 `PlannedAction` 列表，按 1 到 6 时点与技能优先级生成 `CombatEvent` 列表；当前支持移动、伤害、治疗、防御、状态与延迟效果事件，其中远程伤害复用伤害效果通道。
- `CombatResolver` 在结算时使用旧 `CharacterData` 的快照状态，不在准备阶段直接改动旧角色 HP 或棋盘坐标；真实旧状态由演出阶段消费事件时应用。
- 新增 `CombatEventApplier`，负责在演出阶段把 `CharacterMoved`、`DamageApplied`、`HealApplied`、`CharacterDefeated` 应用回旧 Demo 状态和现有头像/棋盘 UI。
- 新增 `CombatEventLogFormatter`，当前最小战斗记录表面为状态标题和 Godot 输出，已改为消费 `CombatEvent`；后续若新增滚动战斗记录面板，可继续复用该格式化入口。
- `BattleManager` 新增 `pendingCombatEvents`：准备阶段结束后通过 `LegacyCommandAdapter.ToPlannedActions()` 后台结算，演出阶段只遍历事件，不再遍历旧指令队列直接执行。
- `BattleManager` 保留 `PlayCombatEventPresentationPlaceholder()` 占位播片接口，后续可接入每个指令约 3 秒的战斗播片占位资源。
- `CharacterData` 新增 `ResetCommandQueue()`，准备阶段初始化时清空旧回合指令，避免后台结算重复消费上一回合残留队列。
- `CommandExecuteInfo.ExecuteInPlay()` 改为兼容入口：被旧代码调用时也会通过 `CombatResolver` 生成并应用 `CombatEvent`，不再在方法内直接 switch 修改 HP 或棋盘坐标。
- 验证记录：`dotnet build Demo/Demo.csproj --no-restore` 在沙箱内因无法读取本机 `NuGet.Config` 与 `Godot.NET.Sdk` 失败；已按规则请求沙箱外构建，但自动审批两次超时。Godot `--headless --path Demo --quit-after 3` 与 `--build-solutions --quit` 均正常退出。直接 Roslyn 抽查命中旧 `BattleManager` Godot 信号源生成器相关 `MainTS`/`PreTS`/`PlayTS`/`BS` 误报，未出现本次新增类型的编译错误。
- 编码检查：本次新增/修改的 C# 与项目管理 Markdown 文件均可严格 UTF-8 解码，且无 UTF-8 BOM。
- 更新 `TaskBoard.md`：M2R 结算与演出分离 Todo 全部完成；下一个 Todo 进入 M3R“复用现有 `CmdQueueUIControl`，改造成我方/敌方六时点时间轴”。

## 2026-05-22 M3R GDD 时间轴 UI 对齐

- 先阅读 `GDD_Split` 与 `ProjectManagement` 下全部文档，确认 M3R 目标是复用现有 `CmdQueueUIControl`、`CommandItem.tscn`、`action_listt.tscn`、头像 prefab 与技能按钮，不做大规模场景迁移。
- `BattleManager.BattleInitialize()` 现在会按当前关卡实际玩家+敌人数量更新 `SceneSingleton.gameCharacterNum`，避免时间轴继续生成多余空行。
- `ActionListUIControl` 支持由 `CmdQueueUIControl` 指定实际行数；`CommandHeadListUIControl.SetCharacterHead()` 改为按关卡动态重建头像行。
- `CmdQueueUIControl` 继续复用现有六列矩阵，给每个槽位绑定对应玩家或敌人；我方槽位与敌方槽位使用不同底色，作为当前阶段的双阵营时间轴过渡表现。
- `CmdQueueUIControl` 动态生成“设置指令”“检视详情”“开始结算”工具条和详情文本区，不改 `MainUI.tscn` 深层节点结构。
- `CommandItemUIControl` 支持空白时点长按 2 秒放置指令，并动态创建 `HoldProgressBar` 显示进度；单击空白槽位只提示长按，不再直接放置。
- `CommandItemUIControl` 已设置槽位保持 `LOCKED`，再次点击仅检视详情，不允许覆盖，也不允许撤回。
- 敌方 `CommandItemUIControl` 初始显示未揭示意图标签，调用 `RevealEnemyActions()` 后显示技能名和优先级。
- `BattleManager` 在后台 `CombatEvent` 结算完成后揭示怪物行动，显示“开始结算”按钮，并等待玩家点击后才进入演出阶段。
- `EnemyCharacterHeadListUIControl` 与 `CharacterHeadButtonControl` 增加怪物准备状态显示：思考中、还剩 X 次、完成。
- `PlayerActionButtonControl` 增加 hover 详情显示，当前通过 `SkillDefinition.FromCommandData()` 展示优先级、MP、目标类型、标签和效果说明，并在 hover 时轻微放大按钮。
- `MainUIControl` 运行时注册 InputMap：`confirm` 与 `place_action_hold` 绑定左键，`back` 绑定右键，`inspect` 绑定中键；右键返回会取消当前指令选择。
- 验证记录：Godot `--headless --path Demo --quit-after 3` 与 `--build-solutions --quit` 均正常退出。直接 `dotnet build Demo/Demo.csproj --no-restore` 仍因沙箱内无法读取本机 `NuGet.Config` / `Godot.NET.Sdk` 失败；按规则请求沙箱外构建两次，自动审批均超时。Roslyn 直接抽查只命中旧 Godot 信号源生成器相关 `MainTS` / `PreTS` / `PlayTS` / `BS` 误报，未出现本次新增 UI 代码的类型错误。
- 编码检查：本次新增/修改的 C# 与项目管理 Markdown 文件均可严格 UTF-8 解码，且无 UTF-8 BOM。
- 更新 `TaskBoard.md`：M3R GDD 时间轴 UI 对齐 Todo 全部完成；下一个 Todo 进入 M4R“基于现有 `LevelData` 配置教学关”。
