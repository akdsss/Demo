# GDD 拆分说明

来源文档：

- `D:\GodotProject\202605gamejam\generalGDDforAI.md`
- `D:\GodotProject\202605gamejam\GDD_Split\DevGuidelines.md`

本目录用于把原始 GDD 拆成便于并行开发的短文档。拆分目标不是替代原始设计文档，而是把战斗内核、内容数据、UI、项目结构、并行 worktree 分工等开发主题分离，降低后续多轮对话中的上下文消耗和遗忘风险。

## 编码核验结论

`generalGDDforAI.md` 与 `DevGuidelines.md` 已确认是 UTF-8 without BOM。此前看到的中文乱码来自终端/工具输出显示链路，不代表文件内容损坏。

后续若再次出现中文显示异常，应先检查文件字节、BOM 与 UTF-8 解码结果，再判断文件是否真的损坏。不要只根据 PowerShell 或终端里的中文显示效果下结论。

## 文档索引

- `01_DesignReview.md`：设计文档评审、风险、可优化点，以及编码误判防范流程。
- `02_CoreLoop_Timeline.md`：回合、准备阶段、演出阶段、时间轴。
- `03_CombatRules_Resolution.md`：技能合法性、优先级、伤害/治疗/状态结算。
- `04_Areas_Statuses.md`：八卦区域、区域修正、状态效果。
- `05_Content_Data.md`：角色、敌人、技能、关卡、成长数据的结构化建议。
- `06_UI_UX.md`：战斗 UI、百科、教程、结算/成长界面。
- `07_GodotProjectStructure.md`：推荐 Godot C# 项目目录结构。
- `08_ParallelWorktrees.md`：适合多个 worktree 同步开发的任务切分。

## 当前开发范围

根据更新后的 `generalGDDforAI.md`：

- 目标平台：Windows。
- 使用语言：C#
- 生成范围：完成文档所有要求，做出包含 3 个关卡的可玩 demo。
- 美术资源：演出阶段每个指令约 3 秒的战斗播片先用占位资源搭建，不要求现在生成；其他缺少的美术资源由 AI 生成，风格优先考虑水墨风。
- 音乐资源：BGM 暂不考虑；攻击、受击等战斗相关音效也暂不考虑，因为战斗以播片方式演出。
- UI 音效：准备阶段提示、怪物行动放置、玩家确认指令、控件 hover/click 等仍在 GDD 中出现，建议作为轻量 UI SFX 单独处理，和攻击/受击类战斗音效分开。

## 使用原则

- 原始 GDD 仍是权威设计来源。
- 拆分文档用于开发沟通、任务切分和实现约束。
- 中文显示名、技能名、状态名、数值和特殊规则，以 `generalGDDforAI.md` 为准。
- AI 开发约束、编码保护和删除限制，以 `DevGuidelines.md` 与项目根目录中的 AGENTS 指令为准。
- 若拆分文档与原始 GDD 不一致，应先记录差异，再由设计者确认。
