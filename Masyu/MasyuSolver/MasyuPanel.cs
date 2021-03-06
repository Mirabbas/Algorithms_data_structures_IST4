﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace Masyu {
    public partial class MasyuPanel : UserControl {

        // Граница доски.(%)
        static float BORDER_PERCENT = .125f;
        // Процент белого круга.
        static float WHITE_CIRCLE_OUTLINE_PERCENT = .066f;

        // Доска.
        MasyuBoard board;
        // Переменные "Высота, ширина доски".
        int boardWidth, boardHeight;
        // Переменные "Размер клетки, x-доска, y-доска".
        float cellSize, xBorder, yBorder;

        public MasyuForm form;

        // Обьявления значений доски.
        public MasyuPanel() {
            ResizeRedraw = true;
            InitializeComponent();

            // Начальные значения.
            boardWidth = 10;
            boardHeight = 10;
            board = new MasyuBoard(boardWidth, boardHeight);
        }

        // Функция реализации нажатия: (Отбражения кругов MouseButtons.Right - "White" ; MouseButtons.Left - "Black").
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseClick(e);
            int x = (int)Math.Floor((e.X - xBorder) / cellSize);
            int y = (int)Math.Floor((e.Y - yBorder) / cellSize);
            if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight) {
                return;
            }
            board.SetCircle(x, y, e.Button == MouseButtons.Right || ModifierKeys == Keys.Shift ? MasyuCircle.WHITE : MasyuCircle.BLACK);
            Solve();
        }

        public void Solve() {
            form.logBox.Clear();

            if (form.solveDepth == 0) {
             
                Invalidate();
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            board.Solve(Log, form.solveDepth - 2);

            stopwatch.Stop();
            form.UpdateValidity(board.GetValidity());
            Invalidate();
        }
        public void Log(string s) {
            if (form.logBox.TextLength > 0) {
                form.logBox.Text += "\r\n";
            }
            form.logBox.Text += s;
        }

        // Пункты меню. (Начать с новую доску)
        public void New(int boardWidth, int boardHeight) {
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            board = new MasyuBoard(boardWidth, boardHeight);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            // Рассчитать структуру.
            base.OnPaint(e);
            float horizonalCellSize = (float)Width / boardWidth;
            float verticalCellSize = (float)Height / boardHeight;
            cellSize = Math.Min(horizonalCellSize, verticalCellSize) * (1 - BORDER_PERCENT);
            xBorder = Width / 2 - boardWidth / 2f * cellSize;
            yBorder = Height / 2 - boardHeight / 2f * cellSize;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем сутку.
            Pen grayPen = new Pen(Color.LightGray);
            for (int x = 1; x < boardWidth; x++) {
                e.Graphics.DrawLine(grayPen, xBorder + x * cellSize, yBorder, xBorder + x * cellSize, Height - yBorder);
            }
            for (int y = 1; y < boardHeight; y++) {
                e.Graphics.DrawLine(grayPen, xBorder, yBorder + y * cellSize, Width - xBorder, yBorder + y * cellSize);
            }
            Pen blackPen = new Pen(Color.Black, Math.Max(1, cellSize / 6));
            blackPen.EndCap = LineCap.Round;
            blackPen.StartCap = LineCap.Round;
            e.Graphics.DrawRectangle(blackPen, xBorder - cellSize / 16, yBorder - cellSize / 16, Width - xBorder * 2 + cellSize / 8, Height - yBorder * 2 + cellSize / 8);

            // Рисуем круги.
            float circleRadius = cellSize * .33f;
            Brush blackCircleBrush = new SolidBrush(Color.FromArgb(20, 20, 20));
            float blackCircleRadius = circleRadius + cellSize / 20;
            Pen whiteCirclePen = new Pen(Color.FromArgb(20, 20, 20), cellSize * WHITE_CIRCLE_OUTLINE_PERCENT);
            for (int x = 0; x < boardWidth; x++) {
                for (int y = 0; y < boardHeight; y++) {
                    MasyuCircle circle = board.GetCircle(x, y);
                    if (circle == MasyuCircle.NONE) {
                        continue;
                    }
                    if (circle == MasyuCircle.BLACK) {
                        e.Graphics.FillEllipse(blackCircleBrush, xBorder + (x + .5f) * cellSize - blackCircleRadius, yBorder + (y + .5f) * cellSize - blackCircleRadius, 2 * blackCircleRadius, 2 * blackCircleRadius);
                    } else if (circle == MasyuCircle.WHITE) {
                        e.Graphics.DrawEllipse(whiteCirclePen, xBorder + (x + .5f) * cellSize - circleRadius, yBorder + (y + .5f) * cellSize - circleRadius, 2 * circleRadius, 2 * circleRadius);
                    }
                }
            }

            // Рисуем горизонтальные линии и X.
                        Pen xPen = new Pen(Color.FromArgb(0, 49, 83), Math.Max(1, cellSize / 20));
            for (int x = 0; x < boardWidth - 1; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (board.IsLine(x, y, true))
                    {
                        e.Graphics.DrawLine(blackPen, xBorder + (x + .5f) * cellSize, yBorder + (y + .5f) * cellSize, xBorder + (x + 1.5f) * cellSize, yBorder + (y + .5f) * cellSize);
                    } else if (board.IsX(x, y, true))
                    {
                        float px = xBorder + (x + 1) * cellSize;
                        float py = yBorder + (y + .5f) * cellSize;
                        float offset = cellSize * .066f;
                        e.Graphics.DrawLine(xPen, px - offset, py - offset, px + offset, py + offset);
                        e.Graphics.DrawLine(xPen, px + offset, py - offset, px - offset, py + offset);
                    }
                }
            }
            // Рисуем вертикальные линии.
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight - 1; y++)
                {
                    if (board.IsLine(x, y, false))
                    {
                        e.Graphics.DrawLine(blackPen, xBorder + (x + .5f) * cellSize, yBorder + (y + .5f) * cellSize, xBorder + (x + .5f) * cellSize, yBorder + (y + 1.5f) * cellSize);
                    }
                    else if (board.IsX(x, y, false))
                    {
                        float px = xBorder + (x + .5f) * cellSize;
                        float py = yBorder + (y + 1) * cellSize;
                        float offset = cellSize * .066f;
                        e.Graphics.DrawLine(xPen, px - offset, py - offset, px + offset, py + offset);
                        e.Graphics.DrawLine(xPen, px + offset, py - offset, px - offset, py + offset);
                    }
                }
            }

            // Рисуем ромбы.
            Brush inBrush = new SolidBrush(Color.FromArgb(227, 38, 54));
            Brush outBrush = new SolidBrush(Color.FromArgb(255, 36, 0));
            for (int x = 0; x < boardWidth - 1; x++) {
                for (int y = 0; y < boardHeight - 1; y++) {
                    MasyuInOut inOut = board.GetInOut(x, y);
                    if (inOut == MasyuInOut.UNKNOWN) {
                        continue;
                    }
                    float px = xBorder + (x + 1) * cellSize;
                    float py = yBorder + (y + 1) * cellSize;
                    float offset = cellSize * .2f;
                    PointF diamond1 = new PointF(px - offset, py);
                    PointF diamond2 = new PointF(px, py - offset);
                    PointF diamond3 = new PointF(px + offset, py);
                    PointF diamond4 = new PointF(px, py + offset);
                    e.Graphics.FillPolygon(inOut == MasyuInOut.IN ? inBrush : outBrush, new PointF[] { diamond1, diamond2, diamond3, diamond4 });
                }
            }
        }
    }
}
