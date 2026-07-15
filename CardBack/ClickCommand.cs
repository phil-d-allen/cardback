using System.Windows.Input;

namespace CardBack;

internal sealed class ClickCommand : ICommand
{
    public required Action<object?> OnClick
    {
        get;init;
    }
    
    public Predicate<object?>? CanClick
    {
        get;init;
    }

    public ClickCommand()
    {    
    }

    public static ClickCommand From(Action<object?> onClick, Predicate<object?> canClick)
    {
        return new ClickCommand()
        {
            OnClick = onClick,
            CanClick = canClick,
        };
    }

    public static ClickCommand From(Action<object?> onClick)
    {
        return new ClickCommand()
        {
            OnClick = onClick
        };
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (CanClick is null)
        {
            return true;
        }
        return CanClick(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object? parameter)
    {
        OnClick(parameter);
    }
}