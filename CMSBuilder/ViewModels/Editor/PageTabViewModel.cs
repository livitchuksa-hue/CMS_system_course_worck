using CMSBuilder.Models;
using CMSBuilder.ViewModels.Base;

namespace CMSBuilder.ViewModels.Editor;

public class PageTabViewModel : BaseViewModel
{
    private bool _isSelected;
    private string _name;

    public PageTabViewModel(Page page)
    {
        Page = page;
        _name = page.Name;
    }

    public Page Page { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                Page.Name = value;
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
