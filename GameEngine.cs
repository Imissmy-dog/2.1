using System;
using System.Collections.Generic;
using System.Drawing;

namespace SnakeGame;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum GameState
{
    Ready,
    Playing,
    Paused,
    GameOver
}

public enum FoodType
{
    Regular,
    Golden
}

public struct FoodItem
{
    public Point Position;
    public FoodType Type;
    public int TicksRemaining; // For golden food timer
}

public class GameEngine
{
    public const int GridWidth = 24;
    public const int GridHeight = 24;

    public List<Point> Snake { get; private set; } = new();
    public Direction CurrentDirection { get; private set; } = Direction.Right;
    public GameState State { get; private set; } = GameState.Ready;
    
    public int Score { get; private set; } = 0;
    public HighScoreData HighScoreInfo { get; private set; }
    public bool IsNewRecordAchieved { get; private set; } = false;

    public FoodItem CurrentFood { get; private set; }
    public FoodItem? BonusFood { get; private set; }

    private readonly Queue<Direction> _inputBuffer = new();
    private readonly Random _random = new();

    public int BaseSpeedMs { get; set; } = 115;
    public int CurrentSpeedMs
    {
        get
        {
            // Speed up slightly as score increases (floor at 55ms)
            int speedIncrease = (Score / 3) * 3;
            return Math.Max(55, BaseSpeedMs - speedIncrease);
        }
    }

    public GameEngine()
    {
        HighScoreInfo = HighScoreManager.Load();
        Reset();
    }

    public void Reset()
    {
        Snake.Clear();
        _inputBuffer.Clear();
        Score = 0;
        IsNewRecordAchieved = false;

        int startX = GridWidth / 3;
        int startY = GridHeight / 2;

        // Snake starts with 3 segments
        Snake.Add(new Point(startX, startY));
        Snake.Add(new Point(startX - 1, startY));
        Snake.Add(new Point(startX - 2, startY));

        CurrentDirection = Direction.Right;
        State = GameState.Ready;
        BonusFood = null;

        SpawnRegularFood();
    }

    public void EnqueueDirection(Direction newDir)
    {
        if (State == GameState.Ready)
        {
            // First move starts the game
            if (!IsOpposite(CurrentDirection, newDir))
            {
                CurrentDirection = newDir;
            }
            State = GameState.Playing;
            return;
        }

        if (State != GameState.Playing) return;

        // Buffer up to 2 future moves for buttery-smooth responsiveness
        if (_inputBuffer.Count < 2)
        {
            Direction lastPlanned = _inputBuffer.Count > 0 ? _inputBuffer.ToArray()[_inputBuffer.Count - 1] : CurrentDirection;
            if (!IsOpposite(lastPlanned, newDir) && lastPlanned != newDir)
            {
                _inputBuffer.Enqueue(newDir);
            }
        }
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
        {
            State = GameState.Paused;
        }
        else if (State == GameState.Paused)
        {
            State = GameState.Playing;
        }
    }

    public void Update()
    {
        if (State != GameState.Playing) return;

        // Dequeue next direction
        if (_inputBuffer.Count > 0)
        {
            CurrentDirection = _inputBuffer.Dequeue();
        }

        Point head = Snake[0];
        Point newHead = CurrentDirection switch
        {
            Direction.Up => new Point(head.X, head.Y - 1),
            Direction.Down => new Point(head.X, head.Y + 1),
            Direction.Left => new Point(head.X - 1, head.Y),
            Direction.Right => new Point(head.X + 1, head.Y),
            _ => head
        };

        // Wall collision check
        if (newHead.X < 0 || newHead.X >= GridWidth || newHead.Y < 0 || newHead.Y >= GridHeight)
        {
            TriggerGameOver();
            return;
        }

        // Self collision check (excluding the tip of the tail which will move unless we eat)
        for (int i = 0; i < Snake.Count - 1; i++)
        {
            if (Snake[i] == newHead)
            {
                TriggerGameOver();
                return;
            }
        }

        bool ateFood = false;

        // Check regular food
        if (newHead == CurrentFood.Position)
        {
            Score += 1;
            ateFood = true;
            SoundManager.PlayEat();
            CheckHighScore();
            SpawnRegularFood();

            // Chance to spawn golden bonus apple
            if (BonusFood == null && _random.Next(100) < 25)
            {
                SpawnBonusFood();
            }
        }
        // Check bonus golden apple
        else if (BonusFood.HasValue && newHead == BonusFood.Value.Position)
        {
            Score += 3;
            ateFood = true;
            BonusFood = null;
            SoundManager.PlayBonus();
            CheckHighScore();
        }

        // Move snake
        Snake.Insert(0, newHead);
        if (!ateFood)
        {
            Snake.RemoveAt(Snake.Count - 1);
        }

        // Update bonus food lifespan
        if (BonusFood.HasValue)
        {
            var bonus = BonusFood.Value;
            bonus.TicksRemaining--;
            if (bonus.TicksRemaining <= 0)
            {
                BonusFood = null;
            }
            else
            {
                BonusFood = bonus;
            }
        }
    }

    private void CheckHighScore()
    {
        if (Score > HighScoreInfo.HighScore)
        {
            if (!IsNewRecordAchieved)
            {
                IsNewRecordAchieved = true;
                SoundManager.PlayHighScore();
            }
            HighScoreInfo.HighScore = Score;
            HighScoreInfo.DateAchieved = DateTime.Now;
        }
    }

    private void TriggerGameOver()
    {
        State = GameState.GameOver;
        SoundManager.PlayDie();

        var highData = HighScoreInfo;
        HighScoreManager.SaveIfHigher(Score, ref highData);
        HighScoreInfo = highData;
    }

    private void SpawnRegularFood()
    {
        CurrentFood = new FoodItem
        {
            Position = GetRandomEmptyCell(),
            Type = FoodType.Regular,
            TicksRemaining = 0
        };
    }

    private void SpawnBonusFood()
    {
        Point pos = GetRandomEmptyCell();
        if (pos != CurrentFood.Position)
        {
            BonusFood = new FoodItem
            {
                Position = pos,
                Type = FoodType.Golden,
                TicksRemaining = 50 // roughly 5-6 seconds of active game time
            };
        }
    }

    private Point GetRandomEmptyCell()
    {
        var occupied = new HashSet<Point>(Snake);
        if (BonusFood.HasValue) occupied.Add(BonusFood.Value.Position);

        var freeCells = new List<Point>();
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                var pt = new Point(x, y);
                if (!occupied.Contains(pt))
                {
                    freeCells.Add(pt);
                }
            }
        }

        if (freeCells.Count == 0)
        {
            // Board is filled! Win scenario
            return new Point(0, 0);
        }

        return freeCells[_random.Next(freeCells.Count)];
    }

    private static bool IsOpposite(Direction d1, Direction d2)
    {
        return (d1 == Direction.Up && d2 == Direction.Down) ||
               (d1 == Direction.Down && d2 == Direction.Up) ||
               (d1 == Direction.Left && d2 == Direction.Right) ||
               (d1 == Direction.Right && d2 == Direction.Left);
    }
}
