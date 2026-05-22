namespace CMSBuilder.Models.Dto;

public class ElementPropertiesDto : NotifyDto
{
    private string? _text;
    private string? _description;
    private string? _src;
    private string? _alt;
    private string? _placeholder;
    private string? _inputType;
    private bool _required;
    private string? _href;
    private string? _logoText;
    private string? _logoSrc;
    private string? _level;
    private bool _isScalable;
    private string? _fieldName;
    private int? _formElementId;
    private int? _linkedButtonId;
    private string? _checkboxVariant;
    private string? _formMethod;
    private string? _formActionUrl;
    private string? _flexDirection;
    private string? _gap;
    private bool _allowComments;
    private int? _maxComments;
    private string? _videoUrl;
    private string? _videoPosterUrl;
    private bool _videoAutoplay;
    private bool _videoControls = true;
    private bool _videoLoop;
    private bool _showCommentAuthor = true;
    private bool _showCommentDate = true;

    public string? Text { get => _text; set => SetField(ref _text, value); }
    public string? Description { get => _description; set => SetField(ref _description, value); }
    public string? Src { get => _src; set => SetField(ref _src, value); }
    public string? Alt { get => _alt; set => SetField(ref _alt, value); }
    public string? Placeholder { get => _placeholder; set => SetField(ref _placeholder, value); }
    public string? InputType { get => _inputType; set => SetField(ref _inputType, value); }
    public bool Required { get => _required; set => SetField(ref _required, value); }
    public string? Href { get => _href; set => SetField(ref _href, value); }
    public string? LogoText { get => _logoText; set => SetField(ref _logoText, value); }
    public string? LogoSrc { get => _logoSrc; set => SetField(ref _logoSrc, value); }
    public string? Level { get => _level; set => SetField(ref _level, value); }
    public bool IsScalable { get => _isScalable; set => SetField(ref _isScalable, value); }

    /// <summary>Имя поля (name) для input/checkbox в форме.</summary>
    public string? FieldName { get => _fieldName; set => SetField(ref _fieldName, value); }
    /// <summary>Id элемента Form на странице.</summary>
    public int? FormElementId { get => _formElementId; set => SetField(ref _formElementId, value); }
    /// <summary>Кнопка, связанная с полем ввода.</summary>
    public int? LinkedButtonId { get => _linkedButtonId; set => SetField(ref _linkedButtonId, value); }
    /// <summary>default | switch | rounded</summary>
    public string? CheckboxVariant { get => _checkboxVariant; set => SetField(ref _checkboxVariant, value); }
    public string? FormMethod { get => _formMethod; set => SetField(ref _formMethod, value); }
    public string? FormActionUrl { get => _formActionUrl; set => SetField(ref _formActionUrl, value); }
    /// <summary>row | column для контейнера.</summary>
    public string? FlexDirection { get => _flexDirection; set => SetField(ref _flexDirection, value); }
    public string? Gap { get => _gap; set => SetField(ref _gap, value); }
    public bool AllowComments { get => _allowComments; set => SetField(ref _allowComments, value); }
    public int? MaxComments { get => _maxComments; set => SetField(ref _maxComments, value); }

    /// <summary>URL видео (вставка iframe или HTML5-плеер).</summary>
    public string? VideoUrl { get => _videoUrl; set => SetField(ref _videoUrl, value); }
    public string? VideoPosterUrl { get => _videoPosterUrl; set => SetField(ref _videoPosterUrl, value); }
    public bool VideoAutoplay { get => _videoAutoplay; set => SetField(ref _videoAutoplay, value); }
    public bool VideoControls { get => _videoControls; set => SetField(ref _videoControls, value); }
    public bool VideoLoop { get => _videoLoop; set => SetField(ref _videoLoop, value); }
    public bool ShowCommentAuthor { get => _showCommentAuthor; set => SetField(ref _showCommentAuthor, value); }
    public bool ShowCommentDate { get => _showCommentDate; set => SetField(ref _showCommentDate, value); }

    public List<NavItemDto>? NavItems { get; set; }
    public List<string>? GalleryImages { get; set; }
    public List<FaqItemDto>? FaqItems { get; set; }
}

public class NavItemDto
{
    public string Text { get; set; } = string.Empty;
    public int? TargetPageId { get; set; }
    public string? Url { get; set; }
}

public class FaqItemDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
