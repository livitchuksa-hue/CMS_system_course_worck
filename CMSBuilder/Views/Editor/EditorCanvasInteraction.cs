using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CMSBuilder.ViewModels.Editor;

namespace CMSBuilder.Views.Editor;

public static class EditorCanvasInteraction
{
    private static CanvasElementViewModel? _dragElement;
    private static Point _dragStartMouse;
    private static double _dragStartX;
    private static double _dragStartY;
    private static bool _isDragging;
    private static IInputElement? _captureTarget;

    public static bool IsDragging => _isDragging;

    public static void SelectAndBeginDrag(CanvasElementViewModel el, FrameworkElement fe, MouseButtonEventArgs e)
    {
        var canvas = FindDesignCanvas(fe);
        if (canvas?.DataContext is not WebsiteEditorViewModel vm) return;

        vm.SelectElementCommand.Execute(el);
        _dragElement = el;
        _dragStartMouse = e.GetPosition(canvas);
        _dragStartX = el.X;
        _dragStartY = el.Y;
        _isDragging = true;
        _captureTarget = fe;
        fe.CaptureMouse();
    }

    public static void HandleMouseMove(MouseEventArgs e)
    {
        if (!_isDragging || _dragElement == null) return;
        var canvas = FindDesignCanvas(_captureTarget as DependencyObject);
        if (canvas == null) return;

        var pos = e.GetPosition(canvas);
        var dx = pos.X - _dragStartMouse.X;
        var dy = pos.Y - _dragStartMouse.Y;

        if (_dragElement.Parent != null)
        {
            var maxX = (_dragElement.Parent.Width ?? 300) - 40;
            var maxY = (_dragElement.Parent.Height ?? 120) - 30;
            _dragElement.X = Math.Max(0, Math.Min(maxX, _dragStartX + dx));
            _dragElement.Y = Math.Max(0, Math.Min(maxY, _dragStartY + dy));
        }
        else if (canvas.DataContext is WebsiteEditorViewModel vm)
        {
            var maxX = vm.CanvasWidth - 40;
            var maxY = vm.CanvasHeight - 30;
            _dragElement.X = Math.Max(0, Math.Min(maxX, _dragStartX + dx));
            _dragElement.Y = Math.Max(0, Math.Min(maxY, _dragStartY + dy));
        }
    }

    public static void EndDrag()
    {
        if (!_isDragging || _dragElement == null) return;
        Mouse.Capture(null);

        var canvas = FindDesignCanvas(_captureTarget as DependencyObject);
        if (canvas?.DataContext is WebsiteEditorViewModel vm)
        {
            var abs = GetAbsolutePosition(_dragElement);
            if (_dragElement.Parent != null)
            {
                var pb = _dragElement.Parent.GetAbsoluteBounds();
                if (abs.x < pb.left + 4 || abs.y < pb.top + 4 ||
                    abs.x > pb.right - 24 || abs.y > pb.bottom - 24)
                    vm.DetachFromContainer(_dragElement);
            }

            abs = GetAbsolutePosition(_dragElement);
            var (container, relX, relY) = vm.ResolveContainerAt(abs.x + 16, abs.y + 16);
            if (container != null && container != _dragElement && !IsDescendant(_dragElement, container))
                vm.AttachToContainer(_dragElement, container, relX, relY);

            _dragElement.SyncToModel();
            vm.PersistSelectedElement();
        }

        _isDragging = false;
        _dragElement = null;
        _captureTarget = null;
    }

    private static (double x, double y) GetAbsolutePosition(CanvasElementViewModel el)
    {
        var b = el.GetAbsoluteBounds();
        return (b.left + 8, b.top + 8);
    }

    private static bool IsDescendant(CanvasElementViewModel ancestor, CanvasElementViewModel node)
    {
        foreach (var child in ancestor.Children)
        {
            if (child == node || IsDescendant(child, node)) return true;
        }
        return false;
    }

    private static Canvas? FindDesignCanvas(DependencyObject? start)
    {
        while (start != null)
        {
            if (start is EditorCanvasView view)
                return view.DesignCanvasControl;
            start = LogicalTreeHelper.GetParent(start) ?? VisualTreeHelper.GetParent(start);
        }
        return null;
    }
}
