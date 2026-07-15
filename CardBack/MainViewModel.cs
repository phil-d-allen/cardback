namespace CardBack;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

#pragma warning disable CA1515 // Consider making public types internal
public sealed class MainViewModel : INotifyPropertyChanged
#pragma warning restore CA1515 // Consider making public types internal
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        MainColor = Colors.Aquamarine;
        AuxiliaryColor = Colors.Black;
        Patterns.Add(new PatternChoice()
        {
            Name = "Crosshatched",
            Algorithm = DrawCrosshatched,
        });

        Patterns.Add(new PatternChoice()
        {
            Name = "Grid",
            Algorithm = DrawGrid,
        });

        Patterns.Add(new PatternChoice()
        {
            Name = "Diamonds",
            Algorithm = DrawDiamond,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Aquamarine",
            Color = Colors.Aquamarine,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Antique White",
            Color = Colors.AntiqueWhite,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Black",
            Color = Colors.Black,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Blue",
            Color = Colors.Blue,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Green",
            Color = Colors.Green,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Purple",
            Color = Colors.Purple,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Red",
            Color = Colors.Red,
        });

        ColorChoices.Add(new ColorChoice()
        {
            Name = "Yellow",
            Color = Colors.Yellow,
        });

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

#pragma warning disable CA1002 // Do not expose generic lists
    public List<PatternChoice> Patterns
#pragma warning restore CA1002 // Do not expose generic lists
    {
        get => field;
    } = new List<PatternChoice>();

    public Action<MainViewModel, int, GeometryGroup>? SelectedAlgorithm
    {
        get => field;
        set => SetProperty(ref field, value);
    }

#pragma warning disable CA1002 // Do not expose generic lists
    public List<ColorChoice> ColorChoices
#pragma warning restore CA1002 // Do not expose generic lists
    {
        get => field;
    } = new List<ColorChoice>();

    public Color MainColor
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public Color AuxiliaryColor
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    private bool ArePropertiesConsistent
    {
        get
        {
            return !(Width is 0 || Height is 0 || CornerRounding > Width || CornerRounding > Height || SelectedAlgorithm is null);
        }
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
        if (!ArePropertiesConsistent)
        {
            return;
        }

        const int patternHeight = 8;

        GeometryDrawing lines = new GeometryDrawing();
        lines.Pen = new Pen(new SolidColorBrush(MainColor), (patternHeight - 2) / 2);

        GeometryGroup lineGroup = new GeometryGroup();

        SelectedAlgorithm(this, patternHeight, lineGroup);

        lines.Geometry = lineGroup;

        GeometryDrawing background = new GeometryDrawing();
        GeometryGroup backgroundGroup = new GeometryGroup();
        backgroundGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, Width, Height)));
        background.Brush = new SolidColorBrush(AuxiliaryColor);
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

        CurrentImage = image;
    }

    private static void DrawCrosshatched(MainViewModel self, int diamondThickness, GeometryGroup lineGroup)
    {
        int ContainXInFigure(int x)
        {
            return Math.Min(self.Width, Math.Max(0, x));
        }

        int ContainYInFigure(int y)
        {
            return Math.Min(self.Height, Math.Max(0, y));
        }

        // Lines from NW to SE, starting in NW and doing glide transformations to the E.
        for (int idX = 0; idX < self.Width; idX += diamondThickness)
        {
            int bottomX = ContainXInFigure(self.Height + idX);
            int bottomY = ContainYInFigure(self.Width - idX);

            lineGroup.Children.Add(new LineGeometry(new Point(idX, 0), new Point(bottomX, bottomY)));
        }

        // Lines from NW to SE, starting just south of NW and doing glide transformations to the S
        for (int idY = diamondThickness; idY < self.Height; idY += diamondThickness)
        {
            int bottomX = ContainXInFigure(self.Height - idY);
            int bottomY = ContainYInFigure(self.Width + idY);

            lineGroup.Children.Add(new LineGeometry(new Point(0, idY), new Point(bottomX, bottomY)));
        }

        // Lines from NE to SW, starting in NW and doing glide transformations to the E.
        for (int idX = self.Width; idX >= 0; idX -= diamondThickness)
        {
            int bottomX = ContainXInFigure(idX - self.Height);
            int bottomY = ContainYInFigure(idX);

            lineGroup.Children.Add(new LineGeometry(new Point(idX, 0), new Point(bottomX, bottomY)));
        }

        // Lines from NE to SW, starting just south of NE and doing glide transformations to the S
        for (int idY = diamondThickness; idY < self.Height; idY += diamondThickness)
        {
            int bottomX = ContainXInFigure(self.Width - self.Height + idY);
            int bottomY = ContainYInFigure(self.Width + idY);

            lineGroup.Children.Add(new LineGeometry(new Point(self.Width, idY), new Point(bottomX, bottomY)));
        }
    }

    private static void DrawGrid(MainViewModel self, int patternHeight, GeometryGroup lineGroup)
    {
        for (int idX = patternHeight / 2; idX < self.Width; idX += patternHeight)
        {
            lineGroup.Children.Add(new LineGeometry(new Point(idX, 0), new Point(idX, self.Height)));
        }

        for (int idY = patternHeight / 2; idY < self.Height; idY += patternHeight)
        {
            lineGroup.Children.Add(new LineGeometry(new Point(0, idY), new Point(self.Width, idY)));
        }
    }

    private static void DrawDiamond(MainViewModel self, int patternHeight, GeometryGroup lineGroup)
    {
        // There are four quadrants of the card; for diamond style, each quadrant has lines running
        // perpendicular to the identity of the quadrant... e.g., the NW quadrant is lines running from
        // the SW to the NE.  These lines are all parallel within a quadrant, and unlike croasshatched
        // the slope of the line is the aspect ratio of the cardback.
        // We use the shorter side of the aspect ratio to determine how many total lines there are.
        // For consistency, we will draw lines starting at the center of the figure and leading
        // towards the corners.

        // NW quadrant: Lines from SW to NE, etc.
        int shorterSideLength = Math.Min(self.Height, self.Width);
        int numberOfLinesOnAxis = shorterSideLength / patternHeight / 2;
        int numberOfLinesTotal = numberOfLinesOnAxis * 2;

        // NW quadrant is from 0...midX and 0...midY
        //             / topRight
        //            /
        //   0,0_____/___midX__________
        //      |   /     |
        //      |  /      |
        //      | /       |
        //      |/        |
        //     /|         |
        //    / |         |
        // bl/ midY-------+---
        static (Point adjustedBottomLeft, Point adjustedTopRight) BoundPointsInNWQuadrant(Point bottomLeft, Point topRight)
        {
            double dx = topRight.X;
            double dy = bottomLeft.Y;
            double slope = dy / dx;
            return (AdjustBottomLeft(bottomLeft, slope),
                    AdjustTopRight(topRight, slope));

            static Point AdjustTopRight(Point topRight, double slope)
            {
                if (topRight.Y < 0)
                {
                    // we know topRight.Y is negative; use that.
                    return new Point(topRight.X + (topRight.Y / slope), 0);
                }
                else
                {
                    return topRight;
                }
            }

            static Point AdjustBottomLeft(Point bottomLeft, double slope)
            {
                if (bottomLeft.X < 0)
                {
                    // we know bottomLeft.X is negative; use that.
                    return new Point(0, bottomLeft.Y + (bottomLeft.X * slope));
                }
                else
                {
                    return bottomLeft;
                }
            }
        }

        int midX = self.Width / 2;
        int midY = self.Height / 2;

        for (int idx = 0; idx < numberOfLinesTotal; ++idx)
        {
            int centerDX = (midX / numberOfLinesOnAxis) * idx + (patternHeight / 2);
            int centerDY = (midY / numberOfLinesOnAxis) * idx + (patternHeight / 2);

            (Point nwBottomLeft, Point nwTopRight) = BoundPointsInNWQuadrant(
                new Point(midX - centerDX, midY),
                new Point(midX, midY - centerDY));


            // in the NW quadrant
            lineGroup.Children.Add(new LineGeometry(
                nwBottomLeft,
                nwTopRight));

            // Flip the nw to ne; reflect nwBottomLeft on the X-axis by
            // subtracting midX, multiply by negative 1, and then adding midX.
            // reflect nwTopRight the same way.

            // NE
            lineGroup.Children.Add(new LineGeometry(
                new Point(((nwBottomLeft.X - midX) * -1) + midX, nwBottomLeft.Y),
                new Point(((nwTopRight.X - midX) * -1) + midX, nwTopRight.Y)));

            // Similar to nw=>ne flip, instead keep X coordinates the same and reflect Y coordinates
            // SW
            lineGroup.Children.Add(new LineGeometry(
                new Point(nwBottomLeft.X, ((nwBottomLeft.Y - midY) * -1) + midY),
                new Point(nwTopRight.X, ((nwTopRight.Y - midY) * -1) + midY)));

            // For NW=>SE flip, reflect both X and Y axis
            // SE
            lineGroup.Children.Add(new LineGeometry(
                new Point(((nwBottomLeft.X - midX) * -1) + midX, ((nwBottomLeft.Y - midY) * -1) + midY),
                new Point(((nwTopRight.X - midX) * -1) + midX, ((nwTopRight.Y - midY) * -1) + midY)));
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

#pragma warning disable CA1515 // Consider making public types internal
public sealed class PatternChoice
#pragma warning restore CA1515 // Consider making public types internal
{
    public required string Name { get; init; }

    public required Action<MainViewModel, int, GeometryGroup> Algorithm { get; init; }

}

#pragma warning disable CA1515 // Consider making public types internal
public sealed class ColorChoice
#pragma warning restore CA1515 // Consider making public types internal
{
    public required string Name { get; init; }

    public required Color Color { get; init; }

}