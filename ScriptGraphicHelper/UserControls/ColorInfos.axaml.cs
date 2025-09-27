using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using ScriptGraphicHelper.Views;

namespace ScriptGraphicHelper.UserControls
{
    public partial class ColorInfos : UserControl
    {
        public ColorInfos()
        {
            this.InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            e.Row.PointerEntered += (object? sender, PointerEventArgs _) =>
            {
                dataGrid.SelectedIndex = e.Row.Index;
            };
        }
    }
}
