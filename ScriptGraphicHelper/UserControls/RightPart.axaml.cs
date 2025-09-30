using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScriptGraphicHelper.UserControls;

/// <summary>
/// 放大镜图像重绘_消息
/// </summary>
/// <param name="value"></param>
public class LoupeInvalidateVisualMessage(bool value = true) : ValueChangedMessage<bool>(value);

public partial class RightPart : UserControl
{
    public RightPart()
    {
        InitializeComponent();

        // NOTE: 修改内存中的颜色后,刷新图像 (由于没有对 vm 直接赋值,不会触发 set, 此时界面不会刷新, 只能手动调用 InvalidateVisual() 刷新)
        // 订阅 事件总线
        WeakReferenceMessenger.Default.Register<LoupeInvalidateVisualMessage>(
            this,
            (_, _) =>
            {
                // 请求重绘
                LoupeImg.InvalidateVisual();
            });
    }
}