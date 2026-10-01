using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SnakeGame;

public class GamePanel : Panel
{
    public GamePanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
    }
}

public partial class Form1 : Form
{
    private readonly GameEngine _engine;
    private readonly System.Windows.Forms.Timer _gameTimer;
    private BoardTheme _theme;

    // UI Controls
    private Panel _headerPanel = null!;
    private GamePanel _canvas = null!;
    private Panel _footerPanel = null!;
    private Label _lblScore = null!;
    private Label _lblHighScore = null!;
    private ComboBox _cboTheme = null!;
    private ComboBox _cboDifficulty = null!;
    private Button _btnMute = null!;
    private Button _btnRestart = null!;
    private Label _lblFooter = null!;

    public Form1()
    {
        InitializeComponent();

        _engine = new GameEngine();
        _theme = BoardTheme.AllThemes[0]; // Classic Meadow (Diluted)

        try
        {
            if (File.Exists("icon.ico"))
            {
                this.Icon = new Icon("icon.ico");
            }
        }
        catch { }

        this.FormClosing += (s, e) =>
        {
            var high = _engine.HighScoreInfo;
            HighScoreManager.SaveIfHigher(_engine.Score, ref high);
        };

        BuildCustomUi();

        _gameTimer = new System.Windows.Forms.Timer
        {
            Interval = _engine.CurrentSpeedMs
        };
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();

        UpdateScoreLabels();
    }

    private void BuildCustomUi()
    {
        this.SuspendLayout();

        // Top Header
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = _theme.HeaderBg,
            Padding = new Padding(12, 10, 12, 10)
        };

        // Score Card
        _lblScore = new Label
        {
            Text = "🍎 Score: 0",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(14, 12)
        };

        // High Score Card
        _lblHighScore = new Label
        {
            Text = $"🏆 Best: {_engine.HighScoreInfo.HighScore}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = _theme.HeaderAccent,
            AutoSize = true,
            Location = new Point(14, 40)
        };

        // Theme ComboBox
        var lblTheme = new Label
        {
            Text = "Theme:",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.Gainsboro,
            AutoSize = true,
            Location = new Point(190, 14)
        };

        _cboTheme = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9),
            Width = 145,
            Location = new Point(245, 11),
            TabStop = false
        };
        foreach (var t in BoardTheme.AllThemes)
        {
            _cboTheme.Items.Add(t.Name);
        }
        _cboTheme.SelectedIndex = 0;
        _cboTheme.SelectedIndexChanged += (s, e) =>
        {
            if (_cboTheme.SelectedIndex >= 0)
            {
                _theme = BoardTheme.AllThemes[_cboTheme.SelectedIndex];
                ApplyThemeColors();
                _canvas.Invalidate();
            }
        };

        // Difficulty ComboBox
        var lblDiff = new Label
        {
            Text = "Speed:",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.Gainsboro,
            AutoSize = true,
            Location = new Point(190, 42)
        };

        _cboDifficulty = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9),
            Width = 145,
            Location = new Point(245, 39),
            TabStop = false
        };
        _cboDifficulty.Items.AddRange(new object[] { "Relaxed (Easy)", "Normal (Standard)", "Speedy (Hard)" });
        _cboDifficulty.SelectedIndex = 1;
        _cboDifficulty.SelectedIndexChanged += (s, e) =>
        {
            _engine.BaseSpeedMs = _cboDifficulty.SelectedIndex switch
            {
                0 => 140,
                1 => 115,
                2 => 85,
                _ => 115
            };
            _gameTimer.Interval = _engine.CurrentSpeedMs;
        };

        // Mute Button
        _btnMute = new Button
        {
            Text = "🔊 Audio: ON",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Size = new Size(110, 28),
            Location = new Point(410, 11),
            BackColor = Color.FromArgb(240, 240, 240),
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
            Cursor = Cursors.Hand
        };
        _btnMute.FlatAppearance.BorderSize = 0;
        _btnMute.Click += (s, e) => ToggleMute();

        // Restart Button
        _btnRestart = new Button
        {
            Text = "🔄 Restart",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Size = new Size(110, 28),
            Location = new Point(410, 40),
            BackColor = Color.FromArgb(240, 240, 240),
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
            Cursor = Cursors.Hand
        };
        _btnRestart.FlatAppearance.BorderSize = 0;
        _btnRestart.Click += (s, e) => RestartGame();

        _headerPanel.Controls.Add(_lblScore);
        _headerPanel.Controls.Add(_lblHighScore);
        _headerPanel.Controls.Add(lblTheme);
        _headerPanel.Controls.Add(_cboTheme);
        _headerPanel.Controls.Add(lblDiff);
        _headerPanel.Controls.Add(_cboDifficulty);
        _headerPanel.Controls.Add(_btnMute);
        _headerPanel.Controls.Add(_btnRestart);

        // Footer / Controls Hint
        _footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 32,
            BackColor = Color.FromArgb(30, 30, 30)
        };

        _lblFooter = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Controls: [Arrow keys / WASD] Move   •   [Space] Pause   •   [R] Restart   •   [M] Mute",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.LightGray
        };
        _footerPanel.Controls.Add(_lblFooter);

        _canvas = new GamePanel
        {
            Dock = DockStyle.Fill,
            BackColor = _theme.TileA
        };
        _canvas.Paint += Canvas_Paint;
        _canvas.Resize += (s, e) => _canvas.Invalidate();
        _canvas.MouseClick += (s, e) =>
        {
            if (_engine.State == GameState.GameOver)
            {
                RestartGame();
            }
            else if (_engine.State == GameState.Ready)
            {
                _engine.EnqueueDirection(_engine.CurrentDirection);
            }
        };

        this.Controls.Add(_canvas);
        this.Controls.Add(_headerPanel);
        this.Controls.Add(_footerPanel);

        this.ResumeLayout(false);
    }

    private void ApplyThemeColors()
    {
        _headerPanel.BackColor = _theme.HeaderBg;
        _lblScore.ForeColor = _theme.HeaderText;
        _lblHighScore.ForeColor = _engine.IsNewRecordAchieved ? Color.Gold : _theme.HeaderAccent;
        _canvas.BackColor = _theme.TileA;
    }

    private void ToggleMute()
    {
        SoundManager.IsMuted = !SoundManager.IsMuted;
        _btnMute.Text = SoundManager.IsMuted ? "🔇 Audio: OFF" : "🔊 Audio: ON";
    }

    private void RestartGame()
    {
        _engine.Reset();
        _gameTimer.Interval = _engine.CurrentSpeedMs;
        UpdateScoreLabels();
        _canvas.Invalidate();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (_engine.State == GameState.Playing)
        {
            _engine.Update();
            _gameTimer.Interval = _engine.CurrentSpeedMs;
            UpdateScoreLabels();
            _canvas.Invalidate();
        }
    }

    private void UpdateScoreLabels()
    {
        _lblScore.Text = $"🍎 Score: {_engine.Score}";

        if (_engine.IsNewRecordAchieved)
        {
            _lblHighScore.Text = $"🏆 Best: {_engine.HighScoreInfo.HighScore} (NEW!)";
            _lblHighScore.ForeColor = Color.Gold;
        }
        else
        {
            _lblHighScore.Text = $"🏆 Best: {_engine.HighScoreInfo.HighScore}";
            _lblHighScore.ForeColor = _theme.HeaderAccent;
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Up:
            case Keys.W:
                if (_engine.State == GameState.GameOver) RestartGame();
                _engine.EnqueueDirection(Direction.Up);
                return true;

            case Keys.Down:
            case Keys.S:
                if (_engine.State == GameState.GameOver) RestartGame();
                _engine.EnqueueDirection(Direction.Down);
                return true;

            case Keys.Left:
            case Keys.A:
                if (_engine.State == GameState.GameOver) RestartGame();
                _engine.EnqueueDirection(Direction.Left);
                return true;

            case Keys.Right:
            case Keys.D:
                if (_engine.State == GameState.GameOver) RestartGame();
                _engine.EnqueueDirection(Direction.Right);
                return true;

            case Keys.Space:
                if (_engine.State == GameState.GameOver)
                {
                    RestartGame();
                }
                else
                {
                    _engine.TogglePause();
                    _canvas.Invalidate();
                }
                return true;

            case Keys.R:
                RestartGame();
                return true;

            case Keys.M:
                ToggleMute();
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int canvasW = _canvas.ClientSize.Width;
        int canvasH = _canvas.ClientSize.Height;

        if (canvasW <= 0 || canvasH <= 0) return;

        int cellSize = Math.Min(canvasW / GameEngine.GridWidth, canvasH / GameEngine.GridHeight);
        if (cellSize < 4) cellSize = 4;

        int boardW = GameEngine.GridWidth * cellSize;
        int boardH = GameEngine.GridHeight * cellSize;
        int offsetX = (canvasW - boardW) / 2;
        int offsetY = (canvasH - boardH) / 2;

        // Draw outer backdrop
        using (var bgBrush = new SolidBrush(Color.FromArgb(25, 25, 25)))
        {
            g.FillRectangle(bgBrush, 0, 0, canvasW, canvasH);
        }

        // --- 1. DILUTED CHECKERBOARD BACKGROUND ---
        using var brushTileA = new SolidBrush(_theme.TileA);
        using var brushTileB = new SolidBrush(_theme.TileB);

        for (int x = 0; x < GameEngine.GridWidth; x++)
        {
            for (int y = 0; y < GameEngine.GridHeight; y++)
            {
                var brush = ((x + y) % 2 == 0) ? brushTileA : brushTileB;
                g.FillRectangle(brush, offsetX + x * cellSize, offsetY + y * cellSize, cellSize, cellSize);
            }
        }

        // Subtle board boundary border
        using (var borderPen = new Pen(_theme.SnakeOutline, 2))
        {
            g.DrawRectangle(borderPen, offsetX, offsetY, boardW, boardH);
        }

        // --- 2. REGULAR FOOD (APPLE) ---
        DrawApple(g, offsetX + _engine.CurrentFood.Position.X * cellSize,
                     offsetY + _engine.CurrentFood.Position.Y * cellSize,
                     cellSize, _theme.FoodColor, _theme.FoodLeaf);

        // --- 3. BONUS FOOD (GOLDEN APPLE) ---
        if (_engine.BonusFood.HasValue)
        {
            var bonus = _engine.BonusFood.Value;
            DrawBonusApple(g, offsetX + bonus.Position.X * cellSize,
                              offsetY + bonus.Position.Y * cellSize,
                              cellSize, bonus.TicksRemaining);
        }

        // --- 4. SNAKE ---
        DrawSnake(g, offsetX, offsetY, cellSize);

        // --- 5. OVERLAYS (Ready, Paused, GameOver) ---
        if (_engine.State == GameState.Ready)
        {
            DrawCenteredOverlay(g, canvasW, canvasH,
                "🐍 SNAKE GAME",
                "Press Arrow Keys or WASD to start moving",
                $"Your High Score: {_engine.HighScoreInfo.HighScore} points",
                Color.FromArgb(210, 20, 30, 20));
        }
        else if (_engine.State == GameState.Paused)
        {
            DrawCenteredOverlay(g, canvasW, canvasH,
                "⏸️ GAME PAUSED",
                "Press [Space] to resume playing",
                $"Current Score: {_engine.Score}",
                Color.FromArgb(190, 15, 20, 30));
        }
        else if (_engine.State == GameState.GameOver)
        {
            string recordMsg = _engine.IsNewRecordAchieved
                ? "🎉 FANTASTIC! NEW HIGH SCORE RECORD! 🎉"
                : $"High Score to beat: {_engine.HighScoreInfo.HighScore}";

            DrawCenteredOverlay(g, canvasW, canvasH,
                "💀 GAME OVER",
                $"Final Score: {_engine.Score} points",
                $"{recordMsg}\n\nPress [Space] or [R] to Play Again",
                Color.FromArgb(220, 40, 15, 15));
        }
    }

    private void DrawApple(Graphics g, int px, int py, int size, Color appleColor, Color leafColor)
    {
        int pad = size / 8;
        int appleSize = size - pad * 2;

        // Apple body
        using (var appleBrush = new SolidBrush(appleColor))
        {
            g.FillEllipse(appleBrush, px + pad, py + pad, appleSize, appleSize);
        }

        // Apple highlight
        using (var highlightBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
        {
            int hSize = Math.Max(2, appleSize / 4);
            g.FillEllipse(highlightBrush, px + pad + appleSize / 4, py + pad + appleSize / 5, hSize, hSize);
        }

        // Apple stem
        using (var stemPen = new Pen(Color.FromArgb(101, 67, 33), Math.Max(1.5f, size / 12f)))
        {
            g.DrawLine(stemPen, px + size / 2, py + pad + 2, px + size / 2 + 2, py + pad - 3);
        }

        // Apple leaf
        using (var leafBrush = new SolidBrush(leafColor))
        {
            int leafW = Math.Max(3, size / 4);
            int leafH = Math.Max(2, size / 6);
            g.FillEllipse(leafBrush, px + size / 2 + 1, py + pad - 4, leafW, leafH);
        }
    }

    private void DrawBonusApple(Graphics g, int px, int py, int size, int ticksRemaining)
    {
        // Golden glowing bonus apple
        int pad = size / 8;
        int appleSize = size - pad * 2;

        // Glow ring indicating remaining timer
        float progress = Math.Clamp(ticksRemaining / 50.0f, 0f, 1f);
        using (var timerPen = new Pen(Color.Gold, 2))
        {
            g.DrawArc(timerPen, px + 1, py + 1, size - 2, size - 2, -90, progress * 360f);
        }

        // Golden body
        using (var goldBrush = new LinearGradientBrush(
            new Rectangle(px + pad, py + pad, appleSize, appleSize),
            Color.FromArgb(255, 220, 50),
            Color.FromArgb(220, 160, 0),
            LinearGradientMode.ForwardDiagonal))
        {
            g.FillEllipse(goldBrush, px + pad, py + pad, appleSize, appleSize);
        }

        // Specular highlight
        using (var highlightBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
        {
            int hSize = Math.Max(2, appleSize / 4);
            g.FillEllipse(highlightBrush, px + pad + appleSize / 4, py + pad + appleSize / 5, hSize, hSize);
        }

        // Stem & leaf
        using (var stemPen = new Pen(Color.FromArgb(120, 80, 20), Math.Max(1.5f, size / 12f)))
        {
            g.DrawLine(stemPen, px + size / 2, py + pad + 2, px + size / 2 + 2, py + pad - 3);
        }
        using (var leafBrush = new SolidBrush(Color.LimeGreen))
        {
            g.FillEllipse(leafBrush, px + size / 2 + 1, py + pad - 4, Math.Max(3, size / 4), Math.Max(2, size / 6));
        }
    }

    private void DrawSnake(Graphics g, int offsetX, int offsetY, int cellSize)
    {
        if (_engine.Snake.Count == 0) return;

        int margin = Math.Max(1, cellSize / 10);
        int cornerRadius = Math.Max(2, cellSize / 4);

        // Draw body segments (from tail to neck)
        using (var bodyBrush = new SolidBrush(_theme.SnakeBody))
        using (var outlinePen = new Pen(_theme.SnakeOutline, 1))
        {
            for (int i = _engine.Snake.Count - 1; i >= 1; i--)
            {
                var pt = _engine.Snake[i];
                var rect = new Rectangle(
                    offsetX + pt.X * cellSize + margin,
                    offsetY + pt.Y * cellSize + margin,
                    cellSize - margin * 2,
                    cellSize - margin * 2);

                GraphicsHelpers.FillRoundedRectangle(g, bodyBrush, rect, cornerRadius);
                GraphicsHelpers.DrawRoundedRectangle(g, outlinePen, rect, cornerRadius);
            }
        }

        // Draw head
        var headPt = _engine.Snake[0];
        int headX = offsetX + headPt.X * cellSize + margin;
        int headY = offsetY + headPt.Y * cellSize + margin;
        int headW = cellSize - margin * 2;
        int headH = cellSize - margin * 2;
        var headRect = new Rectangle(headX, headY, headW, headH);

        using (var headBrush = new SolidBrush(_theme.SnakeHead))
        using (var headPen = new Pen(_theme.SnakeOutline, 1.5f))
        {
            GraphicsHelpers.FillRoundedRectangle(g, headBrush, headRect, cornerRadius + 2);
            GraphicsHelpers.DrawRoundedRectangle(g, headPen, headRect, cornerRadius + 2);
        }

        // Eyes
        DrawSnakeEyes(g, headX, headY, headW, headH, _engine.CurrentDirection, _engine.State == GameState.GameOver);
    }

    private void DrawSnakeEyes(Graphics g, int x, int y, int w, int h, Direction dir, bool isDead)
    {
        int eyeRadius = Math.Max(2, w / 5);
        int pupilRadius = Math.Max(1, eyeRadius / 2);

        Point eye1, eye2;
        Point pupilOffset;

        switch (dir)
        {
            case Direction.Up:
                eye1 = new Point(x + w / 4, y + h / 4);
                eye2 = new Point(x + (w * 3) / 4, y + h / 4);
                pupilOffset = new Point(0, -pupilRadius / 2);
                break;
            case Direction.Down:
                eye1 = new Point(x + w / 4, y + (h * 3) / 4);
                eye2 = new Point(x + (w * 3) / 4, y + (h * 3) / 4);
                pupilOffset = new Point(0, pupilRadius / 2);
                break;
            case Direction.Left:
                eye1 = new Point(x + w / 4, y + h / 4);
                eye2 = new Point(x + w / 4, y + (h * 3) / 4);
                pupilOffset = new Point(-pupilRadius / 2, 0);
                break;
            case Direction.Right:
            default:
                eye1 = new Point(x + (w * 3) / 4, y + h / 4);
                eye2 = new Point(x + (w * 3) / 4, y + (h * 3) / 4);
                pupilOffset = new Point(pupilRadius / 2, 0);
                break;
        }

        if (isDead)
        {
            // Dead X eyes
            using var deadPen = new Pen(Color.White, 2);
            DrawCross(g, deadPen, eye1, eyeRadius);
            DrawCross(g, deadPen, eye2, eyeRadius);
        }
        else
        {
            // White sclera
            using var whiteBrush = new SolidBrush(Color.White);
            g.FillEllipse(whiteBrush, eye1.X - eyeRadius, eye1.Y - eyeRadius, eyeRadius * 2, eyeRadius * 2);
            g.FillEllipse(whiteBrush, eye2.X - eyeRadius, eye2.Y - eyeRadius, eyeRadius * 2, eyeRadius * 2);

            // Pupil
            using var blackBrush = new SolidBrush(Color.Black);
            g.FillEllipse(blackBrush,
                eye1.X + pupilOffset.X - pupilRadius,
                eye1.Y + pupilOffset.Y - pupilRadius,
                pupilRadius * 2, pupilRadius * 2);
            g.FillEllipse(blackBrush,
                eye2.X + pupilOffset.X - pupilRadius,
                eye2.Y + pupilOffset.Y - pupilRadius,
                pupilRadius * 2, pupilRadius * 2);
        }
    }

    private void DrawCross(Graphics g, Pen pen, Point center, int r)
    {
        g.DrawLine(pen, center.X - r, center.Y - r, center.X + r, center.Y + r);
        g.DrawLine(pen, center.X - r, center.Y + r, center.X + r, center.Y - r);
    }

    private void DrawCenteredOverlay(Graphics g, int w, int h, string title, string line1, string line2, Color bgTint)
    {
        using (var overlayBrush = new SolidBrush(bgTint))
        {
            g.FillRectangle(overlayBrush, 0, 0, w, h);
        }

        int boxW = Math.Min(480, w - 40);
        int boxH = 220;
        int boxX = (w - boxW) / 2;
        int boxY = (h - boxH) / 2;

        var boxRect = new Rectangle(boxX, boxY, boxW, boxH);
        using (var cardBrush = new SolidBrush(Color.FromArgb(235, 20, 24, 30)))
        using (var cardPen = new Pen(Color.FromArgb(160, 255, 255, 255), 1.5f))
        {
            GraphicsHelpers.FillRoundedRectangle(g, cardBrush, boxRect, 16);
            GraphicsHelpers.DrawRoundedRectangle(g, cardPen, boxRect, 16);
        }

        // Title
        using (var titleFont = new Font("Segoe UI", 20, FontStyle.Bold))
        using (var titleBrush = new SolidBrush(Color.Gold))
        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        {
            g.DrawString(title, titleFont, titleBrush, new RectangleF(boxX, boxY + 20, boxW, 40), sf);
        }

        // Line 1
        using (var textFont = new Font("Segoe UI", 12, FontStyle.Regular))
        using (var textBrush = new SolidBrush(Color.White))
        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        {
            g.DrawString(line1, textFont, textBrush, new RectangleF(boxX + 15, boxY + 70, boxW - 30, 35), sf);
        }

        // Line 2
        using (var subFont = new Font("Segoe UI", 10, FontStyle.Bold))
        using (var subBrush = new SolidBrush(Color.LightGreen))
        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        {
            g.DrawString(line2, subFont, subBrush, new RectangleF(boxX + 15, boxY + 115, boxW - 30, 75), sf);
        }
    }
}
