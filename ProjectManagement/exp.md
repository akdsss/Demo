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
