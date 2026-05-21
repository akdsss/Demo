# Demo UI Prefab 复用报告

本报告处理 `TaskBoard.md` 中 M0R Todo：明确哪些现有 UI prefab 可继续复用。

依据：

- `ProjectManagement/DemoM0RInspectionReport.md`
- `GDD_Split/06_UI_UX.md`

本次只读检查 `Demo/Scene/UI`、`Demo/Scene/PrefabScene` 和相关 UI 控制脚本，未修改 `Demo` 代码、场景或资源。

## 结论总览

| Prefab / 场景 | 当前主 UI 是否使用 | 复用结论 | 主要用途 |
| --- | --- | --- | --- |
| `Demo/Scene/UI/CharacterHeadButton.tscn` | 是 | 继续复用，需改造脚本接口 | 玩家头像、HP、行动次数、选中态 |
| `Demo/Scene/PrefabScene/enemy_character_head_button.tscn` | 是 | 继续复用，建议后续与玩家头像 prefab 合流 | 敌人头像、HP、行动/意图状态 |
| `Demo/Scene/UI/ActionButton.tscn` | 是 | 继续复用，需扩展 hover 与合法性状态 | 技能列表按钮 |
| `Demo/Scene/PrefabScene/CommandItem.tscn` | 是 | 重点复用，需承担时间轴槽位改造 | 六时点槽位、指令锁定、未揭示/已揭示 |
| `Demo/Scene/PrefabScene/action_listt.tscn` | 是 | 可复用为过渡容器，后续需重命名和重排 | 当前六列指令队列的列容器 |
| `Demo/Scene/PrefabScene/CmdHeadPrefab.tscn` | 是 | 继续复用，低风险 | 时间轴行头像/队列头像 |
| `Demo/Scene/PrefabScene/ChessCellButton.tscn` | 是 | 可短期复用，八卦区域实现时需替换语义 | 棋盘目标选择按钮 |
| `Demo/Scene/PrefabScene/chess_cell.tscn` | 否 | 暂不直接复用 | 未接入主 UI，导出 NodePath 不完整 |
| `Demo/Scene/MainUI.tscn` | 是 | 继续作为战斗 UI 原型容器，不宜大搬动 | 主战斗 UI 组装场景 |

## 可继续复用的 prefab

### `CharacterHeadButton.tscn`

当前作用：

- 由 `PlayerCharacterHeadListUIControl` 动态生成玩家头像。
- 显示头像、行动状态、行动次数、HP 条、HP 数字和选中箭头。
- 通过 `CharacterHeadButtonControl` 与 `PlayerData` / `EnemyData` 交互。

复用价值：

- 符合 GDD 战斗记录、角色列表、成长界面对头像与状态显示的基础需求。
- 已有按钮组、选中态、行动次数显示和 HP 显示。

需改造点：

- 当前脚本直接依赖 `Autoloads.sceneSingleton` 和 `BattleManager.eventManager`，后续应改为接收 view model 或请求事件。
- 当前行动状态只有“待命/行动/已行动”，需扩展为 GDD 的“还剩 X 次”“思考中”“完成”等显示。
- 后续需要支持状态图标、MP、禁用原因或 tooltip。

结论：

- 继续复用，作为玩家角色头像 prefab 的基础。

### `enemy_character_head_button.tscn`

当前作用：

- 由 `EnemyCharacterHeadListUIControl` 动态生成敌方头像。
- 与玩家头像使用同一 `CharacterHeadButtonControl`，但默认关闭交互。

复用价值：

- 可继续显示敌人头像、HP、行动状态。
- 可作为敌方行动未揭示/已揭示 UI 的入口之一。

需改造点：

- 当前敌方目标选择和玩家头像逻辑混在同一控制脚本中。
- GDD 需要敌方意图未揭示/已揭示两种显示状态，当前 prefab 还没有技能标签、目标、优先级显示空间。

结论：

- 继续复用，但建议后续把头像显示基础组件与玩家/敌人交互逻辑拆开。

### `ActionButton.tscn`

当前作用：

- `PlayerChoseListPanelControl` 用它动态生成玩家技能/指令按钮。
- 点击后调用 `PlayerCommandData.UIButtonClick()`，再设置当前玩家指令。

复用价值：

- 可作为 GDD 技能列表的最小按钮基础。
- 已能由数据驱动显示技能名。

需改造点：

- 当前只处理点击，不处理 hover 详情。
- 需要新增优先级、MP、目标类型、效果说明、合法/非法状态显示。
- 不应长期由按钮直接调用 `PlayerCommandData.UIButtonClick()`；后续应通过 UI 请求对象进入统一合法性检查。

结论：

- 继续复用为技能按钮基础，但 M3R 需要扩展为完整技能列表项。

### `CommandItem.tscn`

当前作用：

- 作为指令队列中的单个槽位。
- 支持 `NORMAL`、`DISABLE`、`HIGHLIGHT`、`LOCKED` 四种视觉状态。
- 鼠标进入/退出时可高亮，点击后把当前玩家指令写入角色 `commandQueue`。

复用价值：

- 与 GDD 六时点槽位最接近，是最值得复用的 UI prefab。
- 已有锁定态、默认文本、hover 高亮和点击事件。

需改造点：

- GDD 要求空白时点长按 2 秒放置并显示进度条，当前是单击。
- GDD 要求已设置指令不可覆盖，当前需要补显式校验与禁用反馈。
- GDD 要求敌方行动未揭示/已揭示状态，当前没有意图遮罩或标签显示。
- 当前点击逻辑直接写 `CommandExecuteInfo`，后续需要改为提交 `PlayerActionRequest` 或 `PlannedAction`。

结论：

- 重点复用为时间轴槽位 prefab，但交互脚本需要中等规模改造。

### `action_listt.tscn`

当前作用：

- 当前六列指令队列的列容器。
- `CmdQueueUIControl` 以它为 `commandListPrefab`，生成 6 个列。
- 内部 `ActionListUIControl` 会按 `gameCharacterNum` 动态生成 `CommandItem`。

复用价值：

- 可继续作为“一个时点内多个行动行”的过渡容器。
- 与当前六长度队列初始化流程兼容。

需改造点：

- 文件名 `action_listt` 有拼写问题，短期不改名以免破坏引用。
- GDD 目标是我方/敌方各自时间轴，当前容器是按角色行混排。
- 后续可能需要改为 `TimelineColumn` / `TimelineTrack` 语义。

结论：

- 可短期复用为时间轴容器；M3R 改造时建议保留引用，逐步替换语义。

### `CmdHeadPrefab.tscn`

当前作用：

- 作为 `CommandHeadListUIControl` 生成队列行头像的基础 `TextureRect`。

复用价值：

- 简单、低耦合，可继续用于时间轴头像、战斗记录头像、敌我轨道标识。

需改造点：

- 如需显示死亡、禁用、行动次数等状态，需要加遮罩或图标层。

结论：

- 继续复用，低风险。

### `ChessCellButton.tscn`

当前作用：

- 嵌在 `MainUI.tscn` 的 10 个棋盘格里。
- 点击后写入 `moveEventInfo.moveTargetCoord` 并开启指令队列放置。

复用价值：

- 可继续用于当前棋盘/区域目标选择。
- 已有点击信号和坐标导出字段。

需改造点：

- 当前语义是棋盘坐标 `Vector2I coord`，GDD 目标是八卦区域 `AreaDefinition`。
- 当前点击后直接进入指令队列设置，后续应先走 `SkillValidator`。

结论：

- 短期复用为目标选择按钮；八卦区域系统落地时需要改成区域按钮或区域热点。

## 暂不直接复用的 prefab

### `chess_cell.tscn`

当前状态：

- 目录中存在，但 `MainUI.tscn` 当前没有引用它。
- 它挂载 `ChessCellUIControl.cs`，但 `allCharacterPointArray` 为空 NodePath 列表，且没有看到完整的 `chessCellButton` 导出绑定。

风险：

- 直接接入可能导致节点引用不完整。
- 当前主 UI 已经内嵌了 10 个棋盘格结构，运行流程实际依赖内嵌版本。

结论：

- 暂不直接复用。后续若要统一棋盘格 prefab，应先修复导出 NodePath，再替换 `MainUI.tscn` 中的内嵌棋盘格。

## 主 UI 容器复用建议

`Demo/Scene/MainUI.tscn` 不是普通小 prefab，但当前应继续作为战斗 UI 原型容器使用。

可保留部分：

- 左侧玩家头像与技能选择面板。
- 中部棋盘显示区。
- 右侧敌人头像区。
- 下方六时点指令队列。
- 设置面板开关逻辑。

需补充部分：

- 上方敌方时间轴与下方我方时间轴的明确分区。
- 开始结算按钮。
- 检视详情入口。
- 技能 hover 详情。
- 敌方行动未揭示/已揭示显示。
- 战斗记录区。
- 百科入口。

不建议立即做的事：

- 不建议立刻重排或重命名 `MainUI.tscn` 深层节点，因为 `MainScene.tscn` 和多个控制脚本已有深层 NodePath / 单例引用。
- 不建议为了命名美观立刻重命名 `action_listt.tscn`，应等引用迁移策略明确后再做。

## M3R 复用优先级

1. 优先复用 `CommandItem.tscn` 和 `action_listt.tscn`，改造为六时点时间轴槽位和容器。
2. 继续复用 `CharacterHeadButton.tscn`、`enemy_character_head_button.tscn` 和 `CmdHeadPrefab.tscn`，支撑角色/敌人状态显示。
3. 继续复用 `ActionButton.tscn`，扩展为技能列表项与 hover 详情入口。
4. 短期复用 `ChessCellButton.tscn`，等八卦区域数据模型确定后再切换语义。
5. 暂缓使用 `chess_cell.tscn`，除非先补齐导出引用并验证。
