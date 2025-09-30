using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ScriptGraphicHelper.UserControls;

public partial class CenterImage : UserControl
{
    public CenterImage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 鼠标进入主图片_事件
    /// </summary>
    private void Image_PointerEntered_Focus(object? sender, PointerEventArgs e)
    {
        // NOTE: 焦点在 DataGrid 时,
        // 键盘方向键 会被拦截不再传播, 功能变成 "上下选择行",
        // 导致无法让鼠标移动 1 像素,
        // 此时鼠标进入图片, 强行聚焦一个按钮上,
        // 可以让 方向键 恢复功能
        // Image 组件估计是无法聚焦, 调用 Focus() 一直返回 false,
        // 其他组件会返回 true  
        // 全局焦点管理器中的 ClearFocus() 在未来会被删除, 所以不用

        // 聚焦到 "生成代码" 按钮
        WeakReferenceMessenger.Default.Send(new FocusCreateButtonMessage());
    }
}