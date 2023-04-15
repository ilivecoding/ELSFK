using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ELSFK
{
    public partial class Form1 : Form
    {
        private const int BlockSize = 10;
        private const int GameWidth = 5;
        private const int GameHeight = 10;
        private readonly Timer _timer;
        private readonly Random _random;
        private int[,] _gameBoard;
        private Point _currentBlock;
        private int _currentBlockType;
        private int _currentRotation;
        private int _score;
        public Form1()
        {
            InitializeComponent();

            Text = "ELSFK";
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            Width = BlockSize * 10;
            Height = BlockSize * 20;

            Paint += Form1_Paint;
            KeyDown += Form1_KeyDown;

            _timer = new Timer { Interval = 500 };
            _timer.Tick += Timer_Tick;
            _random = new Random();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            _gameBoard = new int[GameWidth, GameHeight];
            StartGame();
        }
        private void StartGame()
        {
            _score = 0;
            _timer.Start();
            NewBlock();
        }
        private void EndGame()
        {
            _timer.Stop();
            MessageBox.Show($"Game over! Your score is {_score}.");
            Refresh();
        }
        private void NewBlock()
        {
            _currentBlockType = _random.Next(7);
            _currentRotation = 0;
            _currentBlock = new Point(GameWidth / 2, 0);
            if (IsCollision())
            {
                EndGame();
            }
        }
        private bool IsCollision()
        {
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    if (Block.Blocks[_currentBlockType, _currentRotation, y, x] != 0)
                    {
                        var blockX = _currentBlock.X + x;
                        var blockY = _currentBlock.Y + y;
                        if (blockX < 0 || blockX >= GameWidth || blockY >= GameHeight ||
                            _gameBoard[blockX, blockY] != 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            _currentBlock.Y++;
            if (IsCollision())
            {
                _currentBlock.Y--;
                for (var x = 0; x < 4; x++)
                {
                    for (var y = 0; y < 4; y++)
                    {
                        if (Block.Blocks[_currentBlockType, _currentRotation, y, x] != 0)
                        {
                            var blockX = _currentBlock.X + x;
                            var blockY = _currentBlock.Y + y;
                            _gameBoard[blockX, blockY] = _currentBlockType + 1;
                        }
                    }
                }
                CheckLines();
                NewBlock();
            }
            Refresh();
        }
        private void CheckLines()
        {
            for (var y = GameHeight - 1; y >= 0; y--)
            {
                var complete = true;
                for (var x = 0; x < GameWidth; x++)
                {
                    if (_gameBoard[x, y] == 0)
                    {
                        complete = false;
                        break;
                    }
                }
                if (complete)
                {
                    _score++;
                    for (var y2 = y; y2 > 0; y2--)
                    {
                        for (var x = 0; x < GameWidth; x++)
                        {
                            _gameBoard[x, y2] = _gameBoard[x, y2 - 1];
                        }
                    }
                    for (var x = 0; x < GameWidth; x++)
                    {
                        _gameBoard[x, 0] = 0;
                    }
                    y++;
                }
            }
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            for (var x = 0; x < GameWidth; x++)
            {
                for (var y = 0; y < GameHeight; y++)
                {
                    if (_gameBoard[x, y] != 0)
                    {
                        var color = Block.BlockColors[_gameBoard[x, y] - 1];
                        var brush = new SolidBrush(color);
                        g.FillRectangle(brush, x * BlockSize, y * BlockSize, BlockSize, BlockSize);
                    }
                }
            }
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    if (Block.Blocks[_currentBlockType, _currentRotation, y, x] != 0)
                    {
                        var color = Block.BlockColors[_currentBlockType];
                        var brush = new SolidBrush(color);
                        g.FillRectangle(brush, (_currentBlock.X + x) * BlockSize, (_currentBlock.Y + y) * BlockSize,
                            BlockSize, BlockSize);
                    }
                }
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && CanMove(-1, 0))
            {
                _currentBlock.X--;
            }
            if (e.KeyCode == Keys.Right && CanMove(1, 0))
            {
                _currentBlock.X++;
            }
            if (e.KeyCode == Keys.Down && CanMove(0, 1))
            {
                _currentBlock.Y++;
            }
            if (e.KeyCode == Keys.Up)
            {
                var newRotation = (_currentRotation + 1) % 4;
                if (CanRotate(newRotation))
                {
                    _currentRotation = newRotation;
                }
            }
            if(e.KeyCode == Keys.Space)
            {
                _timer.Enabled = !_timer.Enabled;
            }
            if (e.KeyCode  == Keys.Escape)
            {
                _timer.Enabled = false;
                Close();
            }
        }
        private bool CanMove(int xOffset, int yOffset)
        {
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    if (Block.Blocks[_currentBlockType, _currentRotation, y, x] != 0)
                    {
                        var blockX = _currentBlock.X + x + xOffset;
                        var blockY = _currentBlock.Y + y + yOffset;
                        if (blockX < 0 || blockX >= GameWidth || blockY >= GameHeight ||
                            _gameBoard[blockX, blockY] != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool CanRotate(int newRotation)
        {
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    if (Block.Blocks[_currentBlockType, newRotation, y, x] != 0)
                    {
                        var blockX = _currentBlock.X + x;
                        var blockY = _currentBlock.Y + y;
                        if (blockX < 0 || blockX >= GameWidth || blockY >= GameHeight ||
                            _gameBoard[blockX, blockY] != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }

    public static class Block
    {
        public static readonly Color[] BlockColors = {
            Color.Red, 
            Color.Green, 
            Color.Blue, 
            Color.Yellow, 
            Color.Purple, 
            Color.Cyan, 
            Color.White, 
            Color.Black,
        };

        public static readonly int[,,,] Blocks =
        {
            {
                {
                    { 1, 1, 1, 1
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 1, 0, 0, 0
                    },
                    { 1, 0, 0, 0
                    },
                    { 1, 0, 0, 0
                    },
                    { 1, 0, 0, 0
                    }
                },
                {
                    { 1, 1, 1, 1
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 1, 0, 0, 0
                    },
                    { 1, 0, 0, 0
                    },
                    { 1, 0, 0, 0
                    },
                    { 1, 0, 0, 0
                    }
                }
            },
            {
                {
                    { 0, 2, 2, 0
                    },
                    { 0, 2, 2, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 2, 2, 0
                    },
                    { 0, 2, 2, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 2, 2, 0
                    },
                    { 0, 2, 2, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 2, 2, 0
                    },
                    { 0, 2, 2, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                }
            },
            {
                {
                    { 3, 3, 0, 0
                    },
                    { 0, 3, 3, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 0, 3, 0
                    },
                    { 0, 3, 3, 0
                    },
                    { 0, 3, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 3, 3, 0, 0
                    },
                    { 0, 3, 3, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 0, 3, 0
                    },
                    { 0, 3, 3, 0
                    },
                    { 0, 3, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                }
            },
            {
                {
                    { 0, 4, 0, 0
                    },
                    { 0, 4, 0, 0
                    },
                    { 0, 4, 4, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 0, 0, 0
                    },
                    { 0, 4, 4, 4
                    },
                    { 0, 0, 0, 4
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 4, 4, 0
                    },
                    { 0, 0, 4, 0
                    },
                    { 0, 0, 4, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 4, 0, 0, 0
                    },
                    { 4, 4, 4, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                }
            },
            {
                 {
                    {0, 5, 0, 0},
                    {0, 5, 0, 0},
                    {0, 5, 0, 0},
                    {0, 5, 0, 0}
                },
                {
                    {0, 0, 0, 0},
                    {5, 5, 5, 5},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0}
                },
                {
                    {0, 5, 0, 0},
                    {0, 5, 0, 0},
                    {0, 5, 0, 0},
                    {0, 5, 0, 0}
                },
                {
                    {0, 0, 0, 0},
                    {5, 5, 5, 5},
                    { 0, 0, 0, 0},
                    { 0, 0, 0, 0}
                }
            },
            {
                {
                    { 0, 6, 0, 0
                    },
                    { 6, 6, 6, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 6, 0, 0, 0
                    },
                    { 6, 6, 0, 0
                    },
                    { 6, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 6, 6, 6, 0
                    },
                    { 0, 6, 0, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 6, 0, 0
                    },
                    { 6, 6, 0, 0
                    },
                    { 0, 6, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                }
            },
            {
                {
                    { 7, 7, 0, 0
                    },
                    { 0, 7, 7, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 0, 7, 0
                    },
                    { 0, 7, 7, 0
                    },
                    { 0, 7, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 7, 0, 0
                    },
                    { 0, 7, 7, 0
                    },
                    { 0, 0, 7, 0
                    },
                    { 0, 0, 0, 0
                    }
                },
                {
                    { 0, 7, 7, 0
                    },
                    { 7, 7, 0, 0
                    },
                    { 0, 0, 0, 0
                    },
                    { 0, 0, 0, 0
                    }
                }
            }
        };
    }

}
