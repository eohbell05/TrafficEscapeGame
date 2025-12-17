using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Timer = System.Timers.Timer;

namespace TrafficEscapeGame;

public partial class GamePage : ContentPage
{
    private int _playerLane = 1;
    private readonly List<Image> _enemies = new();
    private readonly List<Image> _jerryCans = new();
    private readonly Dictionary<Image, int> _enemySpeeds = new();
    private int _moveCounter = 0;
    private readonly Random _random = new();
    private Timer? _gameTimer;

    // Score
    private int _score;
    private int _highScore;
    private int _carsDogged;
    private DateTime _gameStartTime;

    // Fuel
    private double _fuel = 100.0;
    private const double FuelConsumptionRate = 2.0;
    private const double FuelPerCanPercent = 10.0;

    public GamePage()
    {
        InitializeComponent();
        _highScore = Preferences.Get("HighScore", 0);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Grid.SetRow(PlayerCar, 7);
        Grid.SetColumn(PlayerCar, 1);
        PlayerCar.TranslationX = 0;

        // Load and apply car color
        string carColor = Preferences.Get("CarColor", "Blue");
        ApplyCarColor(carColor);

        _score = 0;
        _carsDogged = 0;
        _fuel = 100;
        _gameStartTime = DateTime.Now;
        _moveCounter = 0;

        UpdateScoreDisplay();
        UpdateFuelDisplay();
        HighScoreLabel.Text = $"Best: {_highScore}";

        string difficulty = Preferences.Get("Difficulty", "Easy");
        int timerSpeed = difficulty switch
        {
            "Medium" => 350,
            "Hard" => 250,
            _ => 450
        };

        _gameTimer = new Timer(timerSpeed);
        _gameTimer.Elapsed += OnTimerElapsed;
        _gameTimer.AutoReset = true;
        _gameTimer.Start();
    }

    private void ApplyCarColor(string color)
    {
        // Change the car image based on selected color
        PlayerCar.Source = color switch
        {
            "Red" => "redplayercar.png",
            "Green" => "greenplayercar.png",
            _ => "newplayercar.png" // Blue (default)
        };
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MoveEnemies();
            UpdateTimeScore();
        });
    }

    private void UpdateTimeScore()
    {
        int seconds = (int)(DateTime.Now - _gameStartTime).TotalSeconds;
        _score = (_carsDogged * 10) + seconds;
        UpdateScoreDisplay();

        _fuel -= FuelConsumptionRate * (_gameTimer!.Interval / 1000.0);
        if (_fuel < 0) _fuel = 0;

        UpdateFuelDisplay();

        if (_fuel <= 0)
            GameOverOutOfFuel();
    }

    private void UpdateScoreDisplay()
    {
        ScoreLabel.Text = $"Score: {_score}";
    }

    private void UpdateFuelDisplay()
    {
        FuelLabel.Text = $"{(int)_fuel}%";
        FuelBar.Progress = _fuel / 100.0;

        if (_fuel > 50)
            FuelBar.ProgressColor = Color.FromArgb("#4CAF50");
        else if (_fuel > 25)
            FuelBar.ProgressColor = Color.FromArgb("#FF9800");
        else
            FuelBar.ProgressColor = Color.FromArgb("#F44336");
    }

    // ================= PLAYER =================
    private void LeftButton_Clicked(object sender, EventArgs e)
    {
        if (_playerLane > 0)
        {
            _playerLane--;
            MovePlayerToLane();
        }
    }

    private void RightButton_Clicked(object sender, EventArgs e)
    {
        if (_playerLane < 2)
        {
            _playerLane++;
            MovePlayerToLane();
        }
    }

    private async void MovePlayerToLane()
    {
        double moveDistance = (RoadArea.Width / 3) * 0.65;

        double targetX = _playerLane switch
        {
            0 => -moveDistance,
            2 => moveDistance,
            _ => 0
        };

        await PlayerCar.TranslateTo(targetX, 0, 200, Easing.CubicOut);
    }

    // ================= SPAWN =================
    private void SpawnEnemy()
    {
        int lane = _random.Next(0, 3);

        var enemy = new Image
        {
            Source = "redenemycar.png",
            WidthRequest = 100,
            HeightRequest = 150,
            Aspect = Aspect.Fill
        };

        RoadArea.Children.Add(enemy);
        Grid.SetRow(enemy, 0);
        Grid.SetColumn(enemy, 1);

        double moveDistance = (RoadArea.Width / 3) * 0.65;
        enemy.TranslationX = lane switch
        {
            0 => -moveDistance,
            2 => moveDistance,
            _ => 0
        };

        // RANDOMIZE ENEMY SPEED - creates gaps to avoid impossible situations
        _enemySpeeds[enemy] = _random.Next(1, 4); // 1=fast, 2=medium, 3=slow

        _enemies.Add(enemy);
    }

    private void SpawnJerryCan()
    {
        int lane = _random.Next(0, 3);

        var can = new Image
        {
            Source = "jerrycan.png",
            WidthRequest = 60,
            HeightRequest = 80,
            Aspect = Aspect.AspectFit
        };

        RoadArea.Children.Add(can);
        Grid.SetRow(can, 0);
        Grid.SetColumn(can, 1);

        double moveDistance = (RoadArea.Width / 3) * 0.65;
        can.TranslationX = lane switch
        {
            0 => -moveDistance,
            2 => moveDistance,
            _ => 0
        };

        _jerryCans.Add(can);
    }

    // ================= GAME LOOP =================
    private void MoveEnemies()
    {
        _moveCounter++; // Track game ticks for varied enemy speeds

        // Get difficulty and adjust spawn rates
        string difficulty = Preferences.Get("Difficulty", "Easy");

        int enemySpawnChance = difficulty switch
        {
            "Easy" => 10,      // 1 in 10 chance (less enemies)
            "Medium" => 8,     // 1 in 8 chance
            "Hard" => 5,       // 1 in 5 chance (more enemies!)
            _ => 10
        };

        int jerryCanSpawnChance = difficulty switch
        {
            "Easy" => 10,      // 1 in 10 (more fuel)
            "Medium" => 12,    // 1 in 12
            "Hard" => 15,      // 1 in 15 (less fuel - harder!)
            _ => 12
        };

        // Enemy spawn with difficulty-based rate
        if (_random.Next(0, enemySpawnChance) == 0)
            SpawnEnemy();

        // Jerry can spawn with difficulty-based rate
        if (_random.Next(0, jerryCanSpawnChance) == 0)
            SpawnJerryCan();

        var removeEnemies = new List<Image>();
        var removeCans = new List<Image>();

        // Move enemies with varied speeds
        foreach (var enemy in _enemies)
        {
            // Only move this enemy if the counter matches its speed
            if (_moveCounter % _enemySpeeds[enemy] == 0)
            {
                int row = Grid.GetRow(enemy);
                if (row < 7)
                    Grid.SetRow(enemy, row + 1);
                else
                    removeEnemies.Add(enemy);
            }

            // Check collision
            if (Grid.GetRow(enemy) == Grid.GetRow(PlayerCar) &&
                Math.Abs(enemy.TranslationX - PlayerCar.TranslationX) < 50)
            {
                GameOver();
                return;
            }
        }

        // Move jerry cans (normal speed)
        foreach (var can in _jerryCans)
        {
            int row = Grid.GetRow(can);
            if (row < 7)
                Grid.SetRow(can, row + 1);
            else
                removeCans.Add(can);

            if (Grid.GetRow(can) == Grid.GetRow(PlayerCar) &&
                Math.Abs(can.TranslationX - PlayerCar.TranslationX) < 50)
            {
                _fuel += FuelPerCanPercent;
                if (_fuel > 100) _fuel = 100;

                UpdateFuelDisplay();
                _score += 20;
                UpdateScoreDisplay();
                removeCans.Add(can);
            }
        }

        // Remove off-screen enemies and clean up speed tracking
        foreach (var e in removeEnemies)
        {
            RoadArea.Children.Remove(e);
            _enemies.Remove(e);
            _enemySpeeds.Remove(e); // Clean up speed dictionary
            _carsDogged++;
        }

        // Remove collected jerry cans
        foreach (var c in removeCans)
        {
            RoadArea.Children.Remove(c);
            _jerryCans.Remove(c);
        }
    }

    // ================= GAME OVER =================
    private async void GameOver()
    {
        _gameTimer?.Stop();
        if (_score > _highScore)
        {
            _highScore = _score;
            Preferences.Set("HighScore", _highScore);
        }

        CleanupGame();

        bool playAgain = await DisplayAlert(
            "Game Over",
            $"Score: {_score}\nCars Dodged: {_carsDogged}\nBest: {_highScore}",
            "Play Again",
            "Main Menu");

        if (playAgain)
            ResetGame();
        else
            await Shell.Current.GoToAsync("//MainPage");
    }

    private async void GameOverOutOfFuel()
    {
        _gameTimer?.Stop();
        CleanupGame();

        bool playAgain = await DisplayAlert(
            "Out of Fuel",
            $"Score: {_score}\nCars Dodged: {_carsDogged}",
            "Play Again",
            "Main Menu");

        if (playAgain)
            ResetGame();
        else
            await Shell.Current.GoToAsync("//MainPage");
    }

    private void CleanupGame()
    {
        foreach (var e in _enemies) RoadArea.Children.Remove(e);
        foreach (var c in _jerryCans) RoadArea.Children.Remove(c);
        _enemies.Clear();
        _jerryCans.Clear();
        _enemySpeeds.Clear(); // Clear speed tracking
    }

    private void ResetGame()
    {
        _playerLane = 1;
        PlayerCar.TranslationX = 0;
        _fuel = 100;
        _score = 0;
        _carsDogged = 0;
        _moveCounter = 0;
        _gameStartTime = DateTime.Now;

        // Reapply car color after reset
        string carColor = Preferences.Get("CarColor", "Blue");
        ApplyCarColor(carColor);

        UpdateScoreDisplay();
        UpdateFuelDisplay();
        _gameTimer?.Start();
    }
}