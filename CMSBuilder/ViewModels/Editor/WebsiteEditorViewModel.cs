using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using CMSBuilder.Core;
using CMSBuilder.Helpers;
using CMSBuilder.Models;
using CMSBuilder.Models.Dto;
using CMSBuilder.Models.Enums;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Account;
using CMSBuilder.ViewModels.Base;
using CMSBuilder.ViewModels.Dashboard;

namespace CMSBuilder.ViewModels.Editor;

public class WebsiteEditorViewModel : BaseViewModel
{
    private readonly NavigationService _navigation;
    private readonly ElementService _elementService;
    private readonly PageService _pageService;
    private readonly WebsiteService _websiteService;
    private readonly int _websiteId;

    private string _websiteName = string.Empty;
    private string _userRole = string.Empty;
    private string _statusMessage = string.Empty;
    private PageTabViewModel? _selectedPage;
    private CanvasElementViewModel? _selectedElement;

    // Button action editing
    private ActionType _buttonActionType = ActionType.None;
    private int? _buttonTargetPageId;
    private int? _buttonTargetElementId;
    private string _buttonTargetUrl = string.Empty;
    private string _buttonCustomJs = string.Empty;
    private ObservableCollection<Page> _pagesForAction = new();
    private ObservableCollection<ElementRefOption> _formsOnPage = new();
    private ObservableCollection<ElementRefOption> _buttonsOnPage = new();
    private ObservableCollection<ElementRefOption> _commentsBlocksOnPage = new();
    private ObservableCollection<PageComment> _blockComments = new();
    private string _newCommentAuthor = string.Empty;
    private string _newCommentText = string.Empty;
    private string _galleryImagesText = string.Empty;
    private string _navItemsText = string.Empty;
    private string _faqItemsText = string.Empty;
    private bool _isSettingsOpen;
    private bool _isOrganizationOpen;
    private double _canvasWidth = 960;
    private double _canvasHeight = 640;
    private string _canvasBackground = "#FFFFFF";
    private string _roleCode = "owner";
    private bool _canEdit = true;
    private bool _canChangeSettings = true;
    private bool _canManageTeam = true;

    public WebsiteEditorViewModel(NavigationService navigation, int websiteId, AccountViewModel? account = null)
    {
        _navigation = navigation;
        Account = account ?? App.Account;
        _websiteId = websiteId;
        _elementService = App.Elements;
        _pageService = App.Pages;
        _websiteService = App.Websites;

        PageTabs = new ObservableCollection<PageTabViewModel>();
        CanvasElements = new ObservableCollection<CanvasElementViewModel>();

        _roleCode = WebsitePermissions.NormalizeRoleCode(SessionContext.CurrentUserRoleCode);
        _canEdit = WebsitePermissions.CanEditCanvas(_roleCode);
        _canChangeSettings = WebsitePermissions.CanManageSettings(_roleCode);
        _canManageTeam = WebsitePermissions.CanManageTeam(_roleCode);

        BackCommand = new RelayCommand(GoBack);
        SaveCommand = new RelayCommand(SaveProject, () => CanEdit);
        LaunchSiteCommand = new RelayCommand(LaunchSite, () => CanLaunch);
        AddPageCommand = new RelayCommand(AddPage, () => CanEdit);
        SelectPageCommand = new RelayCommand<PageTabViewModel>(SelectPage);
        SelectElementCommand = new RelayCommand<object?>(p => SelectElement(p as CanvasElementViewModel));
        DeleteElementCommand = new RelayCommand(DeleteSelected, () => CanEdit && SelectedElement != null);
        ApplyButtonActionCommand = new RelayCommand(ApplyButtonAction, () => CanEdit && (SelectedElement?.Type == ElementType.Button || SelectedElement?.Type == ElementType.Card));
        AddNavItemCommand = new RelayCommand(AddNavItem, () => IsNavbarSelected);
        AddCommentCommand = new RelayCommand(AddComment, () => IsCommentElementSelected && SelectedPage != null);
        RefreshCommentsCommand = new RelayCommand(LoadBlockComments, () => IsCommentElementSelected);
        OpenAccountCommand = Account.ToggleCommand;
        var canDeleteWebsite = _roleCode == "owner";
        Settings = new WebsiteSettingsPanelViewModel(_websiteId, OnSettingsSaved, OnWebsiteDeleted, CanChangeSettings, canDeleteWebsite);
        Organization = new OrganizationPanelViewModel(_websiteId, msg => StatusMessage = msg, GoBack);
        ToggleSettingsCommand = new RelayCommand(() =>
        {
            if (!CanChangeSettings) return;
            IsSettingsOpen = !IsSettingsOpen;
            if (IsSettingsOpen) Settings.Load();
        }, () => CanChangeSettings);
        ToggleOrganizationCommand = new RelayCommand(() =>
        {
            if (!CanManageTeam) return;
            IsOrganizationOpen = !IsOrganizationOpen;
            if (IsOrganizationOpen) Organization.Load();
        }, () => CanManageTeam);

        LoadWebsite();
    }

    public WebsiteSettingsPanelViewModel Settings { get; }
    public OrganizationPanelViewModel Organization { get; }

    public AccountViewModel Account { get; }

    public ObservableCollection<PageTabViewModel> PageTabs { get; }
    public ObservableCollection<CanvasElementViewModel> CanvasElements { get; }

    public string WebsiteName
    {
        get => _websiteName;
        set => SetProperty(ref _websiteName, value);
    }

    public string UserRole
    {
        get => _userRole;
        set => SetProperty(ref _userRole, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public PageTabViewModel? SelectedPage
    {
        get => _selectedPage;
        set => SetProperty(ref _selectedPage, value);
    }

    public CanvasElementViewModel? SelectedElement
    {
        get => _selectedElement;
        set
        {
            if (_selectedElement != null && !ReferenceEquals(_selectedElement, value))
                SaveElementToDb(_selectedElement);

            if (_selectedElement != null)
                _selectedElement.PropertyChanged -= OnSelectedElementPropertyChanged;

            if (SetProperty(ref _selectedElement, value))
            {
                if (_selectedElement != null)
                    _selectedElement.PropertyChanged += OnSelectedElementPropertyChanged;
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(IsTextSelected));
                OnPropertyChanged(nameof(IsHeaderSelected));
                OnPropertyChanged(nameof(IsImageSelected));
                OnPropertyChanged(nameof(IsButtonSelected));
                OnPropertyChanged(nameof(IsCardSelected));
                OnPropertyChanged(nameof(IsInputSelected));
                OnPropertyChanged(nameof(IsContainerSelected));
                OnPropertyChanged(nameof(IsNavbarSelected));
                OnPropertyChanged(nameof(IsGallerySelected));
                OnPropertyChanged(nameof(IsLinkSelected));
                OnPropertyChanged(nameof(IsDividerSelected));
                OnPropertyChanged(nameof(IsSpacerSelected));
                OnPropertyChanged(nameof(IsFormSelected));
                OnPropertyChanged(nameof(IsFooterSelected));
                OnPropertyChanged(nameof(IsFaqSelected));
                OnPropertyChanged(nameof(IsCommentsSelected));
                OnPropertyChanged(nameof(IsCommentsViewerSelected));
                OnPropertyChanged(nameof(IsCommentElementSelected));
                OnPropertyChanged(nameof(IsVideoEmbedSelected));
                OnPropertyChanged(nameof(IsVideoPlayerSelected));
                OnPropertyChanged(nameof(IsCheckboxSelected));
                OnPropertyChanged(nameof(IsTextAreaSelected));
                OnPropertyChanged(nameof(IsSliderSelected));
                AddCommentCommand.RaiseCanExecuteChanged();
                RefreshCommentsCommand.RaiseCanExecuteChanged();
                LoadButtonActionFromSelection();
                RefreshElementRefLists();
                LoadNavItemsText();
                LoadGalleryImagesText();
                LoadFaqItemsText();
                LoadBlockComments();
                DeleteElementCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<ElementRefOption> FormsOnPage
    {
        get => _formsOnPage;
        set => SetProperty(ref _formsOnPage, value);
    }

    public ObservableCollection<ElementRefOption> ButtonsOnPage
    {
        get => _buttonsOnPage;
        set => SetProperty(ref _buttonsOnPage, value);
    }

    public ObservableCollection<ElementRefOption> CommentsBlocksOnPage
    {
        get => _commentsBlocksOnPage;
        set => SetProperty(ref _commentsBlocksOnPage, value);
    }

    public ObservableCollection<PageComment> BlockComments
    {
        get => _blockComments;
        set => SetProperty(ref _blockComments, value);
    }

    public string NewCommentAuthor
    {
        get => _newCommentAuthor;
        set => SetProperty(ref _newCommentAuthor, value);
    }

    public string NewCommentText
    {
        get => _newCommentText;
        set => SetProperty(ref _newCommentText, value);
    }

    public string GalleryImagesText
    {
        get => _galleryImagesText;
        set
        {
            if (!SetProperty(ref _galleryImagesText, value)) return;
            if (SelectedElement?.Type == ElementType.Gallery)
            {
                SelectedElement.Properties.GalleryImages = value
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
                PersistSelectedElementProperties();
            }
        }
    }

    public string FaqItemsText
    {
        get => _faqItemsText;
        set
        {
            if (!SetProperty(ref _faqItemsText, value)) return;
            if (SelectedElement?.Type == ElementType.FAQ)
            {
                SelectedElement.Properties.FaqItems = ParseFaqItemsText(value);
                PersistSelectedElementProperties();
            }
        }
    }

    public string NavItemsText
    {
        get => _navItemsText;
        set
        {
            if (!SetProperty(ref _navItemsText, value)) return;
            if (SelectedElement?.Type == ElementType.Navbar)
            {
                SelectedElement.Properties.NavItems = ParseNavItemsText(value);
                PersistSelectedElementProperties();
            }
        }
    }

    public string[] CheckboxVariants { get; } = { "default", "switch", "rounded", "radio" };

    public RelayCommand AddNavItemCommand { get; }
    public RelayCommand AddCommentCommand { get; }
    public RelayCommand RefreshCommentsCommand { get; }

    public bool HasSelection => SelectedElement != null;
    public bool IsTextSelected => SelectedElement?.Type == ElementType.Text;
    public bool IsHeaderSelected => SelectedElement?.Type == ElementType.Header;
    public bool IsImageSelected => SelectedElement?.Type == ElementType.Image;
    public bool IsButtonSelected => SelectedElement?.Type == ElementType.Button;
    public bool IsCardSelected => SelectedElement?.Type == ElementType.Card;
    public bool IsInputSelected => SelectedElement?.Type == ElementType.Input;
    public bool IsContainerSelected => SelectedElement?.Type == ElementType.Container;
    public bool IsNavbarSelected => SelectedElement?.Type == ElementType.Navbar;
    public bool IsGallerySelected => SelectedElement?.Type == ElementType.Gallery;
    public bool IsLinkSelected => SelectedElement?.Type == ElementType.Link;
    public bool IsDividerSelected => SelectedElement?.Type == ElementType.Divider;
    public bool IsSpacerSelected => SelectedElement?.Type == ElementType.Spacer;
    public bool IsFormSelected => SelectedElement?.Type == ElementType.Form;
    public bool IsFooterSelected => SelectedElement?.Type == ElementType.Footer;
    public bool IsFaqSelected => SelectedElement?.Type == ElementType.FAQ;
    public bool IsCommentsSelected => SelectedElement?.Type == ElementType.CommentsBlock;
    public bool IsCommentsViewerSelected => SelectedElement?.Type == ElementType.CommentsViewer;
    public bool IsCommentElementSelected => SelectedElement?.Type is ElementType.CommentsBlock or ElementType.CommentsViewer;
    public bool IsVideoEmbedSelected => SelectedElement?.Type == ElementType.VideoEmbed;
    public bool IsVideoPlayerSelected => SelectedElement?.Type == ElementType.VideoPlayer;
    public bool IsCheckboxSelected => SelectedElement?.Type == ElementType.Checkbox;
    public bool IsTextAreaSelected => SelectedElement?.Type == ElementType.TextArea;
    public bool IsSliderSelected => SelectedElement?.Type == ElementType.Slider;

    public ObservableCollection<Page> PagesForAction
    {
        get => _pagesForAction;
        set => SetProperty(ref _pagesForAction, value);
    }

    public Array ActionTypes => Enum.GetValues(typeof(ActionType));

    public ActionType ButtonActionType
    {
        get => _buttonActionType;
        set => SetProperty(ref _buttonActionType, value);
    }

    public int? ButtonTargetPageId
    {
        get => _buttonTargetPageId;
        set => SetProperty(ref _buttonTargetPageId, value);
    }

    public int? ButtonTargetElementId
    {
        get => _buttonTargetElementId;
        set => SetProperty(ref _buttonTargetElementId, value);
    }

    public string ButtonTargetUrl
    {
        get => _buttonTargetUrl;
        set => SetProperty(ref _buttonTargetUrl, value);
    }

    public string ButtonCustomJs
    {
        get => _buttonCustomJs;
        set => SetProperty(ref _buttonCustomJs, value);
    }

    public bool CanEdit => _canEdit;
    public bool CanChangeSettings => _canChangeSettings;
    public bool CanManageTeam => _canManageTeam;
    public bool CanLaunch => WebsitePermissions.CanLaunchSite(_roleCode);

    public RelayCommand BackCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand LaunchSiteCommand { get; }
    public RelayCommand AddPageCommand { get; }
    public RelayCommand<PageTabViewModel> SelectPageCommand { get; }
    public RelayCommand<object?> SelectElementCommand { get; }
    public RelayCommand DeleteElementCommand { get; }
    public RelayCommand ApplyButtonActionCommand { get; }
    public RelayCommand OpenAccountCommand { get; }
    public RelayCommand ToggleSettingsCommand { get; }
    public RelayCommand ToggleOrganizationCommand { get; }
    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set => SetProperty(ref _isSettingsOpen, value);
    }

    public bool IsOrganizationOpen
    {
        get => _isOrganizationOpen;
        set => SetProperty(ref _isOrganizationOpen, value);
    }

    public double CanvasWidth
    {
        get => _canvasWidth;
        set => SetProperty(ref _canvasWidth, value);
    }

    public double CanvasHeight
    {
        get => _canvasHeight;
        set => SetProperty(ref _canvasHeight, value);
    }

    public string CanvasBackground
    {
        get => _canvasBackground;
        set => SetProperty(ref _canvasBackground, value);
    }

    public void AddElementFromToolbox(ElementType type, double canvasX, double canvasY)
    {
        if (!CanEdit || SelectedPage == null) return;
        var (parent, relX, relY) = ResolveContainerAt(canvasX, canvasY);
        var model = _elementService.AddElement(SelectedPage.Page.Id, type, relX, relY, parent?.Id);
        var vm = new CanvasElementViewModel(model);
        if (parent != null)
        {
            vm.Parent = parent;
            parent.Children.Add(vm);
        }
        else
            CanvasElements.Add(vm);
        RefreshElementRefLists();
        SelectElement(vm);
    }

    public (CanvasElementViewModel? parent, double relX, double relY) ResolveContainerAt(double canvasX, double canvasY)
    {
        CanvasElementViewModel? best = null;
        var bestArea = double.MaxValue;
        foreach (var container in EnumerateAll().Where(e => e.IsContainer))
        {
            if (!container.ContainsPoint(canvasX, canvasY)) continue;
            var w = container.Width ?? 120;
            var h = container.Height ?? 80;
            var area = w * h;
            if (area < bestArea)
            {
                bestArea = area;
                best = container;
            }
        }
        if (best == null) return (null, canvasX, canvasY);
        var b = best.GetAbsoluteBounds();
        return (best, canvasX - b.left, canvasY - b.top);
    }

    public void DetachFromContainer(CanvasElementViewModel element)
    {
        if (element.Parent == null) return;
        var b = element.Parent.GetAbsoluteBounds();
        element.Parent.Children.Remove(element);
        element.X = b.left + element.X;
        element.Y = b.top + element.Y;
        element.Parent = null;
        CanvasElements.Add(element);
    }

    public void AttachToContainer(CanvasElementViewModel element, CanvasElementViewModel container, double relX, double relY)
    {
        if (element.Parent == container) return;
        if (element.Parent != null)
            element.Parent.Children.Remove(element);
        else
            CanvasElements.Remove(element);

        element.Parent = container;
        element.X = Math.Max(0, relX);
        element.Y = Math.Max(0, relY);
        container.Children.Add(element);
    }

    public IEnumerable<CanvasElementViewModel> EnumerateAll()
    {
        foreach (var root in CanvasElements)
        {
            yield return root;
            foreach (var child in EnumerateChildren(root))
                yield return child;
        }
    }

    private static IEnumerable<CanvasElementViewModel> EnumerateChildren(CanvasElementViewModel parent)
    {
        foreach (var child in parent.Children)
        {
            yield return child;
            foreach (var nested in EnumerateChildren(child))
                yield return nested;
        }
    }

    public void PersistSelectedElementProperties()
    {
        if (SelectedElement == null) return;
        SaveElementToDb(SelectedElement);
    }

    public void PersistSelectedElement() => PersistSelectedElementProperties();

    public void SaveDraggedElement(CanvasElementViewModel element) => SaveElementToDb(element);

    private void SaveElementToDb(CanvasElementViewModel element)
    {
        element.SyncToModel();
        _elementService.SaveElement(element.Model);
    }

    private void LoadWebsite()
    {
        var website = _websiteService.GetById(_websiteId);
        if (website == null) return;
        WebsiteName = website.Name;
        UserRole = SessionContext.CurrentUserRole;

        var pages = _pageService.GetPages(_websiteId);
        PagesForAction = new ObservableCollection<Page>(pages);
        PageTabs.Clear();
        foreach (var p in pages)
            PageTabs.Add(new PageTabViewModel(p));

        if (PageTabs.Any())
            SelectPage(PageTabs[0]);

        ApplyCanvasFromSettings();
        Settings.ReloadPages();
    }

    private void OnSettingsSaved()
    {
        ApplyCanvasFromSettings();
        foreach (var item in Settings.PageItems)
        {
            var tab = PageTabs.FirstOrDefault(t => t.Page.Id == item.Page.Id);
            if (tab != null)
                tab.Name = item.Name;
        }
        IsSettingsOpen = false;
    }

    private void OnWebsiteDeleted()
    {
        IsSettingsOpen = false;
        if (SessionContext.CurrentWebsite?.Id == _websiteId)
            SessionContext.CurrentWebsite = null;
        _navigation.Navigate(new Views.Dashboard.DashboardView
        {
            DataContext = new DashboardViewModel(_navigation)
        });
    }

    private void ApplyCanvasFromSettings()
    {
        var settings = _websiteService.GetSettings(_websiteId);
        if (settings == null) return;

        CanvasWidth = settings.PageWidth > 0 ? settings.PageWidth : 960;
        CanvasHeight = settings.PageHeight > 0 ? settings.PageHeight : 640;
        CanvasBackground = settings.PageBackgroundColor;
        WebsiteName = settings.SiteTitle;
    }

    private void OnSelectedElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CanvasElementViewModel.Width) or nameof(CanvasElementViewModel.Height)
            or nameof(CanvasElementViewModel.X) or nameof(CanvasElementViewModel.Y))
            PersistSelectedElementProperties();
    }

    private void SelectPage(PageTabViewModel? tab)
    {
        if (tab == null) return;
        PersistAll();
        foreach (var t in PageTabs)
            t.IsSelected = t == tab;
        SelectedPage = tab;
        LoadCanvas();
    }

    private void PersistAll()
    {
        var all = EnumerateAll().ToList();
        foreach (var el in all)
            el.SyncToModel();
        if (all.Count > 0)
            _elementService.SaveAllElements(all.Select(c => c.Model));
    }

    private void LoadCanvas()
    {
        CanvasElements.Clear();
        SelectedElement = null;
        if (SelectedPage == null) return;
        var elements = _elementService.LoadAllPageElements(SelectedPage.Page.Id);
        var map = elements.ToDictionary(e => e.Id, e => new CanvasElementViewModel(e));
        foreach (var vm in map.Values)
        {
            if (vm.Model.ParentElementId is int pid && map.TryGetValue(pid, out var parent))
            {
                vm.Parent = parent;
                parent.Children.Add(vm);
            }
            else
                CanvasElements.Add(vm);
        }
        RefreshElementRefLists();
    }

    private void SelectElement(CanvasElementViewModel? el)
    {
        foreach (var item in CanvasElements)
            item.IsSelected = item == el;
        SelectedElement = el;
    }

    private void DeleteSelected()
    {
        if (!CanEdit || SelectedElement == null) return;
        var target = SelectedElement;
        if (target.IsContainer)
        {
            foreach (var child in target.Children.ToList())
            {
                DetachFromContainer(child);
                child.SyncToModel();
                _elementService.SaveElement(child.Model);
            }
        }
        if (target.Parent != null)
            target.Parent.Children.Remove(target);
        else
            CanvasElements.Remove(target);

        _elementService.DeleteElement(target.Id);
        SelectedElement = null;
        RefreshElementRefLists();
    }

    private void LoadButtonActionFromSelection()
    {
        if (SelectedElement?.Type is not (ElementType.Button or ElementType.Card))
            return;
        var action = SelectedElement.Model.Action;
        if (action == null)
        {
            ButtonActionType = ActionType.None;
            return;
        }
        ButtonActionType = action.ActionType;
        ButtonTargetPageId = action.TargetPageId;
        ButtonTargetElementId = action.TargetElementId;
        ButtonTargetUrl = action.TargetUrl ?? string.Empty;
        ButtonCustomJs = action.CustomJavaScript ?? string.Empty;
    }

    private void ApplyButtonAction()
    {
        if (SelectedElement?.Type is not (ElementType.Button or ElementType.Card)) return;
        var action = new ElementAction
        {
            Id = SelectedElement.Model.ActionId ?? 0,
            WebsiteId = _websiteId,
            Name = $"Action for {SelectedElement.Type} #{SelectedElement.Id}",
            ActionType = ButtonActionType,
            TargetPageId = ButtonActionType == ActionType.NavigateToPage ? ButtonTargetPageId : null,
            TargetElementId = ButtonActionType == ActionType.SubmitForm ? ButtonTargetElementId : null,
            TargetUrl = ButtonActionType == ActionType.OpenUrl ? ButtonTargetUrl : null,
            CustomJavaScript = ButtonActionType == ActionType.CustomScript ? ButtonCustomJs : null
        };
        action = _elementService.CreateOrUpdateAction(_websiteId, action);
        SelectedElement.Model.ActionId = action.Id;
        SelectedElement.Model.Action = action;
        SelectedElement.SyncToModel();
        _elementService.SaveElement(SelectedElement.Model);
        StatusMessage = SelectedElement.Type == ElementType.Card
            ? "Действие карточки сохранено."
            : "Действие кнопки сохранено.";
    }

    private void SaveProject()
    {
        if (!CanEdit) return;
        PersistAll();
        StatusMessage = "Проект сохранён в базе данных.";
        MessageBox.Show("Все изменения сохранены в базе данных.", "Сохранение",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LaunchSite()
    {
        try
        {
            if (!CanEdit)
            {
                var website = _websiteService.GetById(_websiteId);
                var folder = website?.LastExportPath;
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                {
                    MessageBox.Show("Сайт ещё не публиковался. Попросите редактора сохранить и запустить проект.", "Запуск",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                OpenExportedSite(folder);
                return;
            }

            PersistAll();
            var exportFolder = App.Export.ExportWebsite(_websiteId);
            _websiteService.UpdateExportPath(_websiteId, exportFolder);
            OpenExportedSite(exportFolder);
            StatusMessage = "Сайт экспортирован из БД и открыт в браузере.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            MessageBox.Show(ex.Message, "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void OpenExportedSite(string folder)
    {
        var index = Path.Combine(folder, "index.html");
        if (!File.Exists(index))
        {
            MessageBox.Show("Файл index.html не найден.", "Запуск",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = index,
            UseShellExecute = true
        });
    }

    private void AddPage()
    {
        var page = _pageService.CreatePage(_websiteId, $"Страница {PageTabs.Count + 1}");
        var tab = new PageTabViewModel(page);
        PageTabs.Add(tab);
        PagesForAction.Add(page);
        Settings.ReloadPages();
        SelectPage(tab);
    }

    private void GoBack()
    {
        if (CanEdit)
            PersistAll();
        _navigation.Navigate(new Views.Dashboard.DashboardView
        {
            DataContext = new DashboardViewModel(_navigation)
        });
    }

    private void RefreshElementRefLists()
    {
        FormsOnPage = new ObservableCollection<ElementRefOption>(
            CanvasElements.Where(e => e.Type == ElementType.Form)
                .Select(e => new ElementRefOption(e.Id, $"Форма #{e.Id}")));
        ButtonsOnPage = new ObservableCollection<ElementRefOption>(
            CanvasElements.Where(e => e.Type == ElementType.Button)
                .Select(e => new ElementRefOption(e.Id, $"Кнопка #{e.Id} ({e.Properties.Text})")));
        CommentsBlocksOnPage = new ObservableCollection<ElementRefOption>(
            CanvasElements.Where(e => e.Type == ElementType.CommentsBlock)
                .Select(e => new ElementRefOption(e.Id, $"Блок комментариев #{e.Id}")));
    }

    private void LoadGalleryImagesText()
    {
        if (SelectedElement?.Type != ElementType.Gallery) return;
        GalleryImagesText = string.Join(Environment.NewLine,
            SelectedElement.Properties.GalleryImages ?? new List<string>());
    }

    private void LoadFaqItemsText()
    {
        if (SelectedElement?.Type != ElementType.FAQ) return;
        FaqItemsText = FormatFaqItemsText(SelectedElement.Properties.FaqItems);
    }

    private static string FormatFaqItemsText(List<FaqItemDto>? items)
    {
        if (items == null || items.Count == 0) return string.Empty;
        return string.Join(Environment.NewLine, items.Select(i => $"{i.Question}|{i.Answer}"));
    }

    private static List<FaqItemDto> ParseFaqItemsText(string text)
    {
        var list = new List<FaqItemDto>();
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('|', 2);
            list.Add(new FaqItemDto
            {
                Question = parts[0].Trim(),
                Answer = parts.Length > 1 ? parts[1].Trim() : ""
            });
        }
        return list;
    }

    private void LoadNavItemsText()
    {
        if (SelectedElement?.Type != ElementType.Navbar) return;
        NavItemsText = FormatNavItemsText(SelectedElement.Properties.NavItems);
    }

    private static string FormatNavItemsText(List<NavItemDto>? items)
    {
        if (items == null || items.Count == 0) return string.Empty;
        return string.Join(Environment.NewLine, items.Select(i =>
        {
            if (i.TargetPageId.HasValue) return $"{i.Text}|page:{i.TargetPageId}";
            return $"{i.Text}|{i.Url ?? "#"}";
        }));
    }

    private static List<NavItemDto> ParseNavItemsText(string text)
    {
        var list = new List<NavItemDto>();
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('|', 2);
            var item = new NavItemDto { Text = parts[0].Trim() };
            if (parts.Length > 1)
            {
                var target = parts[1].Trim();
                if (target.StartsWith("page:", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(target.AsSpan(5), out var pageId))
                    item.TargetPageId = pageId;
                else
                    item.Url = target;
            }
            list.Add(item);
        }
        return list;
    }

    private void AddNavItem()
    {
        if (SelectedElement?.Type != ElementType.Navbar) return;
        SelectedElement.Properties.NavItems ??= new List<NavItemDto>();
        SelectedElement.Properties.NavItems.Add(new NavItemDto { Text = "Новый пункт", Url = "#" });
        NavItemsText = FormatNavItemsText(SelectedElement.Properties.NavItems);
        PersistSelectedElementProperties();
    }

    private void LoadBlockComments()
    {
        BlockComments.Clear();
        if (SelectedElement?.Type is not (ElementType.CommentsBlock or ElementType.CommentsViewer) || SelectedPage == null) return;
        var sourceId = ResolveSelectedCommentsSourceId();
        foreach (var c in App.Comments.GetForBlock(SelectedPage.Page.Id, sourceId))
            BlockComments.Add(c);
    }

    private int ResolveSelectedCommentsSourceId()
    {
        if (SelectedElement == null || SelectedPage == null)
            return SelectedElement?.Id ?? 0;
        var pageElements = CanvasElements.Select(e => e.Model).ToList();
        return CommentsHelper.ResolveSourceElementId(SelectedElement.Model, SelectedElement.Properties, pageElements);
    }

    private void AddComment()
    {
        if (SelectedElement?.Type is not (ElementType.CommentsBlock or ElementType.CommentsViewer) || SelectedPage == null) return;
        if (string.IsNullOrWhiteSpace(NewCommentText)) return;
        var sourceId = ResolveSelectedCommentsSourceId();
        var c = App.Comments.Add(SelectedPage.Page.Id, sourceId, NewCommentAuthor, NewCommentText);
        BlockComments.Add(c);
        NewCommentText = string.Empty;
        StatusMessage = "Комментарий добавлен.";
    }
}

public record ElementRefOption(int Id, string Label);
