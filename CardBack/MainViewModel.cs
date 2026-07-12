namespace CardBack;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

internal sealed class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        PropertyChanged += SelfPropertyChanged;
    }

    public Drawing? CurrentImage
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public string? Name
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public int Height
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public int Width
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public int CornerRounding
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    private void SelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(CurrentImage))
        {
            GenerateImage();
        }
    }

    /// <summary>
    /// GenerateImage does not check and see the current properties to know if an image already exists with these properties;
    /// as used by the view model, it is expecting to be called when any of the properties change.
    /// </summary>
    private void GenerateImage()
    {
        if (Width is 0 || Height is 0 || CornerRounding > Width || CornerRounding > Height)
        {
            return;
        }

        const int diamondThickness = 8;

        GeometryDrawing lines = new GeometryDrawing();
        lines.Pen = new Pen(Brushes.Aquamarine, (diamondThickness - 2) / 2);

        GeometryGroup lineGroup = new GeometryGroup();

        DrawCrosshatched(diamondThickness, lineGroup);

        lines.Geometry = lineGroup;

        GeometryDrawing background = new GeometryDrawing();
        GeometryGroup backgroundGroup = new GeometryGroup();
        backgroundGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, Width, Height)));
        background.Brush = Brushes.Black;
        background.Geometry = backgroundGroup;

        DrawingGroup image = new DrawingGroup();
        image.Children.Add(background);
        image.Children.Add(lines);

        if (CornerRounding > 0)
        {
            // create a shape that is a quarter circle with no pen and a "purple" background.
            // Now create three more
            // place in the corners
            GeometryDrawing corners = new GeometryDrawing();
            GeometryGroup cornersGroup = new GeometryGroup();

            // Start at 'bottom left' of corner of the first call to DrawOneQuarter,
            // which is the left center of the total figure.
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = new Point(0, Height / 2),
                IsFilled = true,
            };

            DrawOneQuarter(pathFigure, new Point(0, CornerRounding), new Point(CornerRounding, 0), new Point(Width / 2, 0));
            DrawOneQuarter(pathFigure, new Point(Width - CornerRounding, 0), new Point(Width, CornerRounding), new Point(Width, Height / 2));
            DrawOneQuarter(pathFigure, new Point(Width, Height - CornerRounding), new Point(Width - CornerRounding, Height), new Point(Width / 2, Height));
            DrawOneQuarter(pathFigure, new Point(CornerRounding, Height), new Point(0, Height - CornerRounding), new Point(0, Height / 2));

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.FillRule = FillRule.Nonzero;
            pathGeometry.Figures.Add(pathFigure);
            cornersGroup.Children.Add(pathGeometry);

            corners.Geometry = cornersGroup;
            image.ClipGeometry = cornersGroup;
        }

        // NEXT: Replace with multiple drawing algorithm, which might require adding new controls/view model properties
        CurrentImage = image;
    }

    private void DrawCrosshatched(int diamondThickness, GeometryGroup lineGroup)
    {
        // Lines from NW to SE, starting in NW and doing glide transformations to the E.
        for (int idX = 0; idX < Width; idX += diamondThickness)
        {
            int bottomX = int.Min(Width, Height + idX);

            int bottomY = int.Min(Height, Width - idX);
            lineGroup.Children.Add(new LineGeometry(new Point(idX, 0), new Point(bottomX, bottomY)));
        }

        // Lines from NW to SE, starting just south of NW and doing glide transformations to the S
        for (int idY = diamondThickness; idY < Height; idY += diamondThickness)
        {
            int bottomX = int.Min(Width, Height - idY);

            int bottomY = int.Min(Height, Width + idY);
            lineGroup.Children.Add(new LineGeometry(new Point(0, idY), new Point(bottomX, bottomY)));
        }

        // Lines from NE to SW, starting in NW and doing glide transformations to the E.
        for (int idX = Width; idX >= 0; idX -= diamondThickness)
        {
            int bottomX = int.Max(0, idX - Height);

            int bottomY = int.Min(Height, idX);
            lineGroup.Children.Add(new LineGeometry(new Point(idX, 0), new Point(bottomX, bottomY)));
        }

        // Lines from NE to SW, starting just south of NE and doing glide transformations to the S
        for (int idY = diamondThickness; idY < Height; idY += diamondThickness)
        {
            int bottomX = int.Max(0, (Width - Height) + idY);

            int bottomY = int.Min(Height, Width + idY);
            lineGroup.Children.Add(new LineGeometry(new Point(Width, idY), new Point(bottomX, bottomY)));
        }
    }

    // Direction names are correct for the top left corner, everything else is just a rotation of it.
    //         N     NE
    //         -------- 
    //        /
    //       / Arc
    //    W |
    //      |
    //   SW |
    // Precondition: pathFigure is already set for "sw".
    private void DrawOneQuarter(PathFigure pathFigure, Point w, Point n, Point ne)
    {

        // Line up from SW to W
        pathFigure.Segments.Add(new LineSegment(w, isStroked: true));

        // Create the ArcSegment
        ArcSegment arcSegment = new ArcSegment
        {
            Point = n, // End at N-center
            Size = new Size(CornerRounding, CornerRounding),
            IsLargeArc = false, // will be 90               
            SweepDirection = SweepDirection.Clockwise
        };

        // Do the arc from W to N
        pathFigure.Segments.Add(arcSegment);
        //N to NE, which is SW for the next quadrant
        pathFigure.Segments.Add(new LineSegment(ne, isStroked: true));
    }

    private void SetProperty<T>(ref T toSet, T newValue, IEqualityComparer<T>? comparer = null, [CallerMemberName] string memberName = "")
    {
        if (ReferenceEquals(toSet, newValue))
        {
            return;
        }

        if (comparer is null)
        {
            comparer = EqualityComparer<T>.Default;
        }

        if (comparer.Equals(toSet, newValue))
        {
            return;
        }

        toSet = newValue;
        // A more advanced implementation might want to send this to the UI thread.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
}