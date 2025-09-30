using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScriptGraphicHelper.UserControls;

/// <summary>
/// 聚焦到生成代码按钮_消息
/// </summary>
/// <param name="value"></param>
public class FocusCreateButtonMessage(bool value = true) : ValueChangedMessage<bool>(value);

public partial class LeftPart : UserControl
{
    public LeftPart()
    {
        InitializeComponent();

        // 聚焦到 "生成代码" 按钮
        WeakReferenceMessenger.Default.Register<FocusCreateButtonMessage>(
            this,
            (_, _) => { Create_btn.Focus(); }
        );
    }
}