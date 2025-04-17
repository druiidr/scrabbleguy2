using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;

namespace scrabbleguy
{
    public class ScrabbleGUI : Form
    {
        private ScrabbleBoard gameBoard;
        private Player player1;
        private Player player2;
        private TileBag tileBag;
        private ScrabbleDictionary dictionary;
        private bool isPlayer2AI;
        private bool isPlayer1Turn = true;

        // GUI Elements
        private Panel boardPanel;
        private Panel player1RackPanel;
        private Panel player2RackPanel;
        private Label player1ScoreLabel;
        private Label player2ScoreLabel;
        private Button playButton;
        private Button exchangeButton;
        private Label statusLabel;
        private Button endTurnButton;

        // Board state tracking
        private Button[,] boardButtons;
        private List<Button> player1RackButtons = new List<Button>();
        private List<Button> player2RackButtons = new List<Button>();
        private List<Tile> selectedTiles = new List<Tile>();
        private List<Point> selectedPositions = new List<Point>();
        private bool isHorizontalPlacement = true;
        private int startRow = -1;
        private int startCol = -1;

        // Board Colors
        private Color normalSquareColor = Color.LightGray;
        private Color doubleLetter = Color.LightBlue;
        private Color tripleLetter = Color.Blue;
        private Color doubleWord = Color.Pink;
        private Color tripleWord = Color.Red;
        private Color centerSquare = Color.Gold;

        // Font and sizes
        private Font tileFont = new Font("Arial", 12, FontStyle.Bold);
        private int boardSquareSize = 40;
        private int tileSize = 35;

        public ScrabbleGUI(bool playAgainstAI)
        {
            isPlayer2AI = playAgainstAI;
            InitializeGame();
            InitializeGUI();
        }

        private void InitializeGame()
        {
            gameBoard = new ScrabbleBoard();
            tileBag = new TileBag();

            // Update this path to your dictionary file
            dictionary = new ScrabbleDictionary("C:\\Users\\ASUS\\source\\repos\\scrabbleguy2\\scrabbleguy2\\fullScrabbleLegalDictionary.txt");
            player1 = new Player("Player 1");
            if (isPlayer2AI)
                player2 = new AIPlayer("Player 2", dictionary);
            else
                player2 = new Player("Player 2");

            // Draw initial tiles
            for (int i = 0; i < 7; i++)
            {
                player1.DrawTile(tileBag);
                player2.DrawTile(tileBag);
            }
        }

        private void InitializeGUI()
        {
            // Form settings
            this.Text = "Scrabble Game";
            this.Size = new Size(1000, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Create the board panel
            boardPanel = new Panel
            {
                Size = new Size(boardSquareSize * 15 + 2, boardSquareSize * 15 + 2),
                Location = new Point(50, 50),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Create player rack panels
            player1RackPanel = new Panel
            {
                Size = new Size(tileSize * 7 + 14, tileSize + 10),
                Location = new Point(50, 50 + boardPanel.Height + 20),
                BorderStyle = BorderStyle.FixedSingle
            };

            player2RackPanel = new Panel
            {
                Size = new Size(tileSize * 7 + 14, tileSize + 10),
                Location = new Point(boardPanel.Right - player1RackPanel.Width, 50 + boardPanel.Height + 20),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = !isPlayer2AI // Hide Player 2's rack if Player 2 is AI
            };

            // Create score labels
            player1ScoreLabel = new Label
            {
                Text = "Player 1: 0",
                Location = new Point(50, 20),
                AutoSize = true,
                Font = new Font("Arial", 12)
            };

            player2ScoreLabel = new Label
            {
                Text = "Player 2: 0",
                Location = new Point(boardPanel.Right - 100, 20),
                AutoSize = true,
                Font = new Font("Arial", 12)
            };

            // Create control buttons
            playButton = new Button
            {
                Text = "Play Word",
                Location = new Point(50, player1RackPanel.Bottom + 20),
                Size = new Size(100, 30),
                Enabled = false
            };
            playButton.Click += PlayButton_Click;

            exchangeButton = new Button
            {
                Text = "Exchange Tiles",
                Location = new Point(playButton.Right + 20, player1RackPanel.Bottom + 20),
                Size = new Size(100, 30)
            };
            exchangeButton.Click += ExchangeButton_Click;

            endTurnButton = new Button
            {
                Text = "End Turn",
                Location = new Point(exchangeButton.Right + 20, player1RackPanel.Bottom + 20),
                Size = new Size(100, 30),
                Enabled = false
            };
            endTurnButton.Click += EndTurnButton_Click;

            // Status label
            statusLabel = new Label
            {
                Text = "Player 1's turn",
                Location = new Point(50, playButton.Bottom + 20),
                AutoSize = true,
                Font = new Font("Arial", 12)
            };

            // Create board buttons
            CreateBoardButtons();

            // Create rack buttons
            CreateRackButtons();

            // Add controls to form
            this.Controls.Add(boardPanel);
            this.Controls.Add(player1RackPanel);
            this.Controls.Add(player2RackPanel);
            this.Controls.Add(player1ScoreLabel);
            this.Controls.Add(player2ScoreLabel);
            this.Controls.Add(playButton);
            this.Controls.Add(exchangeButton);
            this.Controls.Add(endTurnButton);
            this.Controls.Add(statusLabel);

            // Initialize toggle button for orientation
            Button orientationButton = new Button
            {
                Text = "Horizontal",
                Location = new Point(endTurnButton.Right + 20, player1RackPanel.Bottom + 20),
                Size = new Size(100, 30),
                Tag = true
            };
            orientationButton.Click += (s, e) =>
            {
                isHorizontalPlacement = !isHorizontalPlacement;
                orientationButton.Text = isHorizontalPlacement ? "Horizontal" : "Vertical";
                orientationButton.Tag = isHorizontalPlacement;
            };
            this.Controls.Add(orientationButton);

            // Update displays
            UpdateBoardDisplay();
            UpdateRackDisplay();
        }

        private void CreateBoardButtons()
        {
            boardButtons = new Button[15, 15];

            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(boardSquareSize, boardSquareSize),
                        Location = new Point(col * boardSquareSize, row * boardSquareSize),
                        Tag = new Point(row, col),
                        Font = tileFont,
                        BackColor = GetSquareColor(row, col),
                        FlatStyle = FlatStyle.Flat,
                        Margin = new Padding(0)
                    };

                    btn.Click += BoardSquare_Click;
                    boardPanel.Controls.Add(btn);
                    boardButtons[row, col] = btn;
                }
            }
        }

        private Color GetSquareColor(int row, int col)
        {
            // Center square
            if (row == 7 && col == 7)
                return centerSquare;

            // Triple word scores
            if ((row == 0 || row == 14) && (col == 0 || col == 7 || col == 14) ||
                (row == 7 && (col == 0 || col == 14)))
                return tripleWord;

            // Double word scores
            if (row == col || row == 14 - col)
                if (row != 0 && row != 14 && row != 7)
                    return doubleWord;

            // Triple letter scores
            if ((row == 1 || row == 13) && (col == 5 || col == 9) ||
                (row == 5 || row == 9) && (col == 1 || col == 5 || col == 9 || col == 13))
                return tripleLetter;

            // Double letter scores
            if ((row == 0 || row == 14) && (col == 3 || col == 11) ||
                (row == 2 || row == 12) && (col == 6 || col == 8) ||
                (row == 3 || row == 11) && (col == 0 || col == 7 || col == 14) ||
                (row == 6 || row == 8) && (col == 2 || col == 6 || col == 8 || col == 12) ||
                (row == 7) && (col == 3 || col == 11))
                return doubleLetter;

            return normalSquareColor;
        }

        private void CreateRackButtons()
        {
            // Player 1's rack
            for (int i = 0; i < 7; i++)
            {
                Button btn = new Button
                {
                    Size = new Size(tileSize, tileSize),
                    Location = new Point(5 + i * (tileSize + 2), 5),
                    Font = tileFont,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.BurlyWood,
                    Tag = i  // Store the rack index
                };
                btn.Click += Player1Tile_Click;
                player1RackPanel.Controls.Add(btn);
                player1RackButtons.Add(btn);
            }

            // Player 2's rack
            for (int i = 0; i < 7; i++)
            {
                Button btn = new Button
                {
                    Size = new Size(tileSize, tileSize),
                    Location = new Point(5 + i * (tileSize + 2), 5),
                    Font = tileFont,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.BurlyWood,
                    Tag = i  // Store the rack index
                };
                btn.Click += Player2Tile_Click;
                player2RackPanel.Controls.Add(btn);
                player2RackButtons.Add(btn);
            }
        }

        private void UpdateBoardDisplay()
        {
            Tile[,] board = gameBoard.GetBoard();

            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    Button btn = boardButtons[row, col];
                    Tile tile = board[row, col];

                    if (tile != null)
                    {
                        btn.Text = tile.Letter.ToString();
                        btn.BackColor = Color.BurlyWood;
                        btn.Enabled = false;
                    }
                    else
                    {
                        btn.Text = "";
                        btn.BackColor = GetSquareColor(row, col);
                        btn.Enabled = true;
                    }
                }
            }
        }

        private void UpdateRackDisplay()
        {
            // Show only the current player's rack
            player1RackPanel.Visible = isPlayer1Turn;
            player2RackPanel.Visible = !isPlayer1Turn && !isPlayer2AI;

            // Update Player 1's rack
            for (int i = 0; i < player1RackButtons.Count; i++)
            {
                Button btn = player1RackButtons[i];
                if (i < player1.Rack.Count)
                {
                    Tile tile = player1.Rack[i];
                    btn.Text = $"{tile.Letter}\n{tile.Score}";
                    btn.Enabled = isPlayer1Turn;
                    btn.Visible = true;
                }
                else
                {
                    btn.Text = "";
                    btn.Enabled = false;
                    btn.Visible = false;
                }
            }

            // Update Player 2's rack (only if not AI)
            if (!isPlayer2AI)
            {
                for (int i = 0; i < player2RackButtons.Count; i++)
                {
                    Button btn = player2RackButtons[i];
                    if (i < player2.Rack.Count)
                    {
                        Tile tile = player2.Rack[i];
                        btn.Text = $"{tile.Letter}\n{tile.Score}";
                        btn.Enabled = !isPlayer1Turn;
                        btn.Visible = true;
                    }
                    else
                    {
                        btn.Text = "";
                        btn.Enabled = false;
                        btn.Visible = false;
                    }
                }
            }

            // Update score labels
            player1ScoreLabel.Text = $"Player 1: {player1.score}";
            player2ScoreLabel.Text = $"Player 2: {player2.score}";
        }

        private void Player1Tile_Click(object sender, EventArgs e)
        {
            if (!isPlayer1Turn) return;

            Button btn = sender as Button;
            int index = (int)btn.Tag;

            if (index < player1.Rack.Count)
            {
                Tile selectedTile = player1.Rack[index];

                if (selectedTiles.Contains(selectedTile))
                {
                    // Deselect the tile
                    selectedTiles.Remove(selectedTile);
                    btn.BackColor = Color.BurlyWood;
                    btn.Enabled = true;
                    statusLabel.Text = "Tile deselected.";
                }
                else
                {
                    // Add to selected tiles
                    selectedTiles.Add(selectedTile);

                    // Visually mark as selected
                    btn.BackColor = Color.LightGreen;
                    btn.Enabled = false;

                    statusLabel.Text = $"Selected tile: {selectedTile.Letter}";
                }
            }
        }

        private void Player2Tile_Click(object sender, EventArgs e)
        {
            if (isPlayer1Turn) return;

            Button btn = sender as Button;
            int index = (int)btn.Tag;

            if (index < player2.Rack.Count)
            {
                Tile selectedTile = player2.Rack[index];

                if (selectedTiles.Contains(selectedTile))
                {
                    // Deselect the tile
                    selectedTiles.Remove(selectedTile);
                    btn.BackColor = Color.BurlyWood;
                    btn.Enabled = true;
                    statusLabel.Text = "Tile deselected.";
                }
                else
                {
                    // Add to selected tiles
                    selectedTiles.Add(selectedTile);

                    // Visually mark as selected
                    btn.BackColor = Color.LightGreen;
                    btn.Enabled = false;

                    statusLabel.Text = $"Selected tile: {selectedTile.Letter}";
                }
            }
        }

        private void BoardSquare_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Point position = (Point)btn.Tag;
            int row = position.X;
            int col = position.Y;

            if (selectedTiles.Count > 0)
            {
                Tile tile = selectedTiles[selectedTiles.Count - 1];
                selectedTiles.RemoveAt(selectedTiles.Count - 1);

                // Remember the selected position
                selectedPositions.Add(new Point(row, col));

                // Set the start position if this is the first tile
                if (startRow == -1 && startCol == -1)
                {
                    startRow = row;
                    startCol = col;

                    // Highlight the starting square
                    btn.FlatAppearance.BorderSize = 2;
                    btn.FlatAppearance.BorderColor = Color.Green;
                }

                // Mark the position on the board temporarily
                btn.Text = tile.Letter.ToString();
                btn.BackColor = Color.LightYellow;
                btn.Tag = new object[] { position, tile };

                statusLabel.Text = $"Placing {tile.Letter} at ({row}, {col})";

                // Enable play button if we have placed at least one tile
                playButton.Enabled = selectedPositions.Count > 0;
            }
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (selectedPositions.Count == 0) return;

            // Gather all tiles placed on the board
            List<Tile> wordTiles = new List<Tile>();
            foreach (Button btn in boardPanel.Controls)
            {
                if (btn.BackColor == Color.LightYellow)
                {
                    var tagArray = btn.Tag as object[];
                    if (tagArray != null && tagArray.Length == 2)
                    {
                        wordTiles.Add((Tile)tagArray[1]);
                    }
                }
            }

            Player currentPlayer = isPlayer1Turn ? player1 : player2;

            // Try to place the word
            try
            {
                if (gameBoard.CanPlaceWord(wordTiles, startRow, startCol, isHorizontalPlacement, currentPlayer))
                {
                    // Place the word permanently
                    gameBoard.PlaceWord(wordTiles, startRow, startCol, isHorizontalPlacement);

                    // Remove the played tiles from the rack
                    foreach (var tile in wordTiles)
                    {
                        currentPlayer.RemoveTileFromRack(tile);
                    }

                    // Update the score (already handled by CanPlaceWord)
                    statusLabel.Text = $"Word placed! Score: {currentPlayer.score}";

                    // Refill the rack
                    currentPlayer.RefillRack(tileBag);

                    // Switch turns
                    SwitchTurns();

                    // Update displays
                    UpdateBoardDisplay();
                    UpdateRackDisplay();
                }
                else
                {
                    statusLabel.Text = "Invalid word placement. Try again.";

                    // Return tiles to rack
                    foreach (var tile in wordTiles)
                    {
                        if (currentPlayer.Rack.All(t => t.Letter != tile.Letter))
                        {
                            currentPlayer.Rack.Add(tile);
                        }
                    }

                    // Reset the board
                    ResetBoardSelections();
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                ResetBoardSelections();
            }

            // Clear selections
            selectedTiles.Clear();
            selectedPositions.Clear();
            startRow = -1;
            startCol = -1;
            playButton.Enabled = false;
        }

        private void ExchangeButton_Click(object sender, EventArgs e)
        {
            if (tileBag.IsEmpty())
            {
                statusLabel.Text = "Tile bag is empty. Cannot exchange tiles.";
                return;
            }

            // Enable selection for exchange
            statusLabel.Text = "Select tiles to exchange, then press End Turn";
            endTurnButton.Enabled = true;
            playButton.Enabled = false;
            exchangeButton.Enabled = false;
        }

        private void EndTurnButton_Click(object sender, EventArgs e)
        {
            Player currentPlayer = isPlayer1Turn ? player1 : player2;

            if (selectedTiles.Count > 0)
            {
                // Return selected tiles to bag
                foreach (var tile in selectedTiles)
                {
                    tileBag.AddTiles(tile.Letter, 1, tile.Score);
                    currentPlayer.RemoveTileFromRack(tile);
                }

                // Draw new tiles
                currentPlayer.RefillRack(tileBag);
                statusLabel.Text = $"Exchanged {selectedTiles.Count} tiles";
                selectedTiles.Clear();
            }

            // Switch turns
            SwitchTurns();

            // Reset board and update displays
            ResetBoardSelections();
            UpdateRackDisplay();

            // Reset buttons
            endTurnButton.Enabled = false;
            exchangeButton.Enabled = true;
        }

        private void ResetBoardSelections()
        {
            foreach (Button btn in boardPanel.Controls)
            {
                if (btn.BackColor == Color.LightYellow)
                {
                    Point position = (Point)((object[])btn.Tag)[0];
                    btn.BackColor = GetSquareColor(position.X, position.Y);
                    btn.Text = "";
                    btn.Tag = position;
                }
            }

            // Reset rack selections
            foreach (Button btn in player1RackPanel.Controls)
            {
                btn.BackColor = Color.BurlyWood;
                btn.Enabled = isPlayer1Turn;
            }

            foreach (Button btn in player2RackPanel.Controls)
            {
                btn.BackColor = Color.BurlyWood;
                btn.Enabled = !isPlayer1Turn;
            }
        }

        private void SwitchTurns()
        {
            isPlayer1Turn = !isPlayer1Turn;
            statusLabel.Text = isPlayer1Turn ? "Player 1's turn" : "Player 2's turn";

            // If it's the AI's turn, do the AI move
            if (!isPlayer1Turn && isPlayer2AI)
            {
                PlayAITurn();
            }
        }

        private void PlayAITurn()
        {
            AIPlayer aiPlayer = player2 as AIPlayer;
            aiPlayer.ExecuteBestMove(gameBoard, tileBag);
            UpdateBoardDisplay();
            UpdateRackDisplay();

            // Switch back to player 1
            SwitchTurns();
        }
    }
}