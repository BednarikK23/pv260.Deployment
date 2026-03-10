// <copyright file="Home.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Demo.BlazorWasmApp.Pages;

/// <summary>
/// The home page of the application, featuring the Dino Jump game.
/// </summary>
public partial class Home : IDisposable
{
    private const int GroundHeight = 40;
    private const double Gravity = -0.95;
    private const double JumpForce = 16;
    private const int GameWidth = 800;
    private const double BaseSpeed = 5.0;
    private const double MaxSpeed = 20.0;

    private readonly List<ObstacleData> obstacles = new();
    private readonly Random rng = new();

    private ElementReference gameDiv;
    private bool gameStarted;
    private bool gameOver;
    private int score;
    private int bestScore;
    private double playerY;
    private double velocityY;
    private bool isJumping;
    private double obstacleSpeed = BaseSpeed;
    private double spawnTimer;
    private double nextSpawnTime = 70;
    private CancellationTokenSource? cts;

    private string SpeedLabel => this.obstacleSpeed switch
    {
        < 8.0 => "Easy",
        < 12.0 => "Medium",
        < 16.0 => "Hard",
        < 20.0 => "Extreme",
        _ => "INSANE",
    };

    private string SpeedColor => this.obstacleSpeed switch
    {
        < 8.0 => "#4caf50",
        < 12.0 => "#ff9800",
        < 16.0 => "#f44336",
        < 20.0 => "#9c27b0",
        _ => "#212121",
    };

    /// <inheritdoc/>
    public void Dispose()
    {
        this.cts?.Cancel();
        this.cts?.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _ = this.gameDiv.FocusAsync();
        }
    }

    private void StartGame()
    {
        this.gameStarted = true;
        this.gameOver = false;
        this.score = 0;
        this.playerY = 0;
        this.velocityY = 0;
        this.isJumping = false;
        this.obstacles.Clear();
        this.obstacleSpeed = BaseSpeed;
        this.spawnTimer = 0;
        this.nextSpawnTime = 70;

        this.cts?.Cancel();
        this.cts?.Dispose();
        this.cts = new CancellationTokenSource();
        _ = this.GameLoopAsync(this.cts.Token);
    }

    private async Task GameLoopAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));
        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                this.Tick();
                await this.InvokeAsync(this.StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void Tick()
    {
        if (this.isJumping)
        {
            this.velocityY += Gravity;
            this.playerY += this.velocityY;
            if (this.playerY <= 0)
            {
                this.playerY = 0;
                this.velocityY = 0;
                this.isJumping = false;
            }
        }

        for (int i = this.obstacles.Count - 1; i >= 0; i--)
        {
            this.obstacles[i].X -= this.obstacleSpeed;
            if (this.obstacles[i].X < -50)
            {
                this.obstacles.RemoveAt(i);
            }
        }

        this.spawnTimer++;
        if (this.spawnTimer >= this.nextSpawnTime)
        {
            this.spawnTimer = 0;

            // At score 0: [60,100]  →  at score 1000: [30,60]  →  at score 2000: [22,42]
            int minSpawn = Math.Max(22, 60 - (this.score / 35));
            int maxSpawn = Math.Max(minSpawn + 8, 100 - (this.score / 25));
            this.nextSpawnTime = this.rng.Next(minSpawn, maxSpawn);
            this.obstacles.Add(new ObstacleData { X = GameWidth, Height = this.rng.Next(32, 58) });

            // Double-obstacle groups from score 700 onward (25% chance, rises to 45%)
            double doubleChance = Math.Min(0.45, 0.25 + (this.score * 0.00003));
            if (this.score > 700 && this.rng.NextDouble() < doubleChance)
            {
                this.obstacles.Add(new ObstacleData { X = GameWidth + 75, Height = this.rng.Next(30, 55) });
            }
        }

        foreach (var obs in this.obstacles)
        {
            if (obs.X > 58 && obs.X < 122 && this.playerY < obs.Height - 12)
            {
                this.EndGame();
                return;
            }
        }

        this.score++;

        // Reaches MaxSpeed at ~1500: comfortable for a new player up to ~1000
        this.obstacleSpeed = Math.Min(MaxSpeed, BaseSpeed + (this.score * 0.01));
    }

    private void EndGame()
    {
        if (this.score > this.bestScore)
        {
            this.bestScore = this.score;
        }

        this.gameOver = true;
        this.cts?.Cancel();
        _ = this.InvokeAsync(this.StateHasChanged);
    }

    private void Jump()
    {
        if (!this.isJumping && this.gameStarted && !this.gameOver)
        {
            this.isJumping = true;
            this.velocityY = JumpForce;
        }
    }

    private void OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Space")
        {
            if (!this.gameStarted || this.gameOver)
            {
                this.StartGame();
            }
            else
            {
                this.Jump();
            }
        }
    }

    private void OnClick()
    {
        if (!this.gameStarted || this.gameOver)
        {
            this.StartGame();
        }
        else
        {
            this.Jump();
        }
    }

    private class ObstacleData
    {
        public double X { get; set; }

        public int Height { get; set; }
    }
}



