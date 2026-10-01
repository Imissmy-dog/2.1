using System;
using System.IO;
using System.Text.Json;

namespace SnakeGame;

public class HighScoreData
{
    public int HighScore { get; set; } = 0;
    public DateTime DateAchieved { get; set; } = DateTime.MinValue;
    public int GamesPlayed { get; set; } = 0;
}

public static class HighScoreManager
{
    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AntigravitySnakeGame");

    private static readonly string FilePath = Path.Combine(FolderPath, "highscore.json");

    public static HighScoreData Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var data = JsonSerializer.Deserialize<HighScoreData>(json);
                if (data != null) return data;
            }
        }
        catch
        {
            // Fallback to local directory if AppData fails
            try
            {
                if (File.Exists("highscore.json"))
                {
                    string json = File.ReadAllText("highscore.json");
                    var data = JsonSerializer.Deserialize<HighScoreData>(json);
                    if (data != null) return data;
                }
            }
            catch
            {
                // Ignore and return fresh
            }
        }

        return new HighScoreData();
    }

    public static bool SaveIfHigher(int currentScore, ref HighScoreData data)
    {
        data.GamesPlayed++;
        bool isNewRecord = false;

        if (currentScore > data.HighScore)
        {
            data.HighScore = currentScore;
            data.DateAchieved = DateTime.Now;
            isNewRecord = true;
        }

        Save(data);
        return isNewRecord;
    }

    public static void Save(HighScoreData data)
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            try
            {
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("highscore.json", json);
            }
            catch
            {
                // Silently ignore disk write issues
            }
        }
    }
}
