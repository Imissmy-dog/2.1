namespace SnakeGame;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--selftest")
        {
            return RunSelfTest();
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
        return 0;
    }

    private static int RunSelfTest()
    {
        Console.WriteLine("=== SNAKE GAME SELF-TEST ===");

        // 1. Test Themes & Diluted Checkerboard
        var themes = BoardTheme.AllThemes;
        Console.WriteLine($"Themes count: {themes.Length}");
        if (themes.Length < 4) throw new Exception("Expected at least 4 themes");
        foreach (var t in themes)
        {
            Console.WriteLine($"Theme '{t.Name}': TileA={t.TileA}, TileB={t.TileB}");
        }

        // 2. Test High Score Persistence
        var initialData = HighScoreManager.Load();
        Console.WriteLine($"Current loaded high score: {initialData.HighScore}");
        var testData = new HighScoreData { HighScore = 9999, DateAchieved = DateTime.Now, GamesPlayed = 1 };
        HighScoreManager.Save(testData);
        var reloadedData = HighScoreManager.Load();
        if (reloadedData.HighScore != 9999) throw new Exception("High score save/reload failed!");
        Console.WriteLine("High score save and load test passed!");
        // Restore initial
        HighScoreManager.Save(initialData);

        // 3. Test GameEngine
        var engine = new GameEngine();
        if (engine.State != GameState.Ready) throw new Exception("Engine should start in Ready state");
        if (engine.Snake.Count != 3) throw new Exception("Snake should start with 3 segments");

        engine.EnqueueDirection(Direction.Right);
        if (engine.State != GameState.Playing) throw new Exception("First move should transition to Playing");

        var headBefore = engine.Snake[0];
        engine.Update();
        var headAfter = engine.Snake[0];
        if (headAfter.X != headBefore.X + 1 || headAfter.Y != headBefore.Y)
            throw new Exception("Snake did not move Right!");

        Console.WriteLine("Snake movement test passed!");

        // 4. Test Pause
        engine.TogglePause();
        if (engine.State != GameState.Paused) throw new Exception("Pause toggle failed");
        engine.TogglePause();
        if (engine.State != GameState.Playing) throw new Exception("Resume toggle failed");
        Console.WriteLine("Pause/Resume test passed!");

        // 5. Test Wall Collision / Game Over
        while (engine.State == GameState.Playing)
        {
            engine.Update();
        }
        if (engine.State != GameState.GameOver) throw new Exception("Snake should reach Game Over at wall");
        Console.WriteLine("Wall collision & Game Over test passed!");

        Console.WriteLine("ALL TESTS PASSED SUCCESSFULLY! 🐍✨");
        return 0;
    }
}