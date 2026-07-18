using System.Windows.Input;
using MauiDay.App.ViewModels;

namespace MauiDay.App.Controls;

public partial class SpeakerCardView : ContentView
{
    public static readonly BindableProperty SpeakerProperty = BindableProperty.Create(
        nameof(Speaker),
        typeof(SpeakerCardModel),
        typeof(SpeakerCardView));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(SpeakerCardView));

    public SpeakerCardView()
    {
        InitializeComponent();
    }

    public SpeakerCardModel? Speaker
    {
        get => (SpeakerCardModel?)GetValue(SpeakerProperty);
        set => SetValue(SpeakerProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
