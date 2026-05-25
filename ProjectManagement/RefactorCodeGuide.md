# 重构代码说明

更新时间：2026-05-24

本文以 `generalGDDforAI.md` 与 `GDD_Split/DevGuidelines.md` 为基准，说明当前 `Demo` 代码的运行架构、主要规范与历史命名映射。目标是让后续开发者能从 GDD 语义进入代码，而不是被早期 Demo 原型的命名牵着走。

## 当前架构

### 入口与场景

| 模块 | 路径 | 责任 |
| --- | --- | --- |
| Godot 项目入口 | `Demo/project.godot` | 主场景为 `Scene/StartMenu.tscn`，保留 `gd_ChessBoard` 自动加载。 |
| 开始菜单 | `Demo/Scene/StartMenu.tscn` / `Script/UIControl/StartMenuContol.cs` | 进入 `Scene/MainScene.tscn`。 |
| 主场景 | `Demo/Scene/MainScene.tscn` | 挂载 `GameMain`、`SceneSingleton`、`MainUI`，并引用当前关卡资源。 |
| UI 根 | `Demo/Scene/MainUI.tscn` / `Script/UIControl/MainUIControl.cs` | 注册输入、百科、教程、成长奖励、区域目标菜单。 |
| 战斗入口 | `Script/GameLogic/GameMain.cs` | 绑定 10 个区域显示锚点，初始化关卡并启动 `BattleManager`。 |

### 战斗主流程

| 阶段 | 主要代码 | 说明 |
| --- | --- | --- |
| 关卡加载 | `Data/DataScript/LevelData.cs` | 将关卡中的玩家/敌人放入 `CurrentAreaId` 指定的区域。旧 `coord` 只作为兼容坐标。 |
| 战斗循环 | `Script/GameLogic/BattleManager.cs` | 回合初始化、敌方准备、我方准备、后台结算、等待“开始结算”、演出事件播放。 |
| 敌方行动 | `Script/Combat/AI/EnemyActionPlanner.cs` | 根据 AI profile 生成 `CommandExecuteInfo`，再进入统一时间轴。 |
| 我方行动 | `Script/UIControl/CmdQueueUIControl.cs` + `CommandItemUIControl.cs` | 左侧选择技能/目标，在我方时间轴空白时点长按 0.5 秒放置。 |
| 后台结算 | `Script/Combat/Resolution/CombatResolver.cs` | 输入 `PlannedAction`，按 1-6 时点与优先级输出 `CombatEvent`。 |
| 演出应用 | `Script/Combat/Resolution/CombatEventApplier.cs` | 消费 `CombatEvent`，同步旧资源对象上的 HP、MP、状态与区域显示。 |

### 核心数据流

```text
LevelData
  -> CharacterData / PlayerData / EnemyData
  -> CommandExecuteInfo
  -> LegacyCommandAdapter.ToPlannedActions()
  -> CombatResolver.ResolveRound()
  -> List<CombatEvent>
  -> BattleManager.PlayCombatEvent()
  -> CombatEventApplier.ApplyToLegacyState()
```

`CommandExecuteInfo` 现在只作为“UI/AI 填写的一次行动请求 DTO”。旧的 `ExecuteInPlay()` 直接执行入口已删除，避免出现第二套结算路径。

## 目录与职责

| 路径 | 当前职责 | 后续建议 |
| --- | --- | --- |
| `Demo/Script/Combat` | GDD 战斗内核：时间轴、技能、角色快照、区域、状态、校验、结算。 | 新战斗规则优先放这里。 |
| `Demo/Script/GameLogic` | 场景级流程：战斗生命周期、关卡入口、全局引用。 | 只保留编排，不直接写伤害/位移公式。 |
| `Demo/Script/UIControl` | Godot 控件与运行时 UI 生成。 | UI 只提交行动请求，不结算战斗结果。 |
| `Demo/Data/DataScript` | Godot Resource 数据结构。 | 保持字段 UTF-8、字段顺序稳定、兼容 `.tres`。 |
| `Demo/Data` | 关卡、角色、技能、教程、成长奖励资源。 | 批量修改必须先做结构与编码检查。 |
| `Demo/Scene` | Godot 场景和 prefab。 | 场景路径变更需同步 `res://` 引用扫描。 |

## 历史命名映射

| 历史名称 | 当前 GDD 语义 | 保留原因 |
| --- | --- | --- |
| `ChessBoard` | 战斗区域显示板 / 10 区域锚点管理器 | 仍作为 Godot autoload `gd_ChessBoard` 与场景脚本引用存在，短期保留以降低场景 UID 风险。 |
| `ChessCell` | 单个区域显示锚点 | 每个锚点对应一个 `CombatAreaId`，不再表示传统棋盘格。 |
| `coord` | 旧兼容坐标 | 仅用于旧 `.tres`、旧目标结构和区域 ID 的互转。真实站位以 `CurrentAreaId` 为准。 |
| `CmdQueueUIControl` | GDD 六时点时间轴 UI | 运行时生成上方敌方时间轴、下方我方时间轴和左侧战斗交互。 |
| `CommandItemUIControl` | 时间轴槽位 | 支持空白槽长按放置、锁定、敌方意图未揭示/揭示、检视详情。 |
| `CommandExecuteInfo` | 行动请求 DTO | 由 UI/AI 创建，经 `LegacyCommandAdapter` 转为 `PlannedAction`。 |

## UI 布局约定

| 区域 | 当前职责 | 主要代码 |
| --- | --- | --- |
| 上方 `TopPanel` | 敌方时间轴，只显示敌方单位行。 | `CmdQueueUIControl.BuildSeparatedTimelines()` |
| 下方 `DownPanel` | 我方行动横幅与我方时间轴，只显示我方单位行。 | `CmdQueueUIControl.EnsureGddTimelineLayout()` |
| 左侧 `BattleInteractionPanel` | 只放战斗交互控件：`设置指令`、`检视详情`、`开始结算`、技能分类和技能列表。 | `CmdQueueUIControl.EnsureLeftInteractionHost()` / `PlayerChoseListPanelControl` |
| 右侧 `BattleInfoPanel` | 准备阶段提示、技能详情、检视详情、战斗记录与演出阶段文本。 | `CmdQueueUIControl.ShowCommandDetail()` |
| 右侧 `EnemyCHContent` | 只在需要敌方目标选择时显示，并置于右侧信息面板上方。 | `EnemyCharacterHeadListUIControl.ChangeToInteractable()` |

时间轴尺寸由 `CmdQueueUIControl` 内同一组常量统一控制，不再分别调敌方/我方容器。后续若要调整宽度、高度或字号，应优先改 `TimelineWidth`、`TimelineSlotHeight`、`TimelineInfoWidth`、`TimelineTailWidth` 等共享常量。

时间轴横向定位拆成两层：`SetTimelineHostBounds()` 让上下时间轴 host 横向铺满面板；`BuildTimelineRows()` 内左侧单位信息通过 `PlaceTimelineRowChild(..., 0f, TimelineInfoWidth)` 从屏幕/面板最左侧开始排列，槽位区通过 `PlaceCenteredTimelineRowChild()` 按屏幕中心线居中。二者只共享同一行的上下位置，左右位置互不影响。单位信息内部间隔由 `TimelineInfoTextSeparation` 控制，HP/MP 列宽在 `CreateTimelineUnitInfo()` 内的 `hpLabel.CustomMinimumSize` 与 `mpLabel.CustomMinimumSize` 调整。

## 指令选择状态机

| 状态/入口 | 责任 | 关键方法 |
| --- | --- | --- |
| 初始状态 | 左侧只保留 `设置指令`、`检视详情`；无技能分类、技能列表、目标菜单和时间轴高亮。 | `ResetCommandSelectionContext()` |
| 设置指令入口 | 重新解析仍有行动次数的我方角色，清空旧技能/旧目标，再打开技能分类面板。 | `OnSetCommandPressed()` / `ResolveCurrentPlayerForCommand()` |
| 技能与目标选择 | 只写入 `EventManager.currentMainPlayerCommand`、`currentTargetAreaId`、`moveEventInfo`、`damageEventInfo`。 | `PlayerChoseListPanelControl` / `AreaTargetMenuControl` / `EnemyCharacterHeadListUIControl` |
| 进入放置模式 | 只有当前角色存活且 `currentRestActionTimes > 0` 时，才允许点亮该角色时间轴空槽。 | `SwitchOnPlayerCommandSet()` |
| 长按提交成功 | 创建 `CommandExecuteInfo`，调用 `SetCommand()` 扣行动次数并锁定槽位，然后清理本次选择上下文。 | `CommandItemUIControl.TryPlaceCurrentCommand()` |

规范：成功提交后必须调用 `ResetCommandSelectionContext()`，让下一次行动从 `设置指令` 重新开始。失败路径不得清空上下文，例如缺少目标区域、缺少技能或槽位不可覆盖时，应保留当前选择，方便玩家修正。

角色目标规范：玩家技能不得再通过敌方头像列表选目标。`Enemy` / `Ally` / `Character` 目标统一由 `PlayerChoseListPanelControl.ShowCharacterTargetList()` 展示次级目标列表；已战败角色禁用并置灰。区域与位移目标继续走 `AreaTargetMenuControl`，自身/无目标技能直接进入时间轴放置模式。

## 本次删减

| 已移除 | 原因 |
| --- | --- |
| `Script/UIControl/MainUIControl/PlayerCharacterHeadListUIControl.cs` 及 `.uid` | 旧我方头像列表已被 GDD 下方我方时间轴取代，脚本只剩空实现。 |
| `Script/UIControl/MainUIControl/CommandHeadListUIControl.cs` 及 `.uid` | 旧混合队列头像列已被上下分离时间轴取代，脚本只剩空实现。 |
| `CommandExecuteInfo.ExecuteInPlay()` | 直接执行入口与 `CombatEvent` 后台结算架构冲突，且当前无运行时引用。 |
| 旧伤害/位移注释块与旧测试流程注释 | 避免误导后续开发者继续沿用早期原型路径。 |

## 编码与修改规范

- 所有 `.cs`、`.tres`、`.tscn`、`.md`、`.json` 均按 UTF-8 处理。
- 不要用未指定编码的 `Set-Content` / `Out-File` 重写中文数据文件。
- 修改 JSON/CSV/TSV 时必须用结构化解析器，不做正则替换结构。
- 不要为了“看起来重复”合并中文条目，GDD 中重复文本可能对应不同上下文。
- 删除文件只能一次删除一个明确路径；不得使用递归批量删除命令。

## 战斗代码规范

- 战斗结果只由 `CombatResolver` 生成的 `CombatEvent` 表达。
- UI 与 AI 不直接改 HP、MP、区域、状态，只创建行动请求。
- 区域判断统一使用 `CharacterData.CurrentAreaId` / `CharacterState.CurrentAreaId`。
- `coord` 只在兼容旧资源或旧接口时使用，新增逻辑不得以坐标作为真实站位。
- 技能标签、优先级、MP、目标类型应从 `SkillDefinition.FromCommandData()` 进入统一模型。
- 演出阶段不得重新计算命中结果，只消费准备阶段已经生成的 `pendingCombatEvents`。
- UI 文本提示不得再塞回左侧战斗交互区；左侧保持小而可点，信息与日志统一走右侧 `BattleInfoPanel`。

- 准备阶段 UI 不得绕过 `SwitchOnPlayerCommandSet()` 直接开启时间轴放置；行动次数校验必须在放置入口和长按提交前各做一次。

## 验证方式

本次重构使用以下方式验证：

```powershell
$env:APPDATA = "D:\GodotProject\202605gamejam\.godot_appdata"
$env:LOCALAPPDATA = "D:\GodotProject\202605gamejam\.godot_localappdata"
$env:NUGET_PACKAGES = "C:\Users\Lenovo\.nuget\packages"
dotnet build .\Demo\Demo.csproj --no-restore
```

结果：0 警告，0 错误。

Godot headless 主场景启动验证：

```powershell
$env:APPDATA = "D:\GodotProject\202605gamejam\.godot_appdata"
$env:LOCALAPPDATA = "D:\GodotProject\202605gamejam\.godot_localappdata"
$env:NUGET_PACKAGES = "C:\Users\Lenovo\.nuget\packages"
& "D:\Software\Godot_C\Godot_v4.6.2-stable_mono_win64_console.exe" --headless --path ".\Demo" --scene "res://Scene/MainScene.tscn" --quit-after 3
```

结果：可进入第 1 回合准备阶段与敌人准备流程；日志未发现 `ERROR`、`Exception` 或 C# 编译错误关键字。退出时仍有 Godot ObjectDB leak warning，属于既有 Godot/headless 清理告警，本次未处理。
