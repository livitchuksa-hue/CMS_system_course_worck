using System.Windows;

using System.Windows.Controls;

using System.Windows.Input;

using CMSBuilder.Models.Enums;

using CMSBuilder.ViewModels.Editor;



namespace CMSBuilder.Views.Editor;



public partial class EditorCanvasView : UserControl

{

    public EditorCanvasView() => InitializeComponent();



    public Canvas DesignCanvasControl => DesignCanvas;



    private WebsiteEditorViewModel? Vm => DataContext as WebsiteEditorViewModel;



    private void EditorCanvas_DragOver(object sender, DragEventArgs e)

    {

        if (Vm?.CanEdit == true && e.Data.GetDataPresent(typeof(ElementType)))

            e.Effects = DragDropEffects.Copy;

        else

            e.Effects = DragDropEffects.None;

        e.Handled = true;

    }



    private void EditorCanvas_Drop(object sender, DragEventArgs e)

    {

        if (Vm == null || !Vm.CanEdit || !e.Data.GetDataPresent(typeof(ElementType))) return;

        var type = (ElementType)e.Data.GetData(typeof(ElementType))!;

        var pos = e.GetPosition(DesignCanvas);

        Vm.AddElementFromToolbox(type, Math.Max(0, pos.X), Math.Max(0, pos.Y));

        e.Handled = true;

    }



    private void DesignCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

    {

        if (EditorCanvasInteraction.IsDragging) return;

        Vm?.SelectElementCommand.Execute(null);

    }



    private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

    {

        if (Vm?.CanEdit != true) return;

        if (sender is FrameworkElement fe && fe.DataContext is CanvasElementViewModel el)

            EditorCanvasInteraction.SelectAndBeginDrag(el, fe, e);

        e.Handled = true;

    }



    private void DesignCanvas_MouseMove(object sender, MouseEventArgs e) =>

        EditorCanvasInteraction.HandleMouseMove(e);



    private void DesignCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>

        EditorCanvasInteraction.EndDrag();



    private void DesignCanvas_MouseLeave(object sender, MouseEventArgs e) =>

        EditorCanvasInteraction.EndDrag();

}


