using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Demo.BlazorWasmApp.Pages;

public partial class Home : IDisposable
{
    private ElementReference gameDiv;

    private bool gameStarted;
    private bool gameOver;
    private int score;
    private int bestScore;

    private double playerY;
    private double velocityY;
    private bool isJumping;

    private const int GroundHeight = 40;
    private const double Gravity = -0.95;
    private const double JumpForce = 16;
    private const int GameWidth = 800;

    private readonly List<ObstacleData> obstacles = new();
    private double obstacleSpeed = 5;
    private double spawnTimer;
    private double nextSpawnTime = 70;

    private const double BaseSpeed = 5.0;
    private const double MaxSpeed = 20.0;

    private string SpeedLabel => obstacleSpeed switch
    {
        < 8.0 => "Easy",
        < 12.0 => "Medium",
        < 16.0 => "Hard",
        < 20.0 => "Extreme",
        _ => "INSANE"
    };

    private string SpeedColor => obstacleSpeed switch
    {
        < 8.0 => "#4caf50",
        < 12.0 => "#ff9800",
        < 16.0 => "#f44336",
        < 20.0 => "#9c27b0",
        _ => "#212121"
    };

    private CancellationTokenSource? cts;
    private readonly Random rng = new();

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _ = gameDiv.FocusAsync();
        }
    }

    private void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        score = 0;
        playerY = 0;
        velocityY = 0;
        isJumping = false;
        obstacles.Clear();
        obstacleSpeed = BaseSpeed;
        spawnTimer = 0;
        nextSpawnTime = 70;

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        _ = GameLoopAsync(cts.Token);
    }

    private async Task GameLoopAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));
        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                Tick();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException) { }
    }

    private void Tick()
    {
        if (isJumping)
        {
            velocityY += Gravity;
            playerY += velocityY;
            if (playerY <= 0)
            {
                playerY = 0;
                velocityY = 0;
                isJumping = false;
            }
        }

        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            obstacles[i].X -= obstacleSpeed;
            if (obstacles[i].X < -50)
                obstacles.RemoveAt(i);
        }

        spawnTimer++;
        if (spawnTimer >= nextSpawnTime)
        {
            spawnTimer = 0;
            // At score 0: [60,100]  →  at score 1000: [30,60]  →  at score 2000: [22,42]
            int minSpawn = Math.Max(22, 60 - score / 35);
            int maxSpawn = Math.Max(minSpawn + 8, 100 - score / 25);
            nextSpawnTime = rng.Next(minSpawn, maxSpawn);
            obstacles.Add(new ObstacleData { X = GameWidth, Height = rng.Next(32, 58) });
            // Double-obstacle groups from score 700 onward (25% chance, rises to 45%)
            double doubleChance = Math.Min(0.45, 0.25 + score * 0.00003);
            if (score > 700 && rng.NextDouble() < doubleChance)
                obstacles.Add(new ObstacleData { X = GameWidth + 75, Height = rng.Next(30, 55) });
        }

        foreach (var obs in obstacles)
        {
            if (obs.X > 58 && obs.X < 122 && playerY < obs.Height - 12)
            {
                EndGame();
                return;
            }
        }

        score++;
        // Reaches MaxSpeed at ~1500: comfortable for a new player up to ~1000
        obstacleSpeed = Math.Min(MaxSpeed, BaseSpeed + score * 0.01);
    }

    private void EndGame()
    {
        if (score > bestScore)
            bestScore = score;
        gameOver = true;
        cts?.Cancel();
        _ = InvokeAsync(StateHasChanged);
    }

    private void Jump()
    {
        if (!isJumping && gameStarted && !gameOver)
        {
            isJumping = true;
            velocityY = JumpForce;
        }
    }

    private void OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Space")
        {
            if (!gameStarted || gameOver)
                StartGame();
            else
                Jump();
        }
    }

    private void OnClick()
    {
        if (!gameStarted || gameOver)
            StartGame();
        else
            Jump();
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    private class ObstacleData
    {
        public double X { get; set; }
        public int Height { get; set; }
    }
}
