using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CMSBuilder.Models.Enums;
using CMSBuilder.ViewModels.Editor;
using CMSBuilder.Views.Editor;

namespace CMSBuilder.Views.Elements;

public partial class FormElementView : UserControl
{
    public FormElementView() => InitializeComponent();

    private WebsiteEditorViewModel? FindEditorVm()
    {
        DependencyObject? p = this;
        while (p != null)
        {
            if (p is FrameworkElement { DataContext: WebsiteEditorViewModel vm })
                return vm;
            p = LogicalTreeHelper.GetParent(p);
        }
        return null;
    }

    private void Container_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(ElementType)))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void Container_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is not CanvasElementViewModel container) return;
        var vm = FindEditorVm();
        if (vm == null || !e.Data.GetDataPresent(typeof(ElementType))) return;
        var type = (ElementType)e.Data.GetData(typeof(ElementType))!;
        var pos = e.GetPosition(sender as IInputElement);
        var abs = container.GetAbsoluteBounds();
        vm.AddElementFromToolbox(type, abs.left + pos.X, abs.top + pos.Y);
        e.Handled = true;
    }

    private void ChildElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is CanvasElementViewModel el)
            EditorCanvasInteraction.SelectAndBeginDrag(el, fe, e);
        e.Handled = true;
    }
}
