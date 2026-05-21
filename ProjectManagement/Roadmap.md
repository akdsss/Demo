# Roadmap

## 项目目标

目标平台为 Windows。当前目标是基于 `Demo` 现有 Godot C# 项目继续开发，完成 `generalGDDforAI.md` 与 `GDD_Split` 中的核心要求，制作一个包含 3 个关卡的可玩 demo。

## 已有基础

`Demo` 中已经存在一部分可复用内容：

- Godot 4.6 C# 项目骨架：`project.godot`、`Demo.csproj`、`Demo.sln`。
- 主场景与 UI 场景：`Scene/MainScene.tscn`、`Scene/MainUI.tscn`、`Scene/StartMenu.tscn`、若干 UI prefab。
- 基础脚本结构：`Script/GameLogic`、`Script/UIControl`、`Script/GeneralTool`。
- 数据资源结构：`Data/DataScript`、`Data/CharacterData`、`Data/CommandData`、`Data/LevelData`。
- 基础战斗流程：`BattleManager` 已有 BattleStart、TurnStart、PrepareTurn、PlayTurn、胜负检查雏形。
- 六长度指令队列：`SceneSingleton.gameQueueLength = 6`，和 GDD 六时点方向一致。
- 棋盘与站位 UI：`ChessBoard` 与 `ChessBoardUIControl` 已能放置角色显示。
- 基础角色、敌人、命令、关卡 `.tres` 资源。
- 部分角色/敌人头像和 UI 图片资源。

## 关键差距

现有 Demo 更接近“棋盘 + 指令队列 + 基础行动 UI 原型”，还不是 GDD 的完整时间轴战斗系统。主要差距：

- 目录命名为 `Asset/Scene/Script`，与推荐结构 `Art/Scenes/Scripts` 不一致；但已有大量引用，暂不做一次性迁移。
- 棋盘当前是 3 行共 10 格，不是明确的八卦区域数据模型。
- 结算直接在 `PlayTurn()` 中执行命令，尚未做到“准备阶段后台结算，演出阶段只播放事件”。
- 缺少独立 `Timeline`、`PlannedAction`、`SkillValidator`、`CombatEvent`。
- 技能使用 `commandId` 和 switch 执行，缺少 GDD 所需标签、目标类型、失败原因、MP、持续/延迟等字段。
- `DataManager` 目前为空，数据加载和校验尚未形成体系。
- UI 与战斗逻辑通过全局单例直接耦合，后续并行开发容易互相影响。
- 教程、百科、成长、三关流程尚未实现。

## 结构策略

不立刻大规模搬目录。先采用“兼容现状 + 内部分层”的方式：

- 保留 `Demo/Asset`，新增子目录映射美术类型，例如 `Asset/Generated`、`Asset/CutscenePlaceholders`、`Asset/UI`。
- 保留 `Demo/Scene`，新增或整理 `Scene/Battle`、`Scene/UI`、`Scene/Levels`。
- 保留 `Demo/Script`，新增或整理 `Script/Combat`、`Script/Data`、`Script/Presentation`、`Script/Progression`。
- 保留现有 `Data` 资源，但逐步增加 GDD 需要的数据字段和校验工具。
- 当接口稳定后，再评估是否从单数目录迁移到 `Art/Scenes/Scripts`。

## 路线

### M0R 现有 Demo 审计与稳定化

确认现有项目可运行边界，记录可复用模块，清理开发路线。只做低风险整理，不迁移大量资源。

### M1R 战斗内核重构准备

在不破坏现有 UI 原型的前提下，引入 GDD 需要的核心模型：`Timeline`、`PlannedAction`、`SkillDefinition`、`SkillValidator`、`CombatEvent`。

### M2R 结算与演出分离

把现有 `CommandExecuteInfo.ExecuteInPlay()` 的直接执行逻辑迁移到结算器中。准备阶段完成后台结算，演出阶段只消费事件和播放占位播片/战斗记录。

### M3R GDD 时间轴 UI 对齐

复用现有指令队列 UI，补齐长按 2 秒、不可撤回、不可覆盖、检视详情、怪物意图揭示、开始结算按钮。

### M4R 教学关可玩

基于现有关卡和角色资源，实现第一关教学流程、基础敌人 AI、百科入口和胜负结算。

### M5R 三关 Demo

实现第二关小型敌方团体、第三关双精英战、成长奖励、技能解锁和水墨风静态资源替换。

## 开发原则

- 先利用已有 Demo，不从零重建。
- 先加抽象层，后迁移旧逻辑。
- 不做一次性大规模目录迁移。
- 结算逻辑和演出逻辑分离：演出阶段只消费 `CombatEvent`。
- 数据优先结构化，避免把数值和中文文本写死在场景中。
- 禁止批量删除文件或目录。
