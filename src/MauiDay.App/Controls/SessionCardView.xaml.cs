using System.Windows.Input;
using MauiDay.App.ViewModels;

namespace MauiDay.App.Controls;

public partial class SessionCardView : ContentView
{
    public static readonly BindableProperty SessionProperty = BindableProperty.Create(
        nameof(Session),
        typeof(SessionCardModel),
        typeof(SessionCardView));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(SessionCardView));

    public SessionCardView()
    {
        InitializeComponent();
    }

    public SessionCardModel? Session
    {
        get => (SessionCardModel?)GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
