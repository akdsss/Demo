# 推荐 Godot C# 项目结构

以下结构适合 Godot 4.x + C#，并支持内容、UI、战斗内核、美术资源并行开发。

```text
D:\GodotProject\202605gamejam
├── Art
│   ├── Characters
│   │   ├── Player
│   │   └── Enemies
│   ├── UI
│   │   ├── Icons
│   │   ├── Portraits
│   │   └── Panels
│   ├── Backgrounds
│   ├── Effects
│   ├── CutscenePlaceholders
│   ├── Generated
│   └── Source
├── Audio
│   ├── BGM
│   ├── SFX
│   └── Source
├── Data
│   ├── Combat
│   ├── Content
│   ├── Levels
│   ├── Localization
│   └── Tutorials
├── Docs
│   ├── GDD
│   ├── Technical
│   └── QA
├── GDD_Split
├── Scenes
│   ├── Battle
│   ├── Characters
│   ├── UI
│   ├── Levels
│   └── Boot
├── Scripts
│   ├── Core
│   ├── Combat
│   │   ├── Timeline
│   │   ├── Resolution
│   │   ├── Skills
│   │   ├── Statuses
│   │   └── AI
│   ├── Data
│   ├── UI
│   ├── Presentation
│   ├── Progression
│   └── Utilities
├── Shaders
├── Tests
│   ├── Combat
│   ├── Data
│   └── PlayMode
└── Tools
    ├── Data
    ├── Json
    ├── Localization
    └── Import
```

## 文件夹说明

### `Art`

存放导入 Godot 的美术资源。缺少的静态美术由 AI 生成，风格优先考虑水墨风。`Generated` 存放 AI 生成并可直接使用的导出图，`Source` 存放可编辑源文件、生成提示词记录、参考图等。战斗播片当前阶段先放入 `CutscenePlaceholders` 占位，不要求生成正式播片。

### `Audio`

存放音乐、音效与源工程。当前 GDD 明确 BGM 暂不考虑，攻击/受击等战斗音效暂不考虑；但 UI hover/click、阶段提示、行动确认等轻量音效可以预留接口并放在 `SFX/UI`。

### `Data`

存放游戏配置。建议只放机器可读数据，不放长篇设计说明。

- `Combat`：技能、状态、区域、公式参数。
- `Content`：角色、敌人、AI profile。
- `Levels`：关卡配置、敌人编队、奖励。
- `Localization`：中文 UI、战斗日志、失败原因。
- `Tutorials`：教程步骤。

### `Docs`

存放开发说明、技术方案、QA 记录。与 `Data` 分离，避免把说明文档误当配置读入。

### `Scenes`

Godot 场景文件。

- `Battle`：战斗主场景、战场区域、时间轴。
- `Characters`：角色表现节点。
- `UI`：可复用 UI 场景。
- `Levels`：关卡入口或关卡组装场景。
- `Boot`：启动、加载、全局服务挂载。

### `Scripts`

C# 脚本。

- `Core`：游戏状态、事件总线、服务定位或依赖注入。
- `Combat/Timeline`：时间轴和行动计划。
- `Combat/Resolution`：战斗结算器、公式、事件。
- `Combat/Skills`：技能执行模型。
- `Combat/Statuses`：状态与触发器。
- `Combat/AI`：敌人行动规划。
- `Data`：配置加载、schema 校验。
- `UI`：界面控制器。
- `Presentation`：动画、特效、演出事件消费。
- `Progression`：关卡胜利、成长、解锁。
- `Utilities`：通用工具。

### `Tests`

推荐优先测试战斗结算和数据加载。时间轴规则复杂，越早有测试越省时间。

### `Tools`

存放数据检查、导入、转换脚本。所有脚本必须显式使用 UTF-8 without BOM，并说明输入输出。

## 命名建议

- C# 类名使用 PascalCase。
- 数据 ID 使用 `snake_case`。
- Godot 场景使用 PascalCase，例如 `BattleScene.tscn`。
- 资源文件使用小写短横线或下划线，避免中文路径。
