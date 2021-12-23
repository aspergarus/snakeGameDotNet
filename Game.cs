using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Threading;

namespace SnakeGame
{
    public partial class Game : Form
    {
        int[] snake = { 3, 3 };
        int[] snakePrev;
        List<int[]> tail = new List<int[]>();
        int snakeDir = -1;
        int snakePrevDir = -100;
        int gridSize = 30;

        int[] food = { 3, 5 };
        int[] field = { 30, 30 };

        int dx = 0;
        int dy = 0;

        int gameTimeFrame = 100;
        int[] gameTimeFrames = { 25, 50, 100, 200, 500 };

        Color snakeColor = Color.FromArgb(255, 28, 91, 100);
        Color tailColor = Color.FromArgb(255, 15, 87, 78);
        Color foodColor = Color.FromArgb(255, 97, 50, 50);
        Color backGround = Color.FromArgb(255, 17, 18, 20);
        Color backGround2 = Color.FromArgb(255, 23, 24, 26);

        Dictionary<string, int> dirMap = new Dictionary<string, int> {
            { Keys.Up.ToString(), 0 },
            { Keys.Down.ToString(), 1 },
            { Keys.Left.ToString(), 3 },
            { Keys.Right.ToString(), 4 }
        };

        public Game()
        {
            Width = 900;
            Height = 900;
            StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true);

            KeyDown += Form1_KeyDown;

            var timer = new DispatcherTimer();
            timer.Tick += (s, e) => {
                MakeMove();
                Refresh();
                timer.Interval = new TimeSpan(0, 0, 0, 0, gameTimeFrame);
            };
            timer.Interval = new TimeSpan(0, 0, 0, 0, gameTimeFrame);
            timer.Start();
        }

        private void MakeMove()
        {
            snakePrev = (int[]) snake.Clone();
            snake[0] += dx;
            snake[1] += dy;

            if (isOut() || eatSelf())
            {
                tail.Clear();
                snake = getFreeCell();
                food = getFreeCell();
                snakeDir = -100;
                snakePrevDir = -100;
                dx = 0;
                dy = 0;
                return;
            }

            tail.Add(snakePrev);

            if (eatFood()) {
                food = getFreeCell();
            }
            else {
                tail = tail.GetRange(1, tail.Count - 1);
            }
        }

        private bool isOut() {
            return snake[0] > field[0] || snake[0] < 0 || snake[1] > field[1] || snake[1] < 0;
        }

        private bool eatSelf() {
            foreach (int[] coord in tail) {
                if (coord[0] == snake[0] && coord[1] == snake[1]) {
                    return true;
                }
            }
            return false;
        }

        private bool eatFood() {
            return snake[0] == food[0] && snake[1] == food[1];
        }

        private int[] getFreeCell() {
            List<int[]> freeField = new List<int[]>();
            for (int x = 0; x < field[0]; x++) {
                for (int y = 0; y < field[1]; y++) {
                    int[] curCoord = {x, y};
                    if (tail.IndexOf(curCoord) == -1 && snake[0] != x && snake[1] != y) {
                        freeField.Add(curCoord);
                    }
                }
            }

            Random rnd = new Random();
            return freeField[rnd.Next(0, freeField.Count)];
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                return;
            }

            if (e.KeyCode == Keys.Add) {
                int curIndex = Array.FindIndex(gameTimeFrames, (int el) => el == gameTimeFrame);
                if (curIndex > 0)
                {
                    curIndex--;
                }
                gameTimeFrame = gameTimeFrames[curIndex];
                return;
            }
            if (e.KeyCode == Keys.Subtract) {
                int curIndex = Array.FindIndex(gameTimeFrames, (int el) => el == gameTimeFrame);
                if (curIndex < gameTimeFrames.Length - 1) {
                    curIndex++;
                }
                gameTimeFrame = gameTimeFrames[curIndex];
                return;
            }

            if (!dirMap.ContainsKey(e.KeyCode.ToString())) {
                return;
            }

            snakeDir = dirMap[e.KeyCode.ToString()];

            // Fix direction. Can't move to oposite direction from current
            if (Math.Abs(snakePrevDir - snakeDir) == 1)
            {
                snakeDir = snakePrevDir;
            }
            snakePrevDir = snakeDir;

            if (snakeDir == 0) { dx = 0; dy = -1; }
            if (snakeDir == 1) { dx = 0; dy = 1; }
            if (snakeDir == 3) { dx = -1; dy = 0; }
            if (snakeDir == 4) { dx = 1; dy = 0; }
        }

        protected override void OnPaint(PaintEventArgs args)
        {
            var g = args.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Black);
            g.TranslateTransform(0, 0);

            // Draw field
            SolidBrush back1 = new SolidBrush(backGround);
            SolidBrush back2 = new SolidBrush(backGround2);
            SolidBrush currentBrush;
            bool rowN = false, colN = false;
            for (int i = 0; i < field[0]; i++)
            {
                for (int j = 0; j < field[1]; j++)
                {
                    if (rowN != colN)
                    {
                        currentBrush = back1;
                    }
                    else
                    {
                        currentBrush = back2;
                    }
                    g.FillRectangle(currentBrush, i * gridSize, j * gridSize, gridSize, gridSize);
                    rowN = !rowN;
                }
                colN = !colN;
            }

            g.FillRectangle(new SolidBrush(snakeColor), snake[0] * gridSize, snake[1] * gridSize, gridSize - 1, gridSize - 1);
            drawIcon(g);
            g.FillRectangle(new SolidBrush(foodColor), food[0] * gridSize, food[1] * gridSize, gridSize - 1, gridSize - 1);
            foreach (int[] coord in tail) {
                g.FillRectangle(new SolidBrush(tailColor), coord[0] * gridSize, coord[1] * gridSize, gridSize - 1, gridSize - 1);
            }
        }
        private void drawIcon(Graphics g)
        {
            if (snakeDir != 0 && snakeDir != 1 && snakeDir != 3 && snakeDir != 4) {
                return;
            }
            
            int x = 0, y = 0, startAngle = 0;
            

            // snakeDir is up
            if (snakeDir == 0) {
                x = snake[0] * gridSize;
                y = snake[1] * gridSize - 10;
                startAngle = 60;
            }

            // snakeDir is down
            if (snakeDir == 1)
            {
                x = snake[0] * gridSize;
                y = snake[1] * gridSize + 10;
                startAngle = 240;
            }

            // snakeDir is right
            if (snakeDir == 3)
            {
                x = snake[0] * gridSize - 10;
                y = snake[1] * gridSize;
                startAngle = 330;
            }

            // snakeDir is left
            if (snakeDir == 4)
            {
                x = snake[0] * gridSize + 10;
                y = snake[1] * gridSize;
                startAngle = 150;
            }

            g.FillPie(new SolidBrush(backGround2), x, y, gridSize - 1, gridSize - 1, startAngle, 60);
        }
    }
}
