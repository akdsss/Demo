# Demo 最小回归清单

本清单处理 M0R 最后一项：为后续重构建立最小回归清单。它用于 M1R/M2R/M3R 修改战斗内核、数据结构或 UI 前后，快速确认现有 Demo 基础流程没有被意外打断。

依据：

- `ProjectManagement/DemoM0RInspectionReport.md`
- `ProjectManagement/DemoUIPrefabReuseReport.md`
- `GDD_Split/02_CoreLoop_Timeline.md`
- `GDD_Split/06_UI_UX.md`

当前基线仍以现有 Demo 行为为准，不要求一次性满足完整 GDD。特别注意：当前 Demo 仍是“直接执行旧 `CommandExecuteInfo.ExecuteInPlay()`”的原型流程，还不是后台 `CombatEvent` 结算后再演出的最终流程。

## 运行环境

- Godot：`D:\Software\Godot_C\Godot_v4.6.2-stable_mono_win64.exe`
- 项目路径：`D:\GodotProject\202605gamejam\Demo`
- 当前主场景：`Scene/StartMenu.tscn`
- 当前战斗场景：`Scene/MainScene.tscn`
- 当前测试关：`Data/LevelData/Level_data0.tres`，显示名“测试关”。

## 快速启动检查

用途：确认项目能被 Godot 加载，不阻塞后续人工回归。

命令：

```powershell
& "D:\Software\Godot_C\Godot_v4.6.2-stable_mono_win64.exe" --headless --path "D:\GodotProject\202605gamejam\Demo" --quit-after 3
```

预期：

- 命令能正常退出。
- 不出现 C# 编译失败、场景加载失败、资源缺失导致的启动阻塞。
- `Demo/godot.log` 可记录 Godot 启动信息。

说明：

- 该检查不能替代人工可玩流程检查，因为当前交互依赖 UI 点击。

## 人工最小回归流程

### REG-001 启动到开始菜单

步骤：

1. 用 Godot 运行 `Demo/project.godot`。
2. 等待主场景 `Scene/StartMenu.tscn` 显示。

预期：

- 显示开始菜单。
- 菜单中至少可见“开始游戏”“设置”“退出”。
- 点击“设置”可打开设置面板，关闭按钮可隐藏设置面板。

失败优先检查：

- `Demo/project.godot`
- `Demo/Scene/StartMenu.tscn`
- `Demo/Script/UIControl/StartMenuContol.cs`

### REG-002 进入测试关并加载关卡

步骤：

1. 在开始菜单点击“开始游戏”。
2. 等待切换到 `Scene/MainScene.tscn`。

预期：

- `StartMenuContol.StartGameButtonClicked()` 能切换到 `res://Scene/MainScene.tscn`。
- 顶部状态文本能进入当前战斗流程，至少不因 NodePath 断链崩溃。
- `Level_data0.tres` 被加载。

失败优先检查：

- `Demo/Scene/MainScene.tscn`
- `Demo/Script/GameLogic/GameMain.cs`
- `Demo/Data/LevelData/Level_data0.tres`
- `Demo/Script/GameLogic/SceneSingleton.cs`

### REG-003 显示角色与敌人

步骤：

1. 进入测试关后观察左侧玩家头像区、右侧敌人头像区、中部棋盘。
2. 观察下方队列头像区。

预期：

- 玩家头像列表能显示当前关卡玩家。
- 敌人头像列表能显示当前关卡敌人。
- 棋盘区域能显示玩家和敌人的头像点位。
- 下方队列头像能按当前关卡玩家/敌人顺序填入头像。
- 顶部状态文本不应持续卡在“关卡加载中”。

当前已知基线：

- 当前棋盘是 3 行共 10 格，不是 GDD 八卦区域。
- 当前头像与棋盘显示依赖 `SceneSingleton`、`ChessBoard` 和 `MainUI` 的深层 NodePath。

失败优先检查：

- `Demo/Scene/MainUI.tscn`
- `Demo/Script/UIControl/MainUIControl/PlayerCharacterHeadListUIControl.cs`
- `Demo/Script/UIControl/MainUIControl/EnemyCharacterHeadListUIControl.cs`
- `Demo/Script/UIControl/MainUIControl/CommandHeadListUIControl.cs`
- `Demo/Script/GameLogic/ChessBoard.cs`

### REG-004 放置玩家指令

步骤：

1. 选择一个玩家头像。
2. 在技能/指令面板中选择“攻击”。
3. 点击一个敌人头像作为目标。
4. 点击下方六时点指令队列中该玩家对应的一个可用槽位。
5. 再选择另一个玩家头像，选择“移动”。
6. 点击中部棋盘一个可用格子作为目标。
7. 点击下方六时点指令队列中该玩家对应的一个可用槽位。

预期：

- 选择玩家头像后，技能/指令面板显示该角色可用指令。
- 选择“攻击”后，敌人头像可被作为目标选择。
- 选择“移动”后，棋盘格按钮可被作为目标选择。
- 点击指令队列槽位后，该槽位显示指令名称并锁定为已设置状态。
- 设置完成的玩家头像行动状态变为“已行动”。

当前已知基线：

- 当前放置指令是单击，不是 GDD 要求的长按 2 秒。
- 当前未实现统一 `SkillValidator`。
- 当前已设置指令不可覆盖的规则没有完整 GDD 级校验，只能作为现有 UI 行为观察。

失败优先检查：

- `Demo/Script/UIControl/CharacterHeadButtonControl.cs`
- `Demo/Script/UIControl/MainUIControl/PlayerChoseListPanelControl.cs`
- `Demo/Script/UIControl/MainUIControl/PlayerActionButtonControl.cs`
- `Demo/Script/UIControl/MainUIControl/CommandItemUIControl.cs`
- `Demo/Script/UIControl/MainUIControl/ChessBoardUIControl/ChessCellButtonControl.cs`

### REG-005 执行一回合

步骤：

1. 继续为所有仍有行动次数的玩家设置指令，直到当前准备阶段结束。
2. 观察进入演出阶段后的命令执行。

预期：

- 当所有玩家准备完成后，`BattleManager.CheckPlayerReadyOver()` 能推进准备阶段。
- `BattleManager.PlayTurn()` 会遍历 6 个时点和所有角色。
- 已设置的攻击指令能调用伤害流程，目标 HP 显示发生变化。
- 已设置的移动指令能调用棋盘移动流程，角色头像点位发生变化。
- 回合执行过程中不应出现空引用崩溃。

当前已知基线：

- 当前没有 GDD 要求的“开始结算”按钮。
- 当前没有后台 `CombatEvent` 列表。
- 当前演出阶段直接调用 `CommandExecuteInfo.ExecuteInPlay()` 改变战斗结果。
- 当前敌人准备固定使用 `enemyCommandDataArray[0]` 放入第 1 个时点。

失败优先检查：

- `Demo/Script/GameLogic/BattleManager.cs`
- `Demo/Data/DataScript/CommandData.cs`
- `Demo/Data/DataScript/CharacterData.cs`
- `Demo/Script/GameLogic/EventManager.cs`
- `Demo/Script/GameLogic/ChessBoard.cs`

## 通过标准

本最小回归清单通过，需要同时满足：

- REG-001 到 REG-005 都能完成。
- 没有启动级、加载级、空引用级阻塞错误。
- 玩家至少能设置一次攻击或移动。
- 至少能进入一次当前旧演出阶段。
- 执行一回合后，能观察到 HP 或棋盘点位变化之一。

## 失败记录模板

```text
日期：
检查项：
失败步骤：
实际表现：
Godot 输出/日志摘录：
疑似相关文件：
是否阻塞 M1R/M2R：
备注：
```

## 后续自动化建议

- M1R 引入 `Timeline` / `PlannedAction` 后，可把 REG-004 的“放置玩家指令”拆成数据层测试。
- M2R 引入 `CombatEvent` 后，可把 REG-005 的“执行一回合”改成结算器输入/输出测试。
- M3R 改造 UI 后，应保留 REG-001 到 REG-003 作为启动与显示烟测，同时把 REG-004 更新为长按 2 秒放置指令。
