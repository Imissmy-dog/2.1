using System.Drawing;

namespace SnakeGame;

public class BoardTheme
{
    public string Name { get; set; } = "Meadow (Diluted)";
    public Color TileA { get; set; }
    public Color TileB { get; set; }
    public Color SnakeHead { get; set; }
    public Color SnakeBody { get; set; }
    public Color SnakeOutline { get; set; }
    public Color FoodColor { get; set; }
    public Color FoodLeaf { get; set; }
    public Color HeaderBg { get; set; }
    public Color HeaderText { get; set; }
    public Color HeaderAccent { get; set; }
    public Color OverlayBg { get; set; }

    public static BoardTheme[] AllThemes => new[]
    {
        new BoardTheme
        {
            Name = "Classic Meadow (Diluted)",
            TileA = Color.FromArgb(170, 215, 81),
            TileB = Color.FromArgb(162, 209, 73),
            SnakeHead = Color.FromArgb(70, 115, 40),
            SnakeBody = Color.FromArgb(90, 140, 52),
            SnakeOutline = Color.FromArgb(55, 92, 32),
            FoodColor = Color.FromArgb(231, 71, 29),
            FoodLeaf = Color.FromArgb(64, 148, 55),
            HeaderBg = Color.FromArgb(74, 117, 44),
            HeaderText = Color.White,
            HeaderAccent = Color.FromArgb(255, 215, 0),
            OverlayBg = Color.FromArgb(190, 30, 45, 20)
        },
        new BoardTheme
        {
            Name = "Dark Slate (Diluted)",
            TileA = Color.FromArgb(43, 52, 64),
            TileB = Color.FromArgb(37, 44, 55),
            SnakeHead = Color.FromArgb(52, 211, 153),
            SnakeBody = Color.FromArgb(16, 185, 129),
            SnakeOutline = Color.FromArgb(5, 120, 80),
            FoodColor = Color.FromArgb(248, 113, 113),
            FoodLeaf = Color.FromArgb(74, 222, 128),
            HeaderBg = Color.FromArgb(24, 29, 38),
            HeaderText = Color.White,
            HeaderAccent = Color.FromArgb(251, 191, 36),
            OverlayBg = Color.FromArgb(200, 15, 20, 28)
        },
        new BoardTheme
        {
            Name = "Soft Pastel (Diluted)",
            TileA = Color.FromArgb(240, 244, 237),
            TileB = Color.FromArgb(226, 234, 222),
            SnakeHead = Color.FromArgb(68, 108, 110),
            SnakeBody = Color.FromArgb(92, 140, 142),
            SnakeOutline = Color.FromArgb(50, 80, 82),
            FoodColor = Color.FromArgb(235, 94, 85),
            FoodLeaf = Color.FromArgb(100, 160, 105),
            HeaderBg = Color.FromArgb(68, 108, 110),
            HeaderText = Color.White,
            HeaderAccent = Color.FromArgb(244, 162, 97),
            OverlayBg = Color.FromArgb(185, 45, 60, 62)
        },
        new BoardTheme
        {
            Name = "Warm Sand (Diluted)",
            TileA = Color.FromArgb(243, 234, 219),
            TileB = Color.FromArgb(234, 223, 204),
            SnakeHead = Color.FromArgb(186, 92, 49),
            SnakeBody = Color.FromArgb(217, 119, 74),
            SnakeOutline = Color.FromArgb(145, 70, 36),
            FoodColor = Color.FromArgb(205, 50, 50),
            FoodLeaf = Color.FromArgb(95, 150, 75),
            HeaderBg = Color.FromArgb(112, 66, 42),
            HeaderText = Color.White,
            HeaderAccent = Color.FromArgb(255, 200, 87),
            OverlayBg = Color.FromArgb(185, 75, 45, 30)
        }
    };
}
