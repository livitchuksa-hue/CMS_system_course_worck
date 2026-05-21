using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CMSBuilder.Models.Enums;
using CMSBuilder.ViewModels.Editor;

namespace CMSBuilder.Views.Editor;

public partial class ToolboxView : UserControl
{
    public ToolboxView()
    {
        InitializeComponent();
    }

    private void ToolboxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string tagStr) return;
        if (!Enum.TryParse<ElementType>(tagStr, out var type)) return;
        DragDrop.DoDragDrop(btn, type, DragDropEffects.Copy);
    }
}
