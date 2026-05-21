using System.Collections.ObjectModel;

using CMSBuilder.Helpers;

using CMSBuilder.Models;

using CMSBuilder.Models.Dto;

using CMSBuilder.Models.Enums;

using CMSBuilder.ViewModels.Base;



namespace CMSBuilder.ViewModels.Editor;



public class CanvasElementViewModel : BaseViewModel

{

    private bool _isSelected;

    private double _x;

    private double _y;

    private double? _width;

    private double? _height;

    private CanvasElementViewModel? _parent;



    public CanvasElementViewModel(PageElement model)

    {

        Model = model;

        _x = model.X;

        _y = model.Y;

        _width = model.Width;

        _height = model.Height;

        Properties = ElementJsonHelper.DeserializeProperties(model.PropertiesJson);

        Style = ElementJsonHelper.DeserializeStyle(model.StyleJson);

        Properties.PropertyChanged += (_, _) => OnElementVisualChanged();

        Style.PropertyChanged += (_, _) => OnElementVisualChanged();

        Children = new ObservableCollection<CanvasElementViewModel>();

    }



    public PageElement Model { get; }

    public ElementType Type => Model.Type;

    public int Id => Model.Id;

    public ObservableCollection<CanvasElementViewModel> Children { get; }



    public CanvasElementViewModel? Parent

    {

        get => _parent;

        set

        {

            if (SetProperty(ref _parent, value))

            {

                Model.ParentElementId = value?.Id;

                OnPropertyChanged(nameof(IsInContainer));

            }

        }

    }



    public bool IsContainer => Type is ElementType.Container or ElementType.Form;

    public bool IsInContainer => Parent != null;



    public bool IsSelected

    {

        get => _isSelected;

        set => SetProperty(ref _isSelected, value);

    }



    public double X

    {

        get => _x;

        set { if (SetProperty(ref _x, value)) Model.X = value; }

    }



    public double Y

    {

        get => _y;

        set { if (SetProperty(ref _y, value)) Model.Y = value; }

    }



    public double? Width

    {

        get => _width;

        set

        {

            if (SetProperty(ref _width, value))

            {

                Model.Width = value;

                OnPropertyChanged(nameof(HasExplicitSize));

            }

        }

    }



    public double? Height

    {

        get => _height;

        set

        {

            if (SetProperty(ref _height, value))

            {

                Model.Height = value;

                OnPropertyChanged(nameof(HasExplicitSize));

            }

        }

    }



    public bool HasExplicitSize => Width is > 0 || Height is > 0;



    public ElementPropertiesDto Properties { get; }

    public ElementStyleDto Style { get; }



    public string DisplayText => Properties.Text ?? Type.ToString();



    public void SyncToModel()

    {

        Model.PropertiesJson = ElementJsonHelper.SerializeProperties(Properties);

        Model.StyleJson = ElementJsonHelper.SerializeStyle(Style);

        Model.X = X;

        Model.Y = Y;

        Model.Width = Width;

        Model.Height = Height;

        Model.ParentElementId = Parent?.Id;

        foreach (var child in Children)

            child.SyncToModel();

    }



    private void OnElementVisualChanged()

    {

        SyncToModel();

        OnPropertyChanged(nameof(DisplayText));

    }



    public (double left, double top, double right, double bottom) GetAbsoluteBounds()

    {

        var left = X;

        var top = Y;

        if (Parent != null)

        {

            var p = Parent.GetAbsoluteBounds();

            left += p.left;

            top += p.top;

        }

        var w = Width ?? 120;

        var h = Height ?? 80;

        return (left, top, left + w, top + h);

    }



    public bool ContainsPoint(double canvasX, double canvasY)

    {

        var b = GetAbsoluteBounds();

        return canvasX >= b.left && canvasX <= b.right && canvasY >= b.top && canvasY <= b.bottom;

    }

}


