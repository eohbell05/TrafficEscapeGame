using Microsoft.Maui.Controls;

namespace TrafficEscapeGame;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Load saved difficulty (default is Easy)
        string difficulty = Preferences.Get("Difficulty", "Easy");
        UpdateDifficultyButtons(difficulty);

        // Load saved car color (default is Blue)
        string carColor = Preferences.Get("CarColor", "Blue");
        UpdateCarColorButtons(carColor);
    }

    // ===========================
    // DIFFICULTY BUTTONS
    // ===========================
    private void EasyButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("Difficulty", "Easy");
        UpdateDifficultyButtons("Easy");
    }

    private void MediumButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("Difficulty", "Medium");
        UpdateDifficultyButtons("Medium");
    }

    private void HardButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("Difficulty", "Hard");
        UpdateDifficultyButtons("Hard");
    }

    private void UpdateDifficultyButtons(string difficulty)
    {
        // Reset all buttons to gray
        EasyButton.BackgroundColor = Color.FromArgb("#666");
        MediumButton.BackgroundColor = Color.FromArgb("#666");
        HardButton.BackgroundColor = Color.FromArgb("#666");

        // Highlight selected button
        if (difficulty == "Easy")
        {
            EasyButton.BackgroundColor = Color.FromArgb("#4CAF50");
            DifficultyDescription.Text = "Enemy speed: Slow";
        }
        else if (difficulty == "Medium")
        {
            MediumButton.BackgroundColor = Color.FromArgb("#FF9800");
            DifficultyDescription.Text = "Enemy speed: Normal";
        }
        else if (difficulty == "Hard")
        {
            HardButton.BackgroundColor = Color.FromArgb("#F44336");
            DifficultyDescription.Text = "Enemy speed: Fast";
        }
    }

    // ===========================
    // CAR COLOR BUTTONS
    // ===========================
    private void BlueCarButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("CarColor", "Blue");
        UpdateCarColorButtons("Blue");
    }

    private void RedCarButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("CarColor", "Red");
        UpdateCarColorButtons("Red");
    }

    private void GreenCarButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("CarColor", "Green");
        UpdateCarColorButtons("Green");
    }

    private void UpdateCarColorButtons(string color)
    {
        // Reset all buttons to gray
        BlueCarButton.BackgroundColor = Color.FromArgb("#666");
        RedCarButton.BackgroundColor = Color.FromArgb("#666");
        GreenCarButton.BackgroundColor = Color.FromArgb("#666");

        // Highlight selected button
        if (color == "Blue")
            BlueCarButton.BackgroundColor = Color.FromArgb("#2196F3");
        else if (color == "Red")
            RedCarButton.BackgroundColor = Color.FromArgb("#F44336");
        else if (color == "Green")
            GreenCarButton.BackgroundColor = Color.FromArgb("#4CAF50");
    }

    // ===========================
    // NAVIGATION
    // ===========================
    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}