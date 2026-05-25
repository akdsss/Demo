# ChangeLog

## 2026-05-25 18:15 百科面板背景图接入
- `EncyclopediaOverlayControl` 将百科正文区域拆成左侧条目列表区与右侧详情区，两侧各自添加铺满容器的 `TextureRect` 背景：左侧加载 `res://Asset/ui image/Encyclopedia_left.png`，右侧加载 `res://Asset/ui image/Encyclopedia_right.png`。
- 背景图使用 `FullRect` 锚点与 `StretchModeEnum.Scale`，随左右区域尺寸自动填充；条目按钮和详情文本通过 `MarginContainer` 叠放在背景图上方。
- 百科面板继续沿用“Overlay 适配视口 + 面板中心锚点”的定位方式，并按当前视口可用宽度收缩，避免窗口较窄时固定宽度溢出过多。
- 通过 Godot headless editor 导入新增背景资源，生成 `Encyclopedia_left.png.import` 与 `Encyclopedia_right.png.import`。
- 验证：沙箱外 `dotnet build "D:\GodotProject\202605gamejam\Demo\Demo.csproj" --no-restore` 通过，0 警告 0 错误；Godot headless 打开 `res://Scene/MainScene.tscn` 可进入准备阶段，`Demo/godot.log` 未检出 `ERROR`、`Exception`、`NullReference` 或百科贴图加载错误。

## 2026-05-25 16:33 分辨率、地图锚点调试与圆形头像适配
- 将项目窗口/视口分辨率调整为 `1920x1080`，并同步适配主要 UI 的尺寸、位置与布局参数。
- `ChessBoardUIControl` 新增 `UseResponsiveLayout` 地图自适应开关；当前场景中保持关闭，使 `MainScene` / `MainUI` 手动调节地图大小时不会被运行时自适应逻辑覆盖。
- `ShowEmptyCharacterAnchorsForDebug` 作为空头像站位点调试显示开关保留；当前 `MainUI.tscn` 设置为 `false`，测试完后默认隐藏空站位点。
- `CharacterAnchorDisplayScale` 用于整体调节所有头像站位点/头像显示缩放；当前场景值为 `3.3`，并通过 `[Tool]` 编辑器预览同步到 `MainScene`，仅用于调节预览，不改变正式运行逻辑。
- `ChessBoardUIControl` 与 `ChessCellUIControl` 增加编辑器安全预览流程，修复 Tool 脚本下 `Control` 转 `ChessCellUIControl` 的 `InvalidCastException`。
- 地图头像改为圆形 shader 遮罩显示，圆形直径跟随头像控件显示宽度；空站位点会清除头像材质，避免调试占位图也套用圆形头像材质。
- 验证：`dotnet build Demo.csproj` 通过，0 警告 0 错误。

## 2026-05-25 战斗记录日志与面板修正

- `CombatEventLogFormatter` 将 `SkillFailed` 日志从直接显示失败枚举名改为中文失败原因，覆盖 MP 不足、攻击目标在不同区域、移动目标为相同区域、释放者/目标已战败、目标闪避、状态限制等情况，并在可用时补充当前 MP、所需 MP、释放者区域和目标区域。
- `PubTool` 新增运行时输出缓存，`PrintToCmdAndTitle()` 写入 Godot 输出面板的同时同步写入战斗记录缓存，供游戏内战斗记录 UI 复用。
- 新增 `BattleLogOverlayControl`，点击右上角战斗记录按钮可打开黑色半透明战斗记录面板；面板支持右键关闭，也支持点击右上角 `关闭` 按钮退出。
- `MainUIControl` 动态创建战斗记录按钮并放置在设置齿轮左侧；按钮点击层级和鼠标处理沿用设置按钮的运行时处理方式。
- 战斗记录按钮图标资源从旧的 `res://Asset/ui image/log.png` 切换为 `res://Asset/ui image/battlelog.png`；旧 `log.png.import` 作为旧资源导入元数据未删除。
- 修正 `battlelog.png` 原图尺寸大于齿轮图标导致显示过大的问题：战斗记录按钮内的 `TextureRect` 改为 `ExpandModeEnum.IgnoreSize` 与 `StretchModeEnum.KeepAspectCentered`，按按钮矩形缩放并保持比例居中。
- 按 `ProjectManagement/exp.md` 中百科面板居中经验修正战斗记录面板定位：外层 Overlay 使用 `FullRect + FitToViewport()` 强制适配视口，面板改为以屏幕中心 `0.5f / 0.5f` 锚点和固定宽度展开，避免运行时贴在屏幕左侧。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误。

## 2026-05-25 设置齿轮点击与设置面板交互修复

- 修复右上角设置齿轮点击无反应：根因是运行时顶部时间轴宿主铺满 `TopPanel`，且位于齿轮按钮之后，空白区域会拦截鼠标；同时齿轮按钮内部的装饰 `TextureRect` 也会参与鼠标命中。
- `CmdQueueUIControl` 将敌我时间轴宿主与运行时生成的时间轴空白行设置为 `MouseFilter.Ignore`，避免时间轴空白区域吞掉设置按钮点击；时间轴布局刷新后会重新整理设置按钮点击层级。
- `MainUIControl` 为设置按钮增加运行时防护：提高 `ZIndex`、`MoveToFront()`、让装饰子节点忽略鼠标，并在 `_Input` 中按按钮全局矩形兜底处理左键点击。
- 删除 `MainUI.tscn` 设置面板内的 `×` 关闭按钮节点、关闭图标资源引用和对应 `pressed` signal；设置面板不再提供独立关闭图标。
- 设置齿轮改为开关行为：再次点击右上角设置 UI 可关闭设置面板；玩家按 `Esc` 也可唤起或关闭设置面板。
- 设置齿轮的位置与尺寸仍由 `Scene/MainUI.tscn` 中 `Panel/VBoxContainer/TopPanel/Button` 的锚点和 offset 决定，当前为右上角锚定、约 `66x66`。
- 验证记录：沙箱内 `dotnet build .\Demo.csproj --no-restore` 受本机 `NuGet.Config` 访问限制失败；沙箱外 `dotnet build .\Demo.csproj --no-restore` 通过，0 警告 0 错误；用户已手动检查设置面板交互并确认无问题。

## 2026-05-25 行动轮次防重复行动修复
- 问题反馈：每个准备轮次内，每个角色应最多安排一次行动，但当前只要 `currentRestActionTimes` 仍大于 0，同一角色就能在同一玩家准备轮次内连续放置多次指令。
- 根因定位：`CharacterData` 已有 `hasPrepared` 标记，玩家放置指令时也会写入该标记，但 `BattleManager.CheckPlayerReadyOver()`、玩家选择面板、时间轴放置校验和部分旧头像入口仍只判断剩余行动次数，没有排除“本轮次已行动”的角色。
- `CharacterData` 新增统一判断 `HasRemainingActionChance()` 与 `CanPrepareActionThisRound()`，并在角色初始化时清空 `hasPrepared`；两个 `SetCommand(...)` 重载都会在成功放置指令时设置 `hasPrepared = true`，敌方行动也纳入同一标记。
- `BattleManager.PrepareTurn()` 现在在每个敌方/玩家准备轮次开始时重置对应阵营的 `hasPrepared`，本轮是否还能行动使用 `CanPrepareActionThisRound()`，整轮准备是否结束仍使用 `HasRemainingActionChance()` 判断剩余总行动次数，避免把“本轮次结束”和“本回合行动次数耗尽”混在一起。
- `CmdQueueUIControl`、`PlayerChoseListPanelControl`、`CommandItemUIControl` 与 `CharacterHeadButtonControl` 的行动入口改为调用统一判断，阻止同一角色在当前轮次再次选择技能或提交时间轴指令。
- 验证：`dotnet build Demo\Demo.csproj` 通过，0 警告 0 错误。

## 2026-05-25 战斗交互角色选择图片化

- `PlayerChoseListPanelControl` 将点击“设置指令”后的角色选择层由矩形文本按钮改为三张角色展示图，图片顺序为 `character0A_SHOW.png`、`character0B_SHOW.png`、`character0C_SHOW.png`。
- 三张角色图从 `res://Asset/character/` 加载，横向紧密排列；`PlayerSelectFirstImagePosition` 统一控制 `character0A_SHOW.png` 的屏幕位置，后两张根据图片宽高缩放后依次贴靠。
- 可行动角色图片支持鼠标悬停增亮并可点击进入技能分类；不可行动判据沿用 `CanPrepareActionThisRound()`，图片使用 `DisabledTargetColor` 置灰并禁用点击。
- 角色选择图片层临时重挂到 `MainUi/Panel` 做屏幕绝对定位；进入技能分类、技能列表或目标列表时恢复到左侧 `BattleInteractionPanel` 的原紧凑菜单布局。
- 编码说明：本轮看到的 `璁剧疆鎸囦护` 一类文本属于 PowerShell 未按 UTF-8 显示造成的终端乱码，不是源码内容损坏；项目已有 `.editorconfig` 指定 `charset = utf-8`，后续读写中文源码应继续显式使用 UTF-8，并避免整文件错误编码重写。
- 验证：`dotnet build Demo.csproj` 通过，0 警告 0 错误；`D:\Software\Godot_C\Godot_v4.6.2-stable_mono_win64_console.exe --headless --path . --quit-after 3` 在当前环境触发 Godot 原生 signal 11，`godot.log` 仅记录引擎启动与“开始游戏”，未能完成运行态截图验证。

## 2026-05-25 区域头像锚点与教学关站位修正

- 修复战斗地图区域锚点绑定：`ChessBoard.BindAreaAnchors()` 改为按显式区域顺序绑定场景节点，使 `ChessCell -> 乾`、`Control2 -> 坎`、`Control3 -> 艮`、`Control4 -> 兑`、`Control5 -> 阳`、`Control6 -> 阴`、`Control7 -> 震`、`Control8 -> 坤`、`Control9 -> 离`、`Control10 -> 巽` 与背景地图一致。
- 同步修正隐藏区域按钮的兼容 `coord`，避免后续临时启用地图区域点击时再次使用旧对应关系。
- 修复敌方初始位置严重覆盖问题：`LevelData.LevelInitialize()` 中敌人初始化不再硬编码为 `CombatAreaId.Yang`，改为与玩家一致读取 `EnemyInfoInLevel.areaId / coord`。
- 教学关 `Level_data0.tres` 当前初始站位调整为：近战训练人偶位于巽，远程训练人偶位于坤；我方近战位于艮，我方远程位于震，我方支援位于兑。
- 将 `MainUI.tscn` 中每个战斗区域的头像锚点从 9 个 `VBoxContainer/HBoxContainer` 九宫格槽位迁移为 7 个直接子 `TextureRect`：`Anchor0` 至 `Anchor6`。这些锚点不再受 3x3 容器布局约束，可在 Godot 编辑器中自由拖动以优化头像视觉分布。
- `ChessCellUIControl` 继续通过 `allCharacterPointArray` 显示头像；运行时逻辑无需感知锚点布局变化，单区域可用头像显示槽位从 9 个变为 7 个。
- 同步记录：此前按“第一关”口径调整过 `Level_data1.tres` 引用的“小型敌方团体”部分站位，目前保留为近战敌人阳、远程敌人阴、我方近战坎、我方远程乾、我方支援离。
- 验证：`dotnet build "D:\GodotProject\202605gamejam\Demo\Demo.csproj"` 通过，0 警告 0 错误；文本核对 `MainUI.tscn` 为 10 个区域、70 个自由锚点，旧九宫格容器已移除。地图锚点最终视觉位置仍需在 Godot 编辑器中人工确认。

## 2026-05-25 角色与技能显示名称替换

- 按用户提供的“旧文本——新文本”清单，更新玩家与敌方运行时角色名称：`虬髯客`、`戏中人`、`神算子`、`连弩车`、`不语僧`、`驱除大将军`、`蜮`、`空青`、`玄俗`、`虎贲将军`、`雷使`。
- 更新各角色对应技能 `commandName`，并同步成长奖励弹窗中的解锁技能文本、技能相关状态名与少量会显示旧技能名的说明文本。
- 为避免共享敌方技能资源改串角色，新增敌方专属技能资源：驱除大将军/空青的近战，虎贲将军的区域突袭，蜮/玄俗/雷使的远程技能，以及雷使的治疗技能；AI 逻辑仍沿用原 `commandId` 映射。
- 记录未精确匹配的旧文本：运行时资源里 `战士`、`射手`、`刺客`、`治疗`、`精英战士`、`精英远程` 分别实际为 `小型近战敌人`、`小型远程敌人`、`小型刺客敌人`、`小型治疗敌人`、`强力近战精英`、`远程支援精英`；部分敌方技能旧名也实际存为 `攻击`、`突袭`、`精英蓄力` 或 `远程`，已按对应角色技能组完成映射。
- 验证：`dotnet build Demo\Demo.csproj` 通过，0 警告 0 错误。

## 2026-05-25 教学提示面板与步骤引导优化

- `TutorialOverlayControl` 重做教学提示面板布局：顶层 Overlay 采用 `FullRect + FitToViewport()` 适配视口，提示面板改为固定宽高并围绕指定屏幕纵线用 offset 展开，避免仅调整百分比锚点导致运行时位置不准。
- 教学提示面板宽度调整为 `1024`，高度保持 `280`；“上一个”“继续”“跳过教学”按钮通过底部 spacer 固定在面板右下角。
- 教学文本支持每步配置横向位置：第 1、2 步居中，第 3 步及后续步骤移至屏幕右侧；第 1 步取消高亮，第 3 步高亮改为圆形并放大 2 倍。
- 教学等待文本改为黄色；“继续”按钮文字为绿色，“上一个”按钮文字为白色，“跳过教学”按钮文字为红色；新增“上一个”按钮，可回到上一条教学提示状态。
- 设置面板新增“教学”按钮，点击后重新启动当前关卡的教学 Overlay 流程。
- 第 5 步等待条件改为进入时间轴放置阶段 `EnterTimelinePlacement` 后推进，不再仅因选择技能推进；第 5 步文案开头追加 `点击”设置指令“，然后`，并新增 `SetCommandButton` 高亮目标指向左侧“设置指令”按钮。
- 验证：`dotnet build Demo.sln` 通过，0 警告 0 错误。

## 2026-05-25 右键返回与百科面板交互修正

- `PlayerChoseListPanelControl` 新增左侧指令菜单层级状态，右键会按“目标列表 -> 技能列表 -> 技能分类 -> 角色选择 -> 关闭指令菜单”的顺序逐级返回，不再直接清空整条选择链。
- `AreaTargetMenuControl` 新增右键返回处理，区域目标弹窗打开时右键会关闭弹窗并返回上一级技能选择。
- `MainUIControl` 改为在 `_Input` 阶段分发右键，优先处理百科面板内右键、战斗记录、设置面板和当前指令菜单，避免右键被控件吞掉或误触发其他面板返回。
- `EncyclopediaOverlayControl` 将“关闭”按钮从标题左侧调整到右上角；当鼠标位于百科面板内部时，右键只关闭百科并吞掉输入，不影响其他菜单或面板的返回逻辑。
- 百科面板背景改为不透明 `StyleBoxFlat`，`BgColor` alpha 调整为 `1f`。
- 新增 `ProjectManagement/exp.md`，记录百科面板第一次居中失败、第二次通过“Overlay 适配视口 + 面板中心锚点固定宽度”成功的实现经验。
- 验证：`dotnet build .\Demo\Demo.csproj` 通过，0 警告 0 错误。

## 2026-05-25 玩家时间轴剩余行动显示

- 玩家时间轴右侧尾列由空占位改为剩余行动次数文本，格式为 `剩余行动次数为X`，即使 X 为 0 也不显示 `完成`。
- 敌方时间轴右侧文案逻辑保持不变：剩余次数为 0 时仍显示 `完成`。
- 放宽时间轴右侧尾列宽度，避免玩家剩余行动文案被裁切。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-25 结算后战斗记录教学

- 将教学第 10 步改为在“开始结算”之后引导点击右上角战斗记录按钮，并新增第 11 步解释战斗记录面板。
- 教程高亮 `BattleLog` 时会优先指向实际战斗记录按钮，打开后指向记录面板；打开战斗记录会触发新的 `OpenBattleLog` 等待条件。
- `TutorialOverlayControl` 现在会缓存已经发生过的等待条件，避免演出阶段日志、区域变化或胜利事件过快发生时，后续教程步骤错过通知而卡住。
- 顺手修复战斗记录按钮重复初始化时尝试断开不存在 `pressed` 连接的运行错误。
- 验证：`dotnet build Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，`Demo/godot.log` 未检出 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-25 战斗交互 UI 时点高亮调整

- 调整 `CommandItemUIControl.EnablePlacement()`：进入时间轴放置阶段时仅开启可放置状态，不再自动将全部可放置时点设为高亮。
- 保留鼠标进入可放置时点时高亮、鼠标退出时取消高亮的交互；长按放置进度条逻辑未改动。
- 验证：`dotnet build Demo.sln` 通过，0 警告 0 错误。

## 2026-05-25 敌方释放时点与支援揭示逻辑修正

- 按 `GDD_Split/EnemyDesign.md` 重写敌方行动策划：`EnemyActionPlanner` 改为基于 AI profile、当前区域、已排队行动投影、空闲时点和候选技能选择释放时点/目标，避免敌方开局总把指令放在第 1 时点。
- 调整敌方初始化落点处理，当前敌方进入阳区以配合本轮敌方行动策划与验证口径。
- 将支援技能 `溯` / `瞰` / `卜` 的揭示改为放置指令后即时生效：当前回合按时点保存揭示状态，超出时点 6 的向后范围转入下一回合，后续放入已揭示时点的敌方行动会直接显示。
- 敌方行动入队时按揭示状态显示技能名/优先级；准备阶段结束仍全量揭示，`RevealIntent` 不再在结算期额外产生日志。
- 验证：`dotnet build D:\GodotProject\202605gamejam\Demo\Demo.csproj` 通过，0 警告 0 错误。

## 2026-05-24 玩家模板、技能补全与技能描述数值化

- 根据 `GDD_Split/PlayerDesign.md` 补齐第 2 关与第 3 关玩家基础角色模板，并新增对应 `PlayerInfoInLevel` 资源；`Level_data1.tres` / `Level_data2.tres` 已改为引用进阶后的近战、远程、支援角色数据。
- 扩展玩家成长奖励数据，支持最大 MP、行动次数、生命、攻击、技能解锁与技能升级等运行时应用；教学关、第 2 关、第 3 关的成长奖励已按当前 GDD 的关卡后解锁节奏更新。
- 补全玩家技能资源与旧指令兼容映射：回蓝、反击、蓄力范围、目标突袭、反击+、踢飞、闪避、区域范围、目标范围、追踪、单体连击、团队回蓝、区域治疗、溯、瞰、卜、强化、换位、回蓝+ 等技能已接入 `SkillDefinition.FromCommandData()`。
- 扩展战斗结算支持：MP 消耗/恢复、反击、防御减伤、强化增伤、目标移动、换位、区域治疗、延迟效果、意图揭示事件以及移动类技能的目标区域合法性检查。
- 调整玩家技能选择流程：需要角色目标的技能走左侧目标列表；`踢飞` 等需要“先选角色、再选区域”的技能会在目标选择后打开区域选择菜单。
- 新增 `SkillEffectMath` 与 `SkillDescriptionFormatter`，让技能详情中的“（倍率）攻击力”在展示时按当前释放者攻击力实时换算为具体伤害/治疗数值；左侧技能详情与右侧时间轴/队列详情均使用同一格式化入口。
- 验证：`dotnet build D:\GodotProject\202605gamejam\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误。

## 2026-05-24 时间轴左侧文本定位解耦

- `CmdQueueUIControl` 将上下时间轴 host 改为横向铺满面板，时间轴槽位区继续按屏幕中心线居中。
- 每一行的头像、HP、MP、名称信息改为独立贴左定位，不再参与时间轴总宽度和居中计算；上下位置仍与对应时间轴槽位保持同一行。
- `RefactorCodeGuide.md` 补充时间轴横向定位说明与后续手动调参位置。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 百科居中、回合流转、关卡推进与顶部背景高度修复

- `EncyclopediaOverlayControl` 改为撑满视口并以屏幕中心锚定百科面板，打开百科时强制刷新视口尺寸并置顶，避免百科显示区域贴到屏幕左端。
- `BattleManager.BattleStart()` 移除每回合结束后等待无人触发的 `MainTS` 信号，确保第二回合能够自然进入准备阶段并重新显示左侧战斗交互 UI。
- `GameMain` 增加三关 demo 的关卡序列推进逻辑；`GrowthRewardOverlayControl` 的继续按钮在关闭胜利奖励界面后调用 `ContinueAfterVictory()`，教学关胜利后可进入下一关。
- `CmdQueueUIControl` 将运行时 `TopPanelHeight` 从 `92f` 调整为 `97f`，使上方红色矩形背景短边约增加 5%。
- 验证：`dotnet build .\Demo\Demo.csproj` 通过，0 警告 0 错误。

## 2026-05-24 教学第 8 步与第二回合交互按钮修复

- 教学第 8 步取消等待 `EnemyActionsRevealed`，改为普通阅读步骤，由现有教程系统显示 `继续` 按钮进入下一步。
- 新回合准备阶段初始化时主动关闭 `开始结算` 按钮，避免上一回合演出前的结算入口残留影响第二回合左侧交互控件。
- `RefreshPrepareControlVisibility()` 在玩家准备阶段会强制关闭 `开始结算`，再根据行动次数显示 `设置指令` / `检视详情`。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 时间轴整体居中

- 将时间轴 host 定位从左侧 padding 改回按 `GetTimelineTotalWidth()` 计算总宽并以屏幕中心锚定。
- 移除 `TimelineHostSidePadding`，后续时间轴整体位置由总宽居中决定；左侧信息与槽位之间仍通过 `TimelineInfoToTrackGap` 控制。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 战斗交互角色选择与时间轴左侧布局优化

- `设置指令` 入口改为先显示我方角色列表；战败或剩余行动次数为 0 的角色置灰并禁用，选择可行动角色后再进入技能分类。
- 百科面板锚点从右侧区域调整到屏幕中间区域显示。
- 时间轴 host 改为横向铺满面板，左侧单位信息从面板左侧开始显示，并通过独立的 `TimelineInfoToTrackGap` 与时间轴槽位拉开距离。
- 新增 `TimelineInfoTextSeparation` / `TimelineHostSidePadding` 等常量，便于后续手动调整时间轴左侧文本间距和整体左右边距。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 时间轴间距与槽位高度微调

- 将时间轴槽位高度从 18 调整为 22，并同步更新 `CommandItem.tscn` 根节点高度。
- 将 `CommandItem.tscn` 场景内槽位文字字号从 8 调整为 9。
- 在 `BuildTimelineRows()` 中再次强制覆盖时间轴 host 行间距，确保敌方时间轴和我方时间轴使用同一套 `TimelineHostRowSeparation`。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 教学百科高亮与角色目标列表重构

- 修复教学第 4 步百科高亮：设置面板关闭时高亮右上角设置按钮，设置面板打开后继续高亮面板内的 `百科` 按钮。
- 将以角色为目标的玩家技能改为左侧次级目标列表：敌方目标列出全部敌人，友方目标列出全部友方；已战败目标显示为灰色并禁用。
- 修复治疗技能默认选自己的问题：治疗现在按 `Ally` 目标类型打开友方目标列表，由玩家选择治疗对象。
- 删除玩家技能选择中的旧敌方头像目标路径：`PlayerCommandData` 不再分发 UI 点击逻辑，旧敌方头像交互不再写入目标或开启时间轴放置。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 时间轴左侧信息区压缩

- 将时间轴左侧单位信息从上下两行改为横向单行：头像、HP、MP（仅玩家）、名称。
- 将角色时间轴行间距从 5 缩减为 1，槽位间距从 3 缩减为 1，行内信息区/槽位区/尾列间距从 4 缩减为 2。
- 同步缩减时间轴面板垂直 padding，减少上下方时间轴整体留白。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 指令长按确认时间调整

- 将玩家在时间轴空白槽位放置指令的长按确认时间从 2.0 秒缩短为 1.2 秒。
- 同步更新设置指令、空白槽位点击提示和区域目标确认后的提示文案。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误。

## 2026-05-24 时间轴短边压缩

- 将 `CmdQueueUIControl` 的 `TimelineSlotHeight` 从 30 调整为 18，约为原短边长度的 60%。
- 同步压缩时间轴行头头像、行头字号、槽位文字字号和长按进度条高度，避免控件内容反向撑高时间轴。
- 同步压缩 `CommandItem.tscn` 槽位 prefab 的原始高度和场景内字号，避免 Godot 容器继续按 prefab 的 37 高度排版。
- 下调 `TopPanel` / `DownPanel` 运行时最小高度，使压缩后的敌我时间轴占屏更少。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-24 指令提交上下文重置与行动次数防漏

- 新增 `CmdQueueUIControl.ResetCommandSelectionContext()`，统一清理技能分类/技能列表、区域目标菜单、敌方目标头像、时间轴放置高亮和当前技能/目标上下文。
- 长按时间轴成功放置指令后，立即退出放置模式并回到左侧主按钮初始状态；下一次指令必须重新点击 `设置指令`，重新选择仍有行动次数的角色。
- 在 `SwitchOnPlayerCommandSet()` 与时间轴长按提交前补充行动次数防线，无剩余行动次数的角色会拒绝提交并提示。
- 统一准备阶段按钮显隐：有可行动我方时显示 `设置指令` / `检视详情`；后台结算完成并揭示敌方行动后显示 `开始结算`。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 轮准备阶段，日志未发现 `ERROR`、`Exception`、`NullReference` 或 C# 编译错误。

## 2026-05-23 时间轴 UI 重写与技能菜单修复

- 重写 `CmdQueueUIControl` 时间轴运行时布局：敌方与我方时间轴共用同一组宽度、行高、槽位宽度和尾列尺寸；`TopPanel` / `DownPanel` 改为固定高度，不再依赖 VBox 拉伸比例，避免我方时间轴过高并溢出屏幕。
- 缩小时间轴行高、头像、文字字号与槽位文本字号；我方 HP/MP 合并为单行显示，敌我时间轴视觉尺寸保持一致。
- 左侧战斗交互区只保留 `设置指令`、`检视详情`、`开始结算` 及其技能分类/技能列表；原左侧 `DetailLabel` 在运行时移除。
- 新增右侧 `BattleInfoPanel`，准备阶段提示、技能详情、检视信息、怪物行动揭示和演出阶段战斗详情统一显示到右侧。
- 压缩 `PlayerChoseListPanelControl` 的分类/技能按钮高度、标题和说明文本高度；技能分类刷新技能列表后强制保持面板可见，减少“点击分类后菜单像消失”的问题。
- 敌方目标头像列表显示时调用 `MoveToFront()`，避免被右侧信息面板遮挡导致技能目标不可选。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 回合准备阶段，日志未检出 `ERROR`、`Exception` 或 C# 编译错误关键字。

## 2026-05-23 旧 UI 兼容层与直接执行入口重构

- 删除旧我方头像列表脚本 `PlayerCharacterHeadListUIControl.cs` 及 `.uid`，删除旧混合队列头像脚本 `CommandHeadListUIControl.cs` 及 `.uid`；相关战斗流程引用已移除，当前我方/敌方信息以 GDD 上下分离时间轴为准。
- 移除 `CommandExecuteInfo.ExecuteInPlay()` 与旧伤害/位移注释块，`CommandExecuteInfo` 收口为 UI/AI 行动请求 DTO，战斗结果统一由 `CombatResolver -> CombatEvent -> CombatEventApplier` 生成和应用。
- 将区域显示锚点绑定收口到 `ChessBoard.BindAreaAnchors()`；`ChessBoard` / `ChessCell` 作为历史场景兼容名保留，真实语义记录为 10 区域显示板与区域锚点。
- 清理 `BattleManager`、`CmdQueueUIControl`、`PubTool` 等文件中的旧测试流程与旧 UI 注释残留，并补充 `PubTool` 状态栏空引用保护。
- 新增 `ProjectManagement/RefactorCodeGuide.md`，说明当前架构、战斗数据流、代码规范、历史命名映射、删除清单与验证方式。
- 验证：通过临时重定向 `APPDATA` / `LOCALAPPDATA` 并使用本机 NuGet 包缓存执行 `dotnet build .\Demo\Demo.csproj --no-restore`，0 警告 0 错误；Godot Mono headless 打开 `res://Scene/MainScene.tscn` 可进入第 1 回合准备阶段，日志未检出 `ERROR`、`Exception` 或 C# 编译错误关键字。

## 2026-05-23 区域 CurrentAreaId / TargetAreaId 重构

- 按 `GDD_Split/04_Areas_Statuses.md` 与 `ProjectManagement/Decisions.md` 的区域核心支柱重构区域相关代码：角色真实站位改以 `CharacterData.CurrentAreaId` / `CharacterState.CurrentAreaId` 为准，旧 `coord` 仅保留为兼容显示坐标。
- 新增并贯通 `CommandExecuteInfo.targetAreaId`、`PlannedAction.TargetAreaId`、`CombatEvent.TargetAreaId` / `FromAreaId` / `ToAreaId`，移动、AI、校验、日志和旧命令适配器均优先读写区域 ID。
- 范围伤害改为按 `CurrentAreaId == TargetAreaId` 过滤存活单位，不再读取头像位置、锚点编号、背景像素或旧棋盘坐标；是否排除施法者由 `SkillEffectDefinition.ExcludeSource` 显式决定。
- 区域视觉继续复用当前 10 个区域 Control，每区 9 个透明头像锚点；角色进出只释放/占用该角色自己的锚点，不重排同区域其他头像。
- 新增运行时 `AreaTargetMenuControl`：玩家选择区域目标时显示 10 个区域名矩形按钮；按钮外点击只返回上一级并吞掉输入，不触发地图、头像或时间轴点击。
- 保持 2026-05-23 素材清理结论：未恢复 `Asset/Generated`、`BattleAssetCatalog`、`BattlePresentationPlaceholderControl`、`UISfxRouter` 或状态图标容器引用。

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

## 2026-05-22 M4R 教学关配置、基础敌人 AI 与教程数据

- 先阅读 `GDD_Split` 与 `ProjectManagement` 下全部文档，确认 M4R 当前先沿用现有 `LevelData` / `.tres` Resource 工作流，不引入 JSON 数据迁移，不改无关场景层级。
- `Demo/Data/DataScript/LevelData.cs` 增加 `tutorialStepDataArray`，并在 `LevelType` 中增加 `TUTORIAL`，用于把教学关和普通关卡区分开。
- 将 `Demo/Data/LevelData/Level_data0.tres` 从“测试关”配置为“教学关”：设置 `levelId = 1`、`levelType = TUTORIAL`，并暂时使用 2 名现有玩家和 1 名教学敌人。
- 新增 `Demo/Data/DataScript/TutorialStepData.cs`，定义教程步骤 Resource 字段：步骤 ID、顺序、标题、提示文本、高亮目标、等待条件、目标 ID 和是否阻塞其他输入。
- 新增 12 个教程步骤资源到 `Demo/Data/Tutorials/TutorialStepData/`，覆盖时间点与区域、六时点、八卦区域修正、百科、技能选择、长按放置、检视详情、怪物行动揭示、开始结算、战斗记录、区域变化和胜利成长。
- 新增 `Demo/Script/Combat/AI/EnemyActionPlanner.cs`，基础敌人行动生成不再固定取 `enemyCommandDataArray[0]`：会根据敌人当前/已规划位置选择攻击、移动或跳过，并放入第一个可用时点。
- `BattleManager.PrepareTurn()` 改为调用 `EnemyActionPlanner` 生成敌人行动，并按生成出的时点写入现有时间轴 UI。
- 更新 `enm_dataA01.tres` 与 `enm_dataA02.tres`，为基础敌人接入移动、攻击、跳过等现有敌人指令资源；`enm_dataA01.tres` 命名为“教学敌人”。
- 更新 `enm_cmd1.tres`、`enm_cmd2.tres`、`enm_cmd3.tres` 的优先级，使基础 AI 可以稳定排序移动、攻击与跳过。
- 验证记录：Godot `--headless --path Demo --quit-after 3` 与 `--build-solutions --quit` 均正常退出；`Level_data0.tres` 中所有 `res://` 引用路径均存在。
- `dotnet build Demo/Demo.csproj --no-restore` 在沙箱内仍因无法读取本机 `NuGet.Config` / `Godot.NET.Sdk` 失败；按规则请求沙箱外构建两次，自动审批均超时。
- 编码检查：本次新增/修改的 C#、`.tres` 与项目管理 Markdown 文件均可严格 UTF-8 解码，且无 UTF-8 BOM。
- 更新 `TaskBoard.md`：完成 M4R 前三个 Todo；下一个 Todo 为“实现教程遮罩、高亮、提示文本和等待条件”。

## 2026-05-22 M4R 运行时教程 Overlay 与百科入口

- 根据用户确认，采用“运行时动态 Overlay”方向：不手工修改 `MainUI.tscn` 深层结构，由 `MainUIControl` 在运行时创建教程和百科 UI。
- 新增 `Demo/Script/UIControl/TutorialOverlayControl.cs`，提供半透明遮罩、目标高亮、提示标题/正文、继续按钮、跳过教学按钮和等待条件推进。
- `MainUIControl` 启动时动态创建 `TutorialOverlayControl` 并注册到 `SceneSingleton`；`BattleManager.BattleInitialize()` 在教学关加载后启动 `LevelData.tutorialStepDataArray`。
- 教程 Overlay 当前可高亮战斗区域、时间轴、技能列表、检视按钮、开始结算按钮、百科入口和战斗记录详情区；战斗区域高亮使用当前棋盘兼容区域。
- 将教程等待条件接入现有交互：选择技能、长按放置指令、检视详情、怪物行动揭示、点击开始结算、战斗记录显示、角色移动和胜利。
- 新增 `Demo/Script/UIControl/EncyclopediaOverlayControl.cs`，运行时创建右上角“百科”入口与条目面板。
- 百科当前包含两个条目集合：“区域修正”和“状态效果”；区域修正解释八卦区域和当前棋盘兼容规则，状态效果包含闪避、刻印、护盾、燃烧、罡风、狂怒等基础条目。
- 教程步骤 1 到 10 已能通过运行时 Overlay 与现有交互推进；步骤 11 暂停在“进入/离开区域的修正变化”，因为当前项目尚未接入真实八卦区域修正，需要后续决定先做占位演示还是先接完整区域修正。
- 验证记录：Godot `--headless --path Demo --quit-after 3` 与 `--build-solutions --quit` 均正常退出；`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- `dotnet build Demo/Demo.csproj --no-restore` 在沙箱内仍因无法读取本机 `NuGet.Config` / `Godot.NET.Sdk` 失败；按规则请求沙箱外构建两次，自动审批均超时。
- 编码检查：本次新增/修改的 C# 与项目管理 Markdown 文件均可严格 UTF-8 解码，且无 UTF-8 BOM。
- 更新 `TaskBoard.md`：完成 M4R 的教程 Overlay、教程步骤 1-10，以及百科入口和两个条目集合；下一个待决策 Todo 为“教程步骤 11：演示进入/离开区域的修正变化”。

## 2026-05-22 M4R 完整八卦区域修正系统

- 按用户指示，先接入完整八卦区域修正系统，再继续教程步骤 11；本次仍沿用现有棋盘和运行时 UI，不改 `MainUI.tscn` 深层结构。
- `AreaDefinition.FromLegacyCoord()` 已将当前 10 个棋盘格映射为乾、兑、离、震、巽、坎、艮、坤、阴、阳十区域，并统一提供显示名、百科文本和坐标格式化。
- 新增并接入 `CombatAreaRules` 与 `StatusCatalog`：覆盖移动优先级修正、伤害/治疗倍率、回合开始/结束触发、进入/离开区域触发、护盾吸收、闪避、刻印、燃烧、罡风、湍扼、压顶残留和丰壤残留。
- `CombatResolver` 现在使用全员共享战斗状态结算一回合，执行区域回合触发、修正后的优先级排序、状态禁用、闪避判定、近战同区域校验和移动到原地校验。
- `CombatEventApplier` 会把运行时状态与护盾同步回 `CharacterData`，避免区域触发产生的状态在后续时点或下一回合丢失。
- 百科区域条目改为从完整十区域定义生成，状态条目改为从状态目录生成；时间轴详情和敌方揭示详情会显示区域修正后的实际优先级。
- 教程步骤 11 现在可由真实 `CharacterMoved` / `AreaChanged` 事件推进，用实际进入/离开区域修正规则演示，不再依赖占位说明。
- 验证记录：Godot `--headless --path Demo --build-solutions --quit`、`--headless --path Demo --quit-after 3` 与 `--headless --editor --path Demo --quit` 均正常退出；`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- `dotnet build Demo/Demo.csproj --no-restore` 在沙箱内仍因无法读取本机 `NuGet.Config` 与 `Godot.NET.Sdk` 失败；按规则请求沙箱外构建两次，自动审批均超时。
- 编码检查：本次新增/修改的八卦区域相关 C# 与项目管理 Markdown 文件均可严格 UTF-8 解码，且无 UTF-8 BOM。
- 更新 `TaskBoard.md`：完成 M4R“教程步骤 11：演示进入/离开区域的修正变化”；下一项为“教程步骤 12：胜利后展示成长界面”。

## 2026-05-22 M4R 胜利成长展示

- 按 M4R 范围完成“胜利后展示成长界面”：新增运行时 `GrowthRewardOverlayControl`，由 `MainUIControl` 动态创建，不修改 `MainUI.tscn` 深层结构。
- 新增 `GrowthRewardData`，让成长展示从奖励资源读取头像、属性变化、新技能提示和总结文本；真实数值应用、技能解锁和后续关卡使用仍留到 M5R。
- 新增 `Demo/Data/GrowthRewards/growth_reward_tutorial.tres`，并挂到教学关 `Level_data0.tres` 的 `growthRewardData` 字段。
- `BattleManager` 在胜利判定时展示成长面板，并继续通知教程 `VictoryReached`，让教程步骤 12 能通过真实胜利流程收尾。
- `TutorialOverlayControl` 支持高亮成长面板，便于步骤 12 指向胜利结算 UI。
- 验证记录：Godot `--headless --path Demo --build-solutions --quit`、`--headless --path Demo --quit-after 3` 与 `--headless --editor --path Demo --quit` 均正常退出；`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- 资源引用检查：`Level_data0.tres` 与 `growth_reward_tutorial.tres` 中的 `res://` 路径均存在。
- 编码检查：本次新增/修改的成长展示相关 C#、`.tres` 与项目管理 Markdown 文件均可严格 UTF-8 解码，且无 UTF-8 BOM。
- 更新 `TaskBoard.md`：完成 M4R“教程步骤 12：胜利后展示成长界面”；下一项为“教学关完整通关测试”，需要进入 Godot 交互流程进行人工回归。

## 2026-05-22 M5R 第二关小型敌方团体配置

- 先复核 `DemoUIPrefabReuseReport.md`、`DemoStructureMapping.md` 和 `GDD_Split/07_GodotProjectStructure.md`，确认当前仍采用“保留 `Demo/Asset`、`Demo/Data`、`Demo/Scene`、`Demo/Script`，在现有目录内部分层”的策略；本次无需修改结构文档。
- 新增第二关关卡资源 `Demo/Data/LevelData/Level_data1.tres`，关卡名为“小型敌方团体”，使用 3 名现有玩家与 4 名第二关敌人。
- 新增第二关敌人站位资源：近战、远程、刺客、治疗 4 个 `EnemyInfoInLevel`。
- 新增第二关敌人数据：`小型近战敌人`、`小型远程敌人`、`小型刺客敌人`、`小型治疗敌人`，按 `EnemyDesign.md` 配置生命值、攻击力和每回合 2 次行动机会。
- 新增敌方指令资源：`蓄力`、`远程`、`突袭`、`治疗`；保留现有 `移动`、`攻击`、`跳过` 指令作为兼容基础。
- 扩展 `SkillDefinition.FromCommandData()` 的敌方旧指令兼容映射，让新增敌方指令能提供标签、目标类型和基础结算效果；完整 AI profile、治疗目标选择、突袭闪避与蓄力延迟仍留给后续 M5R Todo。
- 验证记录：新增 `.tres` 文件均可严格 UTF-8 解码且无 BOM；`.tres/.tscn` 的 `res://` 引用路径存在；资源 UID 未发现重复。
- 验证记录：Godot `--headless --path Demo --build-solutions --quit`、`--headless --path Demo --quit-after 3` 与 `--headless --editor --path Demo --quit` 均正常退出，`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- 更新 `TaskBoard.md`：完成 M5R“配置第二关小型敌方团体”；下一项为“配置第三关双精英战”。

## 2026-05-22 M5R 第三关双精英战配置

- 继续沿用现有 `LevelData` / `.tres` Resource 工作流，不改 `Demo` 目录结构、不移动场景、不调整 UI prefab。
- 新增第三关关卡资源 `Demo/Data/LevelData/Level_data2.tres`，关卡名为“双精英战”，使用 3 名现有玩家与 2 名精英敌人。
- 新增第三关敌人站位资源：`enm_info_level2_melee_elite.tres` 与 `enm_info_level2_support_elite.tres`，当前按 `EnemyDesign.md` 的敌人初始站位原则放在阳区域兼容坐标。
- 新增第三关敌人数据：`强力近战精英`（生命值 300、攻击力 16、基础每回合 2 次行动）与 `远程支援精英`（生命值 228、攻击力 16、基础每回合 3 次行动）。
- 新增第三关敌方指令资源：`精英蓄力`、`单体`、`狂怒`、`冲锋`、`至阴`、`至阳`、`范围远程`；复用已有 `移动`、`治疗`、`远程`、`跳过`。
- 扩展 `SkillDefinition.FromCommandData()` 的敌方旧指令兼容映射，让第三关新增指令能在 UI 中展示目标、标签、优先级和基础结算效果。
- 本次只完成关卡/敌人/技能配置；奇偶回合行动次数、支援优先级 AI、狂怒触发与狂怒行动方案、至阴/至阳路径移动、延迟范围伤害仍留给后续 M5R Todo。
- 验证记录：新增第三关 `.tres` 与修改的 C# 文件均可严格 UTF-8 解码且无 BOM；`.tres/.tscn` 的 `res://` 引用路径存在；资源 UID 未发现重复。
- 验证记录：Godot `--headless --path Demo --build-solutions --quit`、`--headless --path Demo --quit-after 3` 与 `--headless --editor --path Demo --quit` 均正常退出，`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- 更新 `TaskBoard.md`：完成 M5R“配置第三关双精英战”；下一项为“实现刺客/近战/远程/支援敌人 AI profile”。

## 2026-05-22 M5R AI Profile、狂怒与成长奖励

- 在 `EnemyData` 中新增 `EnemyAiProfile`、狂怒开关、奇偶回合行动次数配置和运行时 `rageTriggered`，并让敌人初始化时重置狂怒触发状态。
- 将第二关敌人配置到 `Melee`、`Ranged`、`Assassin`、`Support` profile；将第三关配置到 `EliteMelee` 与 `EliteSupport` profile。
- 重构 `EnemyActionPlanner`：按 profile 分派近战追击、远程拉开距离、刺客突袭低血量目标、支援治疗/远程、精英近战和精英支援行动。
- 实现精英近战半血后优先在时点 6 放置 `狂怒`；狂怒状态持续至战斗结束，造成伤害 +50%、受到伤害 +50%、每回合行动次数 +1，并接入精英狂怒后的冲锋/蓄力/单体/至阴/至阳行动分支。
- 扩展范围伤害兼容结算：带 `Area` 标签的伤害技能会命中目标区域内除施放者外的存活单位，支撑精英蓄力、至阴/至阳和范围远程的 demo 级结算。
- 扩展 `GrowthRewardData`：新增可应用的最大生命、攻击、解锁技能、升级技能字段；胜利时由 `BattleManager` 应用到运行时玩家数据，再展示成长面板。
- 新增玩家技能资源：`远程射击`、`治疗`、`强击`；扩展玩家旧指令兼容映射和按钮点击逻辑，使解锁后的技能能进入现有目标选择与时间轴放置流程。
- 新增并接入 `growth_reward_level1.tres`、`growth_reward_level2.tres`，并补强 `growth_reward_tutorial.tres`，让教学关、第二关、第三关胜利后都有结构化成长奖励。
- 验证记录：变更涉及的 C#、`.tres` 与项目管理 Markdown 文件均可严格 UTF-8 解码且无 BOM；`.tres/.tscn` 的 `res://` 引用路径存在；资源 UID 未发现重复。
- 验证记录：Godot `--headless --path Demo --build-solutions --quit`、`--headless --path Demo --quit-after 3` 与 `--headless --editor --path Demo --quit` 均正常退出，`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- 更新 `TaskBoard.md`：完成 M5R 的 AI profile、狂怒状态与行动逻辑、胜利成长奖励、技能解锁和升级；下一项为“三关连续游玩测试”。

## 2026-05-22 M6R 资源与打磨暂停记录

- 按用户更正，技能不制作或接入图标，技能 UI 继续只显示技能名称；文档中的“技能/状态图标”已修正为“状态图标”。
- 新增区域图标资源到 `Demo/Asset/Generated/AreaIcons`，并通过 `BattleAssetCatalog` 为百科区域条目提供图标路径。
- 新增简约蓝色状态图标资源到 `Demo/Asset/Generated/StatusIcons`，并在角色头像状态条与百科状态条目中预留显示入口。
- 新增战斗播片占位资源到 `Demo/Asset/Generated/CutscenePlaceholders`，并接入运行时 `BattlePresentationPlaceholderControl`。
- 新增 `UISfxRouter` 作为 UI hover/click/confirm/phase 音效接口；当前仅预留 AudioStream 接口，不接入 BGM 或战斗攻击/受击音效。
- `EncyclopediaOverlayControl` 详情区已补 `ScrollContainer`，内容超出时可滚动浏览；区域和状态条目可显示对应图标。
- 角色头像与敌人头像的正式素材由用户后续提供，本次不将头像 Todo 记为完成；已暂停继续处理头像素材。
- 验证记录：Godot `--headless --path Demo --build-solutions --quit` 与 `--headless --path Demo --quit-after 3` 均正常退出，`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- 更新 `TaskBoard.md`：完成 M6R 的区域图标、状态图标、战斗播片占位、UI 音效接口和百科滚动检查；角色/敌人头像保留为待办，等待用户提供正式素材。

## 2026-05-23 M6R 头像素材接入与生成素材残留清理

- 按用户指示，不再生成素材；先检查并清理此前错误接入的生成素材相关代码引用。
- 删除残留脚本 `Demo/Script/Presentation/BattleAssetCatalog.cs`、`BattlePresentationPlaceholderControl.cs`、`UISfxRouter.cs` 及对应 `.uid` 文件；删除操作均按单个明确路径逐个执行。
- 从 `MainUIControl`、`BattleManager`、`SceneSingleton`、`CharacterHeadButtonControl`、`CmdQueueUIControl`、`PlayerChoseListPanelControl`、`CommandItemUIControl`、`EncyclopediaOverlayControl` 中移除 `Asset/Generated`、状态图标、播片占位 overlay 和 UI SFX 路由相关引用。
- 依据 `ProjectManagement/PictureAsset.md`，将 3 名玩家头像接入 `character01.png`、`character02.png`、`character03.png`。
- 依据 `ProjectManagement/PictureAsset.md`，将 8 名敌人头像接入 `enemyA01.png`、`enemyA02.png`、`enemyB01.png`、`enemyB02.png`、`enemyB03.png`、`enemyB04.png`、`enemyC01.png`、`enemyC02.png`。
- 残留检查：`Demo/Script`、`Demo/Data`、`Demo/Scene` 中已无 `BattleAssetCatalog`、`UISfxRouter`、`BattlePresentationPlaceholder`、`StatusIconContainer` 或 `Asset/Generated` 引用。
- 验证记录：Godot `--headless --path Demo --build-solutions --quit`、`--headless --path Demo --quit-after 3`、`--headless --editor --path Demo --quit` 均正常退出；`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。
- 更新 `TaskBoard.md`：完成 M6R“接入玩家角色与敌人的图片素材”；下一项为“接入战斗播片占位资源”。

## 2026-05-23 教学关 GDD 配置与战斗 UI 修正

- 依据 `GDD_Split/EnemyDesign.md`、`GDD_Split/PlayerDesign.md`、`GDD_Split/06_UI_UX.md`、`ProjectManagement/PictureAsset.md` 与既有 `ProjectManagement/ChangeLog.md` 复核当前教学关偏差。
- 偏差原因：此前 M3R/M4R 记录中为降低风险选择“复用现有 `CmdQueueUIControl`、不做大规模场景迁移”，导致运行时仍保留旧的“点击我方头像选择技能 + 下方混合时间轴矩阵”原型；教学关资源也仍沿用临时的 2 名玩家 + 1 名教学敌人配置。
- 将 `Level_data0.tres` 调整为教学关 3 名玩家与 2 名训练人偶：我方近战 26 HP/12 ATK、我方远程 19 HP/10 ATK、我方支援 22 HP/10 ATK；近战训练人偶 49 HP/10 ATK、远程训练人偶 44 HP/10 ATK，均为每回合 2 次行动。
- 按 `EnemyDesign.md` 将教学关两名敌人初始站位改到阳区域兼容坐标，并将远程训练人偶接入远程攻击指令与 Ranged AI profile。
- 为玩家数据补充 MP/最大 MP 字段并按 `PlayerDesign.md` 初始值配置为 60/60；技能详情显示 GDD 优先级与 MP 消耗。
- 重构 `CmdQueueUIControl` 的运行时布局：敌方时间轴生成到屏幕上方，我方时间轴生成到屏幕下方，时间轴槽位总宽度收窄为约原先的一半；左侧生成“设置指令”“检视详情”“开始结算”和详情区域。
- 将释放技能流程改为左侧“设置指令 -> 近战/位移/远程/特殊 -> 技能”分类菜单；选择技能和目标后再在我方时间轴空白时点长按 2 秒放置，不再依赖点击我方头像 UI。
- “开始结算”按 `06_UI_UX.md` 改为显示确认弹窗；准备阶段判定改为所有存活玩家行动次数用尽后才进入结算。
- 按 `PictureAsset.md` 修复场景中残留的 `characterA01.png` / `characterB01.png` 缺失引用，改为 `character01.png` / `character02.png`。
- 验证记录：`dotnet build Demo/Demo.csproj --no-restore` 成功，0 警告 0 错误；`.tres/.tscn` 全量 `res://` 引用检查通过；本机 PATH 中未找到 Godot 可执行文件，未能进行 Godot 运行态截图/交互回归。
- 更新 `TaskBoard.md`：完成教学关 GDD 数值配置、敌/我时间轴布局、左侧设置指令入口三项修正。

## 2026-05-23 教学关 UI 运行态修正

- 修复旧 `CharacterHeadButton` 场景占位节点在未绑定 `characterData` 时输出“未知角色类型/未设置角色数据”的问题：占位头像现在静默隐藏，动态绑定数据的头像仍正常工作。
- 将 `CharacterHeadButtonControl` 与 `EnemyCharacterHeadListUIControl` 的 C# 基类改为实际挂载节点类型，避免隐藏/显示控件时出现类型问题。
- 将我方时间轴从旧 `CmdQueueUIControl` 所在的混合 `HBoxContainer` 中移出，改为直接挂到 `DownPanel/PlayerTimelineHost` 独立锚点区域，避免被旧容器布局挤成竖排或异常尺寸。
- 敌方目标选择用的旧敌人头像列表默认隐藏，只在选择需要敌方目标的技能时临时显示；常驻敌方信息由上方敌方时间轴承担。
- 验证记录：`dotnet build Demo/Demo.csproj --no-restore` 成功，0 警告 0 错误；Godot Mono `--headless --path Demo --quit-after 3` 正常退出；Godot Mono `--headless --path Demo --build-solutions --quit` 正常完成 .NET 构建。
- 更新 `TaskBoard.md`：完成旧头像占位报错修复与新时间轴容器修正。

## 2026-05-23 旧我方 UI 残留清理

- 按用户运行反馈，移除左上角“玩家准备中……”状态写入，准备阶段等待循环不再向状态 Label 或控制台输出该文本。
- 删除 `MainUI.tscn` / `MainScene.tscn` 中旧 `PlayerCHContainer` 下的预置头像节点与编辑路径；`PlayerCharacterHeadListUIControl.Initialize()` 不再实例化旧我方头像按钮，屏幕上我方角色 UI 只保留新生成的下方我方时间轴。
- 删除 `MainUI.tscn` / `MainScene.tscn` 中旧 `HeadList` 下的头像列节点与覆盖资源；`CommandHeadListUIControl` 不再实例化或写入旧时间轴头像列，避免再次显示 3 个旧我方头像。
- 撤回上一轮对旧 UI prefab/场景默认贴图的误改：旧 `CharacterHeadButton`、`CmdHeadPrefab`、`BaseCharacter` 以及旧场景占位贴图均改回 `unknown.png`，正式玩家素材只由角色数据资源引用。
- 验证记录：`.tres/.tscn` 全量 `res://` 引用检查通过；沙箱内 `dotnet build Demo/Demo.csproj --no-restore` 因无法读取 `C:\Users\Lenovo\AppData\Roaming\NuGet\NuGet.Config` 失败，已两次请求沙箱外构建但自动审批超时；Godot Mono headless 启动本轮在沙箱内触发 Godot 原生 signal 11，`Demo/godot.log` 未记录 C# 编译错误或旧头像空数据错误。
- 更新 `TaskBoard.md`：完成旧我方头像 UI、旧时间轴头像列和“玩家准备中……”显示清理。
## 2026-05-23 敌我时间轴视觉尺寸统一补修

- 原因说明：上一轮虽然统一了行头、槽位和尾列宽度参数，但我方时间轴仍额外生成 `PlayerActionBanner` 横幅；同时敌我时间轴宿主使用百分比锚点拉伸，槽位 prefab 实际高度为 37，而代码行高参数仍是 20，导致运行态视觉尺寸仍不一致。
- 移除我方时间轴专属 `PlayerActionBanner` 生成和 `BattleManager` 对该横幅的状态更新调用，避免我方时间轴比敌方多一块额外高度。
- 将敌我 `EnemyTimelineHost` / `PlayerTimelineHost` 改为使用 `GetTimelineTotalWidth()` 固定总宽并居中，避免父容器百分比锚点造成视觉宽度差异。
- 将 `TimelineSlotHeight` 调整为 38，与 `CommandItem.tscn` 槽位 prefab 的实际高度一致；敌我行高、槽位高度、行头高度和尾列高度继续共用同一组参数。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 通过，0 警告 0 错误；Godot Mono headless 主场景可进入准备阶段。

## 2026-05-23 战斗 UI 状态文本与时间轴尺寸修正

- 原因说明：左上角再次出现 `已将prepareTurnState修改为PLAYER_PRE`，是因为 `BattleManager.SetManagerState(PrepareTurnState)` 仍把状态切换调试文本传给 `PubTool.PrintToCmdAndTitle()`，而该工具会写入旧 `TopPanel/Label`。
- 移除 `BattleManager` 四个 `SetManagerState(...)` 重载中的 `已将...修改为...` 调试标题写入，避免内部状态枚举显示到战斗 UI。
- 将 `MainUI.tscn` 的旧 `TopPanel/Label` 默认隐藏并清空显示文本，避免旧状态标题再次覆盖敌方时间轴区域。
- 原因说明：敌我时间轴显示大小不一致，是因为敌方行额外带 `剩余行动` 尾列，且敌我角色信息区宽度分别使用了不同硬编码值。
- `CmdQueueUIControl` 新增统一的时间轴尺寸参数：行头宽度、槽位总宽、尾列宽度、行间距和槽间距；敌方使用尾列显示剩余行动，我方使用同宽空占位，因此敌我时间轴总宽一致。
- 更新 `TaskBoard.md`：完成旧状态标题文本清理与敌我时间轴尺寸统一。

## 2026-05-23 Godot 运行态崩溃窗口修正

- 针对用户反馈的 `0xc00007FF78C996C14` / `0x0000000000000058` 读取内存崩溃窗口，复查旧 UI 清理后的运行态初始化路径。
- 旧我方 UI 的可见内容仍保持删除：`PlayerCHContainer` 下不再保留预置头像按钮，`HeadList` 下不再保留旧头像列节点；为避免 Godot 继承场景和脚本生命周期访问空路径，仅保留隐藏的空兼容锚点。
- `CmdQueueUIControl` 的“设置指令 / 检视详情 / 开始结算”按钮 signal 改为只连接一次，避免初始化重复调用时先断开不存在连接而报错。
- `PlayerCharacterHeadListUIControl`、`EnemyCharacterHeadListUIControl`、`CommandHeadListUIControl`、`ActionListUIControl` 的 `_Ready()` / 构建入口增加空安全保护，避免单独装载 UI 场景或旧兼容容器为空时抛出 C# 空引用。
- 验证：`Godot_v4.6.2-stable_mono_win64_console.exe --headless --path .\Demo --scene res://Scene/MainScene.tscn --quit-after 2` 正常退出；此前的按钮 signal 错误不再出现。沙箱内 `dotnet build` 仍受本机 `NuGet.Config` / `Godot.NET.Sdk` 访问限制影响，无法直接验证完整构建。

## 2026-05-23 战斗 UI 场景布局重构

- 依据用户更新后的 `GDD_Split/06_UI_UX.md`，将 `MainUI.tscn` 从旧下方混合指令矩阵原型改为静态保留明确的 GDD 布局锚点：`EnemyTimelineHost` 位于上方，`BattleInteractionPanel` 位于左侧，`PlayerActionBanner` 与 `PlayerTimelineHost` 位于下方。
- 删除 `MainUI.tscn` / `MainScene.tscn` 中的 `GameTest` 调试菜单节点，并逐个删除未引用的 `Demo/Script/GameLogic/GameTest.cs` 与 `.uid` 文件。
- 从 `MainUI.tscn` 移除旧 `PlayerCHContainer`、旧 `HeadList`、旧 `CommandMatrix` / `CommandColumn*` 与对应 editable path；`CmdQueueUIControl` 不再暴露旧 `CommandQueueMatrix` / `commandListPrefab`，时间轴槽位继续运行时从 `CommandItem.tscn` 生成。
- 按新版设置说明保留右上角齿轮入口，设置面板内容改为“百科”和“退出游戏”；百科入口从独立右上角按钮迁入设置面板，教程高亮改为指向设置面板内的百科按钮。
- 百科打开时右侧内容改为空白初始状态，并按新版文档限制为『区域修正』与『状态效果』条目；区域顺序和状态说明同步到 `06_UI_UX.md` 表格内容。
- 将棋盘区域按钮保持隐藏、禁用并忽略鼠标，避免地图热点参与区域目标选择；区域目标仍只通过 `AreaTargetMenuControl` 的 10 个矩形按钮选择。
- 恢复我方行动横幅为文档要求：仅在准备阶段且玩家行动时显示“我方行动”，其余阶段隐藏；玩家时间轴下移以避开横幅。
- 收紧左侧战斗交互按钮显隐：场景初始和敌方准备时隐藏“设置指令/检视详情”，我方行动时显示；等待进入演出阶段时隐藏二者，只显示“开始结算”。
- 验证：`dotnet build .\Demo\Demo.csproj --no-restore` 成功，0 警告 0 错误；Godot Mono headless 直接打开 `res://Scene/MainScene.tscn` 可进入准备阶段，`Demo/godot.log` 未检出错误、异常或 C# 编译错误关键字。退出时仍有 Godot ObjectDB leak warning，未阻塞场景加载。
- 更新 `TaskBoard.md`：记录静态战斗 UI 场景重构、设置菜单百科入口、百科滚动检查和左侧交互按钮阶段显隐完成。
