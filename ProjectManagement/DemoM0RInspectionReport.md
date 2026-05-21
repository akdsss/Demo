# Demo M0R 检查报告

本报告覆盖 `TaskBoard.md` 中 M0R 的三项检查：

- 检查 C# 文件是否存在真实编码损坏，区分终端显示乱码和文件内容损坏。
- 检查现有 `.tres` 数据字段，列出可复用字段与 GDD 缺失字段。
- 检查 `Scene/MainScene.tscn` 和 `Scene/MainUI.tscn` 的节点引用关系。

依据文档：

- `GDD_Split/01_DesignReview.md`
- `GDD_Split/03_CombatRules_Resolution.md`
- `GDD_Split/05_Content_Data.md`
- `GDD_Split/06_UI_UX.md`
- `ProjectManagement/DemoStructureMapping.md`

本次只读检查 `Demo`，未修改 `Demo` 代码、场景或资源。

## 1. C# 编码检查

检查范围：

- `Demo/Data/DataScript/**/*.cs`
- `Demo/Script/**/*.cs`
- 不计入 `Demo/.godot/mono/temp` 下的 Godot 生成临时代码。

检查方法：

- 使用严格 UTF-8 解码读取字节，解码失败即判定为真实编码问题。
- 检查 UTF-8 BOM。
- 检查常见乱码特征：`�`、`锟斤拷`、`Ã`、`閫`、`銆`、`â€™`、`â€œ`、`â€`。
- 终端输出前显式设置 UTF-8 输出编码，避免 PowerShell 显示链路造成误判。

结果：

| 项目 | 结果 |
| --- | --- |
| 项目 C# 文件数 | 33 |
| 严格 UTF-8 解码失败 | 0 |
| UTF-8 BOM | 0 |
| 常见乱码特征命中 | 0 |

结论：

- 当前项目 C# 文件没有发现真实编码损坏。
- 此前出现的中文乱码风险应按文档判断为终端/控制台显示链路问题，而不是文件内容损坏。
- 后续若再次看到乱码，应继续先做字节级 UTF-8 检查，再判断是否需要修复文件。

## 2. `.tres` 数据字段检查

检查范围：

- `Demo/Data/**/*.tres`

当前 `.tres` 数量：

- 数据资源：20 个。
- UI 按钮组资源：2 个。
- 总计：22 个。

### 2.1 当前字段汇总

| 资源类型 | 数量 | 当前字段 |
| --- | ---: | --- |
| `PlayerData` | 3 | `characterId`、`characterName`、`characterDescription`、`characterHeadImage`、`hp`、`maxHp`、`atk`、`turnInitialActionTimes`、`playerCommandDataList` |
| `EnemyData` | 2 | `characterId`、`characterName`、`characterDescription`、`characterHeadImage`、`hp`、`maxHp`、`atk`、`turnInitialActionTimes`、`enemyCommandDataArray` |
| `PlayerCommandData` | 4 | `commandId`、`commandName`、`commandDescription` |
| `EnemyCommandData` | 4 | `commandId`、`commandName`、`commandDescription`、`enemyCommandName` |
| `LevelData` | 1 | `levelName`、`enemyInfoInLevelArray`、`playerInfoInLevelArray` |
| `PlayerInfoInLevel` | 3 | `playerData`、`coord` |
| `EnemyInfoInLevel` | 3 | `enemyData`、`coord` |
| `ButtonGroup` | 2 | `allow_unpress` |

说明：

- `CommandData.cs` 已导出 `priority` 和 `allCommandParams`，但当前 `.tres` 基本未显式保存这些字段，可能仍为默认值。
- `LevelData.cs` 已导出 `levelId` 和 `levelType`，但当前 `Level_data0.tres` 未显式保存，可能仍为默认值。
- `enm_cmd2.tres` 中存在 `enemyCommandName`，但 `EnemyCommandData.cs` 里对应导出枚举目前被注释。按数据保护规则，这类旧字段不能自动删除。
- `enm_info_level2.tres` 当前只保存 `coord`，未保存 `enemyData`；它未被当前 `Level_data0.tres` 引用，但后续清理前仍需确认用途。

### 2.2 可复用字段

角色与敌人：

- `characterId` 可作为现有数字 ID。
- `characterName` 可映射到 GDD 的中文显示名 `displayName`。
- `characterDescription` 可复用为描述或百科短描述来源。
- `characterHeadImage` 可复用为头像资源引用。
- `maxHp`、`atk` 可复用为生命值与攻击力基础数值。
- `turnInitialActionTimes` 可复用为 GDD 的 `actionsPerRound`。
- `playerCommandDataList`、`enemyCommandDataArray` 可作为现有技能/指令引用关系。

技能/指令：

- `commandId` 可作为旧系统兼容 ID。
- `commandName` 可映射到技能中文显示名。
- `commandDescription` 可复用为技能说明。
- `priority` 已在脚本中存在，可用于 GDD 技能优先级，但数据尚未填写。
- `allCommandParams` 可临时承载数值参数，但不适合长期表达结构化效果。

关卡：

- `levelName` 可映射到关卡显示名。
- `playerInfoInLevelArray`、`enemyInfoInLevelArray` 可复用为初始队伍配置。
- `coord` 可作为当前棋盘坐标站位；八卦区域实现前可作为兼容层输入。

UI 资源：

- `ButtonGroup` 可继续用于玩家/敌人头像按钮互斥选择。

### 2.3 GDD 缺失字段

角色数据缺失：

- 稳定英文字符串 `id`。
- `role`。
- `portraitPath` 字符串路径。
- `maxMp` 与 MP 当前值/恢复规则。
- `initialArea` 八卦区域。
- `skillIds` 字符串 ID 列表。
- `growthTrackId`。

敌人数据缺失：

- 稳定英文字符串 `id`。
- `enemyType`。
- `initialArea` 八卦区域。
- `skillIds` 字符串 ID 列表。
- `aiProfileId`。
- `portraitPath` 字符串路径。

技能数据缺失：

- 稳定英文字符串 `id`。
- `tags`：近战、远程、移动、防御、单体、范围、治疗等。
- `mpCost`。
- `targetType`。
- `rangeRule`。
- `durationSlots` 或 `duration`。
- 结构化 `effects` / `effectList`。
- `failTextKey`。
- `unlockCondition`。
- `aiWeight`。

关卡数据缺失：

- `chapterIndex`。
- `enemyGroups`。
- `playerStartAreas`。
- `enemyStartAreas`。
- `tutorialStepIds`。
- `rewardId`。
- `winCondition`。
- `loseCondition`。

整类数据缺失：

- `statuses`。
- `areas`。
- `growth_rewards`。
- `tutorial_steps`。
- `localization.zh-cn`。
- `assets_manifest`。
- `ai_profiles`。
- `ai_behaviors`。

结论：

- 当前 `.tres` 足够支撑现有测试关原型：角色、敌人、指令、关卡和坐标站位。
- 当前数据还不足以直接支撑 GDD 的完整时间轴战斗、技能合法性检查、八卦区域、状态、教程、百科、成长和 AI 行为表。
- M1R/M2R 应优先做兼容层：保留旧 `CommandData`、`CharacterData`、`LevelData`，同时引入 GDD 所需新模型。

## 3. 主场景节点引用关系检查

检查范围：

- `Demo/Scene/MainScene.tscn`
- `Demo/Scene/MainUI.tscn`

### 3.1 文件规模

| 场景 | 节点数 | 外部资源 | 信号连接 | editable path |
| --- | ---: | ---: | ---: | ---: |
| `MainScene.tscn` | 11 | 8 | 0 | 10 |
| `MainUI.tscn` | 206 | 23 | 2 | 9 |

### 3.2 `MainScene.tscn`

关键外部资源：

- `res://Script/GameLogic/GameMain.cs`
- `res://Scene/MainUI.tscn`
- `res://Data/LevelData/Level_data0.tres`
- `res://Script/GameLogic/SceneSingleton.cs`
- 若干头像图片资源。

关键节点关系：

- 根节点 `MainScene` 挂载 `GameMain.cs`，并通过 `levelData` 引用 `Level_data0.tres`。
- 子节点 `SceneSingleton` 挂载 `SceneSingleton.cs`。
- `SceneSingleton.gameStateLable` 指向 `../MainUi/Panel/VBoxContainer/TopPanel/Label`。
- `SceneSingleton.playerActionChoseList` 指向 `../MainUi/Panel/VBoxContainer/MidPanel/HBoxContainer/LeftRegion/ChoseListPanel`。
- `SceneSingleton.defaultCharacterImage` 引用 `unknown_head.png`。
- 子节点 `MainUi` 是 `MainUI.tscn` 的实例。
- `GameTest` 位于 `MainUi/Panel/VBoxContainer/TopPanel` 下，`gameMain` 指回场景根节点，`levelData` 同样引用 `Level_data0.tres`。
- `MainScene.tscn` 对 `MainUI` 实例中的部分头像、敌人头像区域和六个指令列设置了 editable path。

结论：

- `MainScene` 主要承担启动当前测试关、挂接全局场景引用、实例化主 UI 的职责。
- 它与 `MainUI` 强耦合：`SceneSingleton` 的导出 NodePath 直接指向 `MainUI` 内部深层节点。
- 后续重构 UI 层级时，必须同步更新 `SceneSingleton` 的导出引用，否则准备阶段 UI 会断链。

### 3.3 `MainUI.tscn`

关键外部资源：

- `MainUIControl.cs`
- `PlayerCharacterHeadListUIControl.cs`
- `PlayerChoseListPanelControl.cs`
- `ChessBoardUIControl.cs`
- `ChessCellUIControl.cs`
- `CmdQueueUIControl.cs`
- `EnemyCharacterHeadListUIControl.cs`
- `CommandHeadListUIControl.cs`
- `CharacterHeadButton.tscn`
- `ActionButton.tscn`
- `ChessCellButton.tscn`
- `action_listt.tscn`
- `enemy_character_head_button.tscn`
- `CmdHeadPrefab.tscn`
- 头像与 UI 图片资源。

关键节点关系：

- 根节点 `MainUi` 挂载 `MainUIControl.cs`，`SetPanel` 指向 `Panel/SetPanel`。
- 顶部设置按钮连接到 `SetPanelOpenButtonClicked()`。
- 设置面板关闭按钮连接到 `SetPanelCloseButtonClicked()`。
- `PlayerCHContainer` 挂载 `PlayerCharacterHeadListUIControl.cs`，`playerHeadButtonPrefab` 指向 `CharacterHeadButton.tscn`。
- `ChoseListPanel` 挂载 `PlayerChoseListPanelControl.cs`，`choseButtonPrefab` 指向 `ActionButton.tscn`，`allChoseContent` 指向内部 `ColorRect/VBoxContainer`。
- `ChessBoard` 挂载 `ChessBoardUIControl.cs`，`chessCellUIControlArray` 指向 10 个格子节点：`ChessCell`、`Control2` 到 `Control10`。
- 每个棋盘格挂载 `ChessCellUIControl.cs`，并持有 9 个角色显示点和一个 `ChessCellButton` 引用。
- `EnemyCHContent` 挂载 `EnemyCharacterHeadListUIControl.cs`，`enemyHeadButtonPrefab` 指向 `enemy_character_head_button.tscn`。
- 下方面板中的 `HBoxContainer` 挂载 `CmdQueueUIControl.cs`，`CommandQueueMatrix` 指向 `CommandMatrix`，`commandListPrefab` 指向 `action_listt.tscn`。
- `HeadList` 挂载 `CommandHeadListUIControl.cs`，`characterHeadPrefab` 指向 `CmdHeadPrefab.tscn`。
- `CommandMatrix` 下有 6 个 `CommandColumn` 实例，对应当前六长度指令队列。

结论：

- `MainUI` 已有可复用战斗 UI 原型：玩家头像区、技能选择面板、棋盘格、敌人头像区、六时点指令队列。
- 当前时间轴仍是一个 6 列矩阵，按角色/敌人头像行显示；尚未拆成 GDD 要求的我方/敌方双时间轴。
- 当前棋盘是 10 个格子，不是 GDD 的八卦区域模型。
- 当前只连接了设置面板按钮，战斗交互主要依靠 prefab 自身脚本和运行时动态绑定。
- 当前场景没有发现需要立即修复的引用断链迹象；但后续修改 `MainUI` 层级时风险较高，应先记录引用清单再改。

## 4. 后续建议

- 下一步 M0R 的 UI prefab 复用检查可以基于本报告中的 `MainUI` 资源和 prefab 引用继续做。
- M1R 开始前，不建议搬动 `Scene/MainUI.tscn` 的深层节点路径。
- 若要引入 `BattleStateViewModel` 或 `CombatEvent` UI 消费层，应优先减少 `SceneSingleton` 对深层 UI 节点的直接引用。
- `.tres` 字段扩展前应先定义兼容策略，避免直接把旧 `CommandData` 改成复杂 GDD 技能数据导致现有原型断裂。
