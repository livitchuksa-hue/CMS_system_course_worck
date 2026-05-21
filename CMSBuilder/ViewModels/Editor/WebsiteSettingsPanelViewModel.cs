using System.Collections.ObjectModel;
using CMSBuilder.Core;
using CMSBuilder.Helpers;
using CMSBuilder.Models;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Base;

namespace CMSBuilder.ViewModels.Editor;

public class WebsiteSettingsPanelViewModel : BaseViewModel
{
    private readonly WebsiteService _websites;
    private readonly PageService _pages;
    private readonly int _websiteId;
    private readonly Action _onSaved;
    private readonly bool _canSave;

    private string _siteTitle = string.Empty;
    private string _metaDescription = string.Empty;
    private string _slug = string.Empty;
    private double _pageWidth = 960;
    private double _pageHeight = 640;
    private string _pageBackground = "#FFFFFF";
    private string _primaryColor = "#1B6B3A";
    private string _secondaryColor = "#5C6B64";
    private string _accentColor = "#2E8B57";
    private string _selectedPreset = "960 × 640";
    private string _statusMessage = string.Empty;

    public WebsiteSettingsPanelViewModel(int websiteId, Action onSaved, bool canSave = true)
    {
        _websiteId = websiteId;
        _onSaved = onSaved;
        _canSave = canSave;
        _websites = App.Websites;
        _pages = App.Pages;

        PageItems = new ObservableCollection<PageSettingsItemViewModel>();
        SiteComments = new ObservableCollection<SiteCommentRowViewModel>();
        SizePresets = new ObservableCollection<string>
        {
            "960 × 640",
            "1280 × 720",
            "1440 × 900",
            "1920 × 1080"
        };

        SaveCommand = new RelayCommand(Save, () => _canSave);
        ApplyPresetCommand = new RelayCommand(ApplyPreset, () => _canSave);
        SyncSlugFromTitleCommand = new RelayCommand(() => Slug = WebsiteService.Slugify(SiteTitle), () => _canSave);
        RefreshCommentsCommand = new RelayCommand(LoadSiteComments);

        Load();
    }

    public ObservableCollection<PageSettingsItemViewModel> PageItems { get; }
    public ObservableCollection<SiteCommentRowViewModel> SiteComments { get; }
    public ObservableCollection<string> SizePresets { get; }
    public bool CanSave => _canSave;

    public string SiteTitle
    {
        get => _siteTitle;
        set => SetProperty(ref _siteTitle, value);
    }

    public string MetaDescription
    {
        get => _metaDescription;
        set => SetProperty(ref _metaDescription, value);
    }

    public string Slug
    {
        get => _slug;
        set => SetProperty(ref _slug, value);
    }

    public double PageWidth
    {
        get => _pageWidth;
        set => SetProperty(ref _pageWidth, value);
    }

    public double PageHeight
    {
        get => _pageHeight;
        set => SetProperty(ref _pageHeight, value);
    }

    public string PageBackground
    {
        get => _pageBackground;
        set => SetProperty(ref _pageBackground, value);
    }

    public string PrimaryColor
    {
        get => _primaryColor;
        set => SetProperty(ref _primaryColor, value);
    }

    public string SecondaryColor
    {
        get => _secondaryColor;
        set => SetProperty(ref _secondaryColor, value);
    }

    public string AccentColor
    {
        get => _accentColor;
        set => SetProperty(ref _accentColor, value);
    }

    public string SelectedPreset
    {
        get => _selectedPreset;
        set => SetProperty(ref _selectedPreset, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand ApplyPresetCommand { get; }
    public RelayCommand SyncSlugFromTitleCommand { get; }
    public RelayCommand RefreshCommentsCommand { get; }

    public void ReloadPages()
    {
        PageItems.Clear();
        foreach (var p in _pages.GetPages(_websiteId))
            PageItems.Add(new PageSettingsItemViewModel(p));
    }

    public void Load()
    {
        var website = _websites.GetById(_websiteId);
        if (website == null) return;

        SiteTitle = website.Settings?.SiteTitle ?? website.Name;
        MetaDescription = website.Settings?.MetaDescription ?? website.Description;
        Slug = website.Slug;
        PageWidth = website.Settings?.PageWidth ?? ExportLayoutHelper.CanvasWidth;
        PageHeight = website.Settings?.PageHeight ?? ExportLayoutHelper.CanvasHeight;
        PageBackground = website.Settings?.PageBackgroundColor ?? "#FFFFFF";
        PrimaryColor = website.Theme?.PrimaryColor ?? "#1B6B3A";
        SecondaryColor = website.Theme?.SecondaryColor ?? "#5C6B64";
        AccentColor = website.Theme?.AccentColor ?? "#2E8B57";
        SelectedPreset = $"{(int)PageWidth} × {(int)PageHeight}";
        ReloadPages();
        LoadSiteComments();
    }

    private void LoadSiteComments()
    {
        SiteComments.Clear();
        foreach (var c in App.Comments.GetForWebsite(_websiteId))
        {
            var pageName = _pages.GetPages(_websiteId).FirstOrDefault(p => p.Id == c.PageId)?.Name ?? $"#{c.PageId}";
            SiteComments.Add(new SiteCommentRowViewModel(c, pageName));
        }
    }

    private void ApplyPreset()
    {
        var parts = SelectedPreset.Split('×', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 &&
            double.TryParse(parts[0], out var w) &&
            double.TryParse(parts[1], out var h))
        {
            PageWidth = w;
            PageHeight = h;
        }
    }

    private void Save()
    {
        if (!_canSave) return;

        _websites.UpdateWebsiteCore(_websiteId, SiteTitle, MetaDescription, Slug);
        _websites.UpdateWebsiteSettings(_websiteId, s =>
        {
            s.SiteTitle = SiteTitle;
            s.MetaDescription = MetaDescription;
            s.BaseUrl = Slug;
            s.PageWidth = PageWidth;
            s.PageHeight = PageHeight;
            s.PageBackgroundColor = PageBackground;
        });
        _websites.UpdateSiteTheme(_websiteId, t =>
        {
            t.PrimaryColor = PrimaryColor;
            t.SecondaryColor = SecondaryColor;
            t.AccentColor = AccentColor;
        });

        foreach (var item in PageItems)
        {
            item.Page.Name = item.Name;
            item.Page.Slug = item.Slug;
            _pages.UpdatePage(item.Page);
        }

        StatusMessage = "Настройки сохранены.";
        _onSaved();
    }
}

public class SiteCommentRowViewModel
{
    public SiteCommentRowViewModel(PageComment comment, string pageName)
    {
        AuthorName = comment.AuthorName;
        Text = comment.Text;
        PageName = pageName;
        CreatedAt = comment.CreatedAt.ToLocalTime().ToString("g");
    }

    public string AuthorName { get; }
    public string Text { get; }
    public string PageName { get; }
    public string CreatedAt { get; }
}
