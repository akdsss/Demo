# Demo 结构映射表

本表依据 `GDD_Split/07_GodotProjectStructure.md` 和当前 `Roadmap.md` 的结构策略整理。它是逻辑映射表，不代表立即迁移目录。

当前原则：

- 保留现有 `Demo/Asset`、`Demo/Data`、`Demo/Scene`、`Demo/Script`，避免破坏 `.tscn`、`.tres` 和 `project.godot` 中已有 `res://` 引用。
- 后续新增代码优先在现有 `Demo/Script` 下建立内部分层，例如 `Script/Combat`、`Script/Data`、`Script/Presentation`、`Script/Progression`。
- 等核心接口稳定后，再评估是否从单数目录迁移到推荐的 `Art`、`Scenes`、`Scripts`。
- 本次只记录映射，不移动、不删除、不批量重写文件。

## 当前结构到推荐结构

| 当前 Demo 路径 | 推荐结构对应 | 当前内容 | 处理策略 |
| --- | --- | --- | --- |
| `Demo/project.godot` | 项目根配置 | 主场景、autoload、C# assembly、插件配置 | 保留在项目根；迁移目录前必须先评估所有 `res://` 引用。 |
| `Demo/Demo.csproj`、`Demo/Demo.sln` | 项目根配置 | Godot C# 工程文件 | 保留在项目根；不纳入运行期目录迁移。 |
| `Demo/Asset/character` | `Art/Characters/Player`、`Art/Characters/Enemies`、`Art/UI/Portraits` | 玩家、敌人、默认头像和角色图 | 短期复用；后续按玩家、敌人、头像类型拆分，路径变化需同步 `.tres` 引用。 |
| `Demo/Asset/ui image` | `Art/UI/Icons`、`Art/UI/Panels` | 按钮、关闭、播放、矩形底图等 UI 图片 | 短期复用；后续建议避免带空格目录名，但不立即改名。 |
| `Demo/Asset/board.png`、`Demo/Asset/map.png` | `Art/Backgrounds`、`Art/UI/Panels` 或 `Scenes/Battle` 依赖资源 | 棋盘/地图占位图 | 短期作为战斗区域占位资源；八卦区域落地前不迁移。 |
| `Demo/Data/DataScript` | `Scripts/Data` | `CharacterData`、`CommandData`、`LevelData` 等 Godot Resource 类 | 短期保留；后续新增 schema、加载和校验代码时移入或镜像到 `Script/Data`。 |
| `Demo/Data/CharacterData/PlayerData` | `Data/Content` | 玩家角色 `.tres` | 复用为角色内容数据；后续补 GDD 字段如角色定位、MP、初始区域、成长轨。 |
| `Demo/Data/CharacterData/EnemyData` | `Data/Content` | 敌人 `.tres` | 复用为敌人内容数据；后续补 enemyType、aiProfileId、技能列表等字段。 |
| `Demo/Data/CommandData/PlayerCommandData` | `Data/Combat` | 玩家指令 `.tres` | 复用为技能数据雏形；后续与 `SkillDefinition` 兼容，补标签、目标类型、MP、失败原因等字段。 |
| `Demo/Data/CommandData/EnemyCommandData` | `Data/Combat`、`Data/Content` | 敌人指令 `.tres` | 短期保留；后续技能本体进入 Combat，敌人使用关系进入 Content/AI。 |
| `Demo/Data/LevelData` | `Data/Levels` | 关卡、玩家初始位置、敌人初始位置 `.tres` | 复用为关卡配置雏形；后续补 winCondition、loseCondition、rewardId、tutorialStepIds。 |
| `Demo/Data/other` | `Scenes/UI` 依赖资源或 `Data/UI` | 头像按钮组等 UI 资源 | 保留；后续若 UI prefab 固化，可随 UI 资源统一整理。 |
| `Demo/Scene/StartMenu.tscn` | `Scenes/Boot` | 开始菜单 | 复用为启动入口；后续可作为 Boot 层场景。 |
| `Demo/Scene/MainScene.tscn` | `Scenes/Battle` | 当前战斗主场景，挂载 `GameMain`、`SceneSingleton`、`MainUI` | 复用为战斗场景雏形；后续拆出 Battle、Boot、Level 装配职责。 |
| `Demo/Scene/MainUI.tscn` | `Scenes/UI` | 当前战斗主 UI | 复用为战斗 UI 原型；后续承接时间轴、技能列表、战斗记录、百科入口。 |
| `Demo/Scene/Character/BaseCharacter.tscn` | `Scenes/Characters` | 角色表现基础场景 | 复用；后续由演出系统或角色表现层接管。 |
| `Demo/Scene/PrefabScene` | `Scenes/UI`、`Scenes/Battle` | 指令槽、头像、棋盘格、行动列表等 prefab | 复用；后续按 UI prefab 与战斗区域 prefab 拆分。 |
| `Demo/Scene/UI` | `Scenes/UI` | `ActionButton`、`CharacterHeadButton` | 复用；后续统一命名与职责。 |
| `Demo/Script/GameLogic` | `Scripts/Core`、`Scripts/Combat`、`Scripts/Presentation` | `BattleManager`、`ChessBoard`、`GameMain`、`SceneSingleton`、`EventManager` | 短期不移动；后续逐步拆出 Combat 内核、Core 启动状态、Presentation 事件消费。 |
| `Demo/Script/UIControl` | `Scripts/UI` | 主 UI、头像、指令队列、棋盘格 UI 控制脚本 | 复用为 UI 层；后续通过 view model 或请求对象减少对全局单例的直接耦合。 |
| `Demo/Script/GeneralTool` | `Scripts/Core`、`Scripts/Utilities` | `Autoloads`、`PubTool` | 短期保留；后续整理为全局服务、工具函数或调试辅助。 |
| `Demo/addons/godot_mcp` | `addons` | Godot MCP 插件 | 第三方/工具插件，保留原位置。 |
| `Demo/Properties` | 项目根工具配置 | C# launch settings | 保留；不纳入游戏内容结构。 |
| `Demo/.godot`、`Demo/.vscode` | 编辑器/本地配置 | Godot 导入缓存、编辑器配置 | 保留或按团队规范处理；不作为游戏结构迁移对象。 |

## 推荐结构覆盖状态

| 推荐结构 | 当前覆盖情况 | 后续动作 |
| --- | --- | --- |
| `Art` | 由 `Demo/Asset` 部分覆盖 | 先复用现有图片；新增水墨风资源时可在 `Asset` 下建立类型子目录，稳定后再考虑迁移。 |
| `Audio` | 暂无对应目录 | GDD 暂不考虑 BGM 和攻击/受击音效；仅需为 UI SFX 预留接口。 |
| `Data/Combat` | 由 `Data/CommandData` 部分覆盖 | M1R 起定义 `SkillDefinition`、状态、区域、公式参数。 |
| `Data/Content` | 由 `Data/CharacterData` 部分覆盖 | 后续补角色、敌人、AI profile 字段。 |
| `Data/Levels` | 由 `Data/LevelData` 部分覆盖 | 后续扩展三关配置、教程步骤引用和成长奖励引用。 |
| `Data/Localization` | 暂无对应目录 | 后续失败原因、UI 文案、战斗记录文本应逐步集中。 |
| `Data/Tutorials` | 暂无对应目录 | M4R 建立教程步骤数据结构时新增。 |
| `Docs` | 当前由 `GDD_Split` 与 `ProjectManagement` 覆盖 | 保持现状；无需为迁移而复制文档。 |
| `Scenes/Battle` | 由 `Scene/MainScene.tscn` 和部分 `Scene/PrefabScene` 覆盖 | 后续拆出战斗主场景、战场区域、时间轴。 |
| `Scenes/Characters` | 由 `Scene/Character` 覆盖 | 继续复用。 |
| `Scenes/UI` | 由 `Scene/MainUI.tscn`、`Scene/UI`、部分 `Scene/PrefabScene` 覆盖 | M3R 复用现有 UI prefab 改造。 |
| `Scenes/Levels` | 暂无明确对应目录 | 三关 demo 关卡入口或组装场景稳定后再新增。 |
| `Scenes/Boot` | 由 `Scene/StartMenu.tscn` 覆盖 | 继续作为启动入口。 |
| `Scripts/Core` | 由 `Script/GameLogic`、`Script/GeneralTool` 部分覆盖 | 后续保留启动、状态、服务入口相关代码。 |
| `Scripts/Combat` | 由 `Script/GameLogic/BattleManager.cs`、`ChessBoard.cs`、`Data/DataScript/CommandData.cs` 部分覆盖 | M1R 新增模型时优先在 `Script/Combat` 下规划，不删除旧流程。 |
| `Scripts/Data` | 由 `Data/DataScript` 部分覆盖 | 后续数据加载和 schema 校验可新增到 `Script/Data`，旧 Resource 类暂留。 |
| `Scripts/UI` | 由 `Script/UIControl` 覆盖 | 继续复用并逐步解耦。 |
| `Scripts/Presentation` | 暂无明确对应目录 | M2R/M6R 接入 `CombatEvent` 消费和占位播片时新增。 |
| `Scripts/Progression` | 暂无明确对应目录 | M5R 成长奖励和技能解锁时新增。 |
| `Scripts/Utilities` | 由 `Script/GeneralTool` 部分覆盖 | 后续整理通用工具。 |
| `Shaders` | 暂无对应目录 | 当前不阻塞。 |
| `Tests` | 暂无对应目录 | 战斗结算与数据加载稳定后新增。 |
| `Tools` | 暂无对应目录 | 编码检查、JSON/schema guard 建立时新增。 |

## 迁移注意事项

- 当前大量场景和资源使用 `res://Asset`、`res://Data`、`res://Scene`、`res://Script`。任何实际移动都需要先列出引用清单，再分批验证。
- `.tres` 数据中的脚本引用和资源引用不能用简单文本替换批量处理；如需迁移，应使用 Godot 或专用工具验证。
- 新增 M1R 代码时，建议采用现有单数目录下的目标子目录，例如 `Demo/Script/Combat/Timeline`，以匹配 Roadmap 的“兼容现状 + 内部分层”策略。
- 当前 `Demo/Asset/ui image` 包含空格，长期不理想；短期不改名，避免破坏引用。
- 本映射表不授权删除任何旧目录或文件。
