using CMSBuilder.Models;
using CMSBuilder.ViewModels.Base;

namespace CMSBuilder.ViewModels.Editor;

public class PageSettingsItemViewModel : BaseViewModel
{
    private string _name;
    private string _slug;

    public PageSettingsItemViewModel(Page page)
    {
        Page = page;
        _name = page.Name;
        _slug = page.Slug;
    }

    public Page Page { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Slug
    {
        get => _slug;
        set => SetProperty(ref _slug, value);
    }
}
