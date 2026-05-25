# 百科面板居中问题复盘

## 问题现象

百科面板最初打开时贴在屏幕左侧，不符合“百科显示区域从左端移动至屏幕中间”的要求。第一次修改后仍然没有稳定居中，第二次修改后才成功。

相关代码位置：

- `Demo/Script/UIControl/EncyclopediaOverlayControl.cs`

## 第一次为什么失败

第一次的思路偏“调面板自身锚点”：把百科面板从原来的位置改成更靠中间的百分比锚点，例如用类似 `AnchorLeft = 0.22f`、`AnchorRight = 0.78f` 这样的方式，希望面板按父节点宽度的 22% 到 78% 显示。

这个思路失败的关键原因是：只调整子面板锚点，没有先保证它的父级 Overlay 真的覆盖整个视口。

`EncyclopediaOverlayControl` 是运行时动态创建并挂到 `MainUIControl` 下的 `Control`。如果这个顶层 `Control` 的实际 `Size` / `Offset` / 视口适配状态没有被明确写死为当前窗口尺寸，那么子节点的百分比锚点就是相对一个不可靠的父矩形计算的。结果是：

- 子面板看起来“改了锚点”，但锚点参考系可能不是整个屏幕。
- 父级尺寸如果还没完成布局，或者仍然接近左上角的默认区域，子面板依然可能贴左。
- 这种做法把“居中”寄托在父容器正确布局上，问题不在面板自身，而在参考坐标系不稳定。

简单说：第一次是在错误的参考系里调位置，所以看起来改了数字，但没有真正解决“相对于屏幕居中”的问题。

## 第二次换了什么思路

第二次不再只调子面板百分比，而是分两步处理：

1. 先把百科 Overlay 自己强制适配整个视口。
2. 再让百科面板以屏幕中心点为锚点，用固定宽度左右展开。

这等于先把坐标系校准，再做居中布局。

## 第二次的关键代码

当前成功版本在 `EncyclopediaOverlayControl.cs` 中增加了固定面板宽度：

```csharp
private const float PanelWidth = 720f;
```

在构建 UI 时，先让 Overlay 变成全屏：

```csharp
private void BuildOverlay()
{
    SetAnchorsPreset(LayoutPreset.FullRect);
    FitToViewport();
    MouseFilter = MouseFilterEnum.Ignore;
    ...
}
```

然后让面板以屏幕水平中心为锚点：

```csharp
panel.AnchorLeft = 0.5f;
panel.AnchorRight = 0.5f;
panel.AnchorTop = 0.10f;
panel.AnchorBottom = 0.90f;
panel.OffsetLeft = -PanelWidth * 0.5f;
panel.OffsetRight = PanelWidth * 0.5f;
panel.OffsetTop = 0f;
panel.OffsetBottom = 0f;
```

这里的意思是：

- `AnchorLeft = 0.5f` 和 `AnchorRight = 0.5f`：左右锚点都放在屏幕中心线。
- `OffsetLeft = -360`、`OffsetRight = 360`：面板从中心线向左右各展开半个宽度。
- 结果面板中心永远落在屏幕中心线上，不再依赖 0.22/0.78 这种百分比范围。

同时新增 `FitToViewport()`，明确把 Overlay 的参考系设置成当前视口：

```csharp
private void FitToViewport()
{
    AnchorLeft = 0f;
    AnchorTop = 0f;
    AnchorRight = 1f;
    AnchorBottom = 1f;
    OffsetLeft = 0f;
    OffsetTop = 0f;
    OffsetRight = 0f;
    OffsetBottom = 0f;
    Size = GetViewportRect().Size;
}
```

还在窗口尺寸变化和打开百科时再次刷新：

```csharp
public override void _Notification(int what)
{
    if (what == NotificationResized)
    {
        FitToViewport();
    }
}

public void OpenEncyclopedia()
{
    FitToViewport();
    panel.Visible = true;
    panel.MoveToFront();
    Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.OpenEncyclopedia);
}
```

这样可以防止运行时窗口尺寸变化、UI 初始化顺序、动态 AddChild 等因素导致 Overlay 尺寸不对。

## 成功的核心原因

第二次成功不是因为单纯换了几个锚点数字，而是改掉了定位模型：

- 第一次：让面板按父级百分比显示，但父级参考系不可靠。
- 第二次：先强制父级 Overlay 等于视口，再让面板围绕视口中心线展开。

这类运行时动态 UI 的可靠做法是：

1. 先确认最外层 `Control` 的实际矩形是否就是屏幕。
2. 需要绝对居中时，优先使用 `AnchorLeft = AnchorRight = 0.5f` 加左右 offset。
3. 不要只看子节点锚点；父节点尺寸、挂载位置、初始化时机同样决定最终位置。
4. 对会随窗口变化的 UI，在 `NotificationResized` 或打开界面时重新同步视口尺寸。

## 后续开发建议

类似百科、奖励面板、教程提示这类“覆盖全屏的弹层”，建议统一采用：

- 顶层 Overlay：`FullRect + FitToViewport()`
- 弹窗 Panel：中心锚点 `0.5f / 0.5f`，再用固定或响应式 offset 控制宽度
- 打开时：`FitToViewport()` + `MoveToFront()`

这样可以避免“看起来写的是居中，实际运行却贴边”的布局问题。

# 战斗交互角色选择菜单点击问题复盘

## 问题现象

本轮问题发生在战斗交互 UI 的“设置指令”流程中。

相关菜单层级：

- `TimelineControlBar`：一级操作菜单，包含 `SetCommandButton`、`InspectButton`、`StartSettlementButton`。
- `ChoiceMenuView.PlayerSelect`：行动角色选择面板，点击 `SetCommandButton` 后出现，显示三张角色大图。
- `ChoiceMenuView.CommandCategory`：技能分类面板，选择角色后出现，显示近战/位移/远程/特殊。
- `ChoiceMenuView.SkillList`：技能列表面板。
- `ChoiceMenuView.CharacterTargetList`：角色目标选择面板。

先后出现了两个表象：

1. 第二轮次点击 `SetCommandButton` 后，下级菜单位置错误，被 `TimelineControlBar` 覆盖，不在“设置指令”下方，导致无法交互。
2. 修复覆盖问题后，第二轮次进入 `ChoiceMenuView.CommandCategory`，技能分类按钮看得到，但点击无反应。

## 问题代码

相关代码位置：

- `Demo/Script/UIControl/MainUIControl/PlayerChoseListPanelControl.cs`
- `Demo/Script/UIControl/CmdQueueUIControl.cs`
- `Demo/Script/UIControl/MainUIControl/CommandItemUIControl.cs`

最初的问题不是某个按钮没有连接 `Pressed`，而是菜单层级和节点职责混在一起。

旧思路中，`ChoseListPanel` 同时承担两种职责：

- 显示行动角色选择大图。
- 显示左侧紧凑文本菜单，包括 `CommandCategory`、`SkillList`、`CharacterTargetList`。

为了让三张角色大图出现在屏幕左侧中间偏下，代码曾让 `ChoseListPanel` 临时离开左侧 `BattleInteractionPanel`，改挂到 `MainUi/Panel`，并写入屏幕绝对定位的锚点和 offset。进入下一层技能分类时，再把同一个 `ChoseListPanel` 移回左侧容器。

这个模型的问题在于：同一个 `Control` 被反复切换父节点、布局模式、锚点、offset、可见性和鼠标命中区域。Godot 的 `Control` 布局状态很容易在这种跨容器复用中留下残留，尤其是第二轮次反复进入/退出菜单后，残留状态更容易暴露。

## 第一次修复思路：只隐藏一级菜单

第一次修复重点放在 `TimelineControlBar` 上：

- 打开下级菜单时调用 `SetCommandSubmenuOpen(true)`。
- 让 `TimelineControlBar.Visible = false`。
- 从 `ChoiceMenuView.PlayerSelect` 继续返回时保持一级菜单隐藏。
- 只有真正关闭整条选择链，或成功放置指令后，才恢复一级菜单。

这个思路解决的是“一级菜单覆盖下级菜单”的问题。它是必要的，因为一级操作菜单和下级指令菜单确实应该互斥显示。

但它没有解决后续 `ChoiceMenuView.CommandCategory` 点击无反应的问题。原因是：技能分类按钮失效并不只是一级菜单挡住了它，还可能是 `ChoseListPanel` 自己的布局/输入命中状态已经被上一层大图面板污染。

## 第二次修复思路：复位 `ChoseListPanel`

第二次修复试图在进入紧凑菜单前强制复位 `ChoseListPanel`：

- 进入 `PrepareCompactPanel()` 时恢复锚点和 offset。
- 增加 `ResetCompactPanelRect()`。
- 设置 `MouseFilter = Pass`。
- 重新设置 `CustomMinimumSize`、`SizeFlagsHorizontal`、`SizeFlagsVertical`。

这个思路看起来合理，因为按钮“看得到但点不到”很像命中区域错位。

但它仍然失败。失败原因是：这仍然沿用同一个根模型，即同一个 `ChoseListPanel` 既作为屏幕绝对定位的大图层，又作为左侧布局容器内的文本菜单。复位只能修补一部分显式属性，不能保证所有由 reparent、布局时机、父容器排序、输入分发造成的状态都被完全还原。

简单说：第二次仍然是在补救一个不稳定结构，而不是移除这个结构风险。

## 成功修复思路：拆分节点职责

最终成功的思路是拆分职责，不再让 `ChoseListPanel` 来回搬家。

当前结构：

- `PlayerSelectOverlay`：运行时创建的独立 overlay，只负责 `ChoiceMenuView.PlayerSelect` 的三张角色大图。
- `ChoseListPanel`：永远待在左侧 `BattleInteractionPanel` 内，只负责 `CommandCategory`、`SkillList`、`CharacterTargetList`。

关键变化：

1. `ShowPlayerSelectPanel()` 不再移动 `ChoseListPanel`。
2. `ShowPlayerSelectPanel()` 创建或复用 `PlayerSelectOverlay`，把三张角色图加到 overlay 上。
3. 进入 `ShowCommandCategoryPanel()`、`ShowSkillList()`、`ShowCharacterTargetList()` 时调用 `HidePlayerSelectOverlay()`，清空并隐藏角色大图层。
4. `PrepareCompactPanel()` 只负责左侧紧凑菜单布局，不再需要从屏幕层恢复父节点。
5. 外部重置菜单时通过 `DismissPanel()` 同时隐藏 `ChoseListPanel` 和 `PlayerSelectOverlay`，避免 overlay 残留。

成功后的职责划分更清楚：

```text
TimelineControlBar
  一级操作菜单：设置指令 / 检视详情 / 开始结算

PlayerSelectOverlay
  行动角色选择面板：三张角色大图

ChoseListPanel
  技能分类面板
  技能列表面板
  角色目标选择面板
```

这样 `ChoiceMenuView.CommandCategory` 的按钮不再继承角色大图层的绝对定位、父节点、输入区域或显示顺序，点击无反应的问题从结构上被消除。

## 同步完成的交互优化

本轮同时完成了两个交互增强：

- 行动角色选择面板中，鼠标悬停可行动角色大图时，除了角色图自身高亮，也会通过 `CmdQueueUIControl.SetHoveredTimelineRow()` 高亮对应玩家时间轴整行。
- 在 `ChoiceMenuView.CharacterTargetList` 中，鼠标悬停目标对象按钮时，会在屏幕右下角显示 `TargetHoverPreviewImage`，图像来源与该目标的 `characterHeadImage` 一致。

## 后续开发建议

这次问题的核心经验是：不要让同一个 `Control` 同时承担“屏幕 overlay”和“局部菜单容器”两种布局职责。

后续类似 UI 建议遵循：

1. 屏幕绝对定位的大图、弹窗、遮罩层，使用独立 overlay 节点。
2. 左侧菜单、列表按钮、技能分类等局部 UI，固定留在自己的局部父容器中。
3. 不要为了解决视觉位置，把已有菜单容器反复 `RemoveChild()` / `AddChild()` 到不同父节点。
4. 如果必须跨容器移动节点，需要额外验证锚点、offset、layout mode、mouse filter、z-index、父节点可见性和输入遮挡；但更推荐拆分节点。
5. 当一次修复只能解释部分表象，例如“覆盖问题修了但按钮仍点不了”，应优先怀疑结构模型，而不是继续堆叠属性复位。

本次最终采用“独立 `PlayerSelectOverlay` + 固定 `ChoseListPanel`”后，代码职责更稳定，也方便之后继续调整行动角色图片位置和左侧文本菜单样式。
