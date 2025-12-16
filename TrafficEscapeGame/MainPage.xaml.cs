namespace TrafficEscapeGame;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void StartGame_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GamePage");
    }

    private async void Settings_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }

    private void Exit_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
    }
}