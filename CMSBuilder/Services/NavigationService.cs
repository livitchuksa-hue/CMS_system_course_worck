using System.Windows;
using System.Windows.Controls;

namespace CMSBuilder.Services;

public class NavigationService
{
    private Frame? _frame;

    public void SetFrame(Frame frame) => _frame = frame;

    public void Navigate(UIElement view)
    {
        if (_frame == null) throw new InvalidOperationException("Frame not set.");
        _frame.Content = view;
    }

    public void Navigate<TView>() where TView : UIElement, new()
    {
        Navigate(new TView());
    }
}
