# 内容与数据结构

## 内容类型

原 GDD 至少包含以下内容：

- 玩家角色：近战、远程、辅助/支援等定位。
- 敌人：教学敌人、小型敌人、刺客、远程支援、Boss。
- 技能：移动、近战、远程、防御、治疗、特殊、范围、延迟。
- 关卡：教学、普通关、Boss 战。
- 成长：关卡胜利后的属性提升与技能解锁。
- 百科：区域、状态、规则条目。
- 美术：水墨风静态美术、UI 图标、角色/敌人头像、区域图标。
- 演出：每个指令约 3 秒战斗播片，当前阶段使用占位资源。

## Demo 范围

当前目标是 Windows 平台 3 关可玩 demo：

- 第 1 关：教学关。
- 第 2 关：小型敌方团体。
- 第 3 关：双精英战。

GDD 中出现 `4-boss战` 标题，但目前未展开具体内容。建议先把 Boss 战标记为后续扩展或待补设计，不阻塞 3 关 demo。

## 推荐配置文件

建议使用 JSON 或 CSV/TSV，但不要混用同一类数据。若策划需要表格编辑，CSV/TSV 更方便；若结构复杂，JSON 更安全。

推荐：

- `Data/characters.json`
- `Data/enemies.json`
- `Data/skills.json`
- `Data/statuses.json`
- `Data/areas.json`
- `Data/levels.json`
- `Data/growth_rewards.json`
- `Data/tutorial_steps.json`
- `Data/localization.zh-cn.json`
- `Data/assets_manifest.json`

## 角色数据

字段建议：

- `id`
- `displayName`
- `role`
- `portraitPath`
- `maxHp`
- `attack`
- `maxMp`
- `actionsPerRound`
- `initialArea`
- `skillIds`
- `growthTrackId`

GDD 当前有三名玩家角色方向：近战、远程、辅助。技能若未补充获取时间，则视为开局即拥有；关卡结束后获得或升级的技能应写入成长配置，而不是写死在角色脚本中。

## 技能数据

字段建议：

- `id`
- `displayName`
- `description`
- `tags`
- `priority`
- `mpCost`
- `targetType`
- `rangeRule`
- `durationSlots`
- `effects`
- `unlockCondition`
- `aiWeight`

## 敌人数据

字段建议：

- `id`
- `displayName`
- `enemyType`
- `maxHp`
- `attack`
- `actionsPerRound`
- `initialArea`
- `skillIds`
- `aiProfileId`
- `portraitPath`

## AI 数据

建议把 AI 拆成：

- `ai_profiles.json`：敌人使用哪个 AI 配置。
- `ai_behaviors.json`：技能权重、释放前提、目标选择。

行为字段：

- `skillId`
- `weight`
- `preconditions`
- `targetSelector`
- `preferredSlots`
- `fallbackBehavior`

## 关卡数据

字段建议：

- `id`
- `displayName`
- `chapterIndex`
- `enemyGroups`
- `playerStartAreas`
- `enemyStartAreas`
- `tutorialStepIds`
- `rewardId`
- `winCondition`
- `loseCondition`

## 成长数据

建议胜利奖励不要写在关卡脚本里。

字段建议：

- `rewardId`
- `characterId`
- `statChanges`
- `unlockSkillIds`
- `upgradeSkillIds`
- `uiSummaryText`

胜利后界面要求展示三名角色头像、各自属性变化和新技能。成长数据应能同时驱动数值变化、技能解锁和该界面展示。

## 资源清单

建议额外维护 `assets_manifest`：

- `id`
- `type`：portrait、icon、area、ui、cutscene_placeholder 等。
- `path`
- `style`：默认水墨风。
- `sourcePrompt`
- `ownerSystem`：UI、BattlePresentation、Encyclopedia 等。

## 数据保护建议

- 所有中文文本用 UTF-8 without BOM。
- 所有 ID 使用英文小写蛇形命名或短横线命名。
- 中文显示名与英文 ID 分离。
- 批量修改前先跑编码和结构校验。
- 不要从乱码 DOCX 直接生成最终配置。
