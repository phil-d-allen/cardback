namespace CardBack;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

internal sealed class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        PropertyChanged += SelfPropertyChanged;
    }

    public void SelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateImage();
    }

    /// <summary>
    /// GenerateImage does not check and see the current properties to know if an image already exists with these properties;
    /// as used by the view model, it is expecting to be called when any of the properties change.
    /// </summary>
    public void GenerateImage()
    {
        if (Width is 0 || Height is 0 || CornerRounding > Width || CornerRounding > Height)
        {
            return;
        }

        // NEXT: Replace with drawing algorithm, which might require adding new controls/view model properties
        CurrentImage = null;
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

    public void SetProperty<T>(ref T toSet, T newValue, IEqualityComparer<T>? comparer = null, [CallerMemberName] string memberName = "")
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