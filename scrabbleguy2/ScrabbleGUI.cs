using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using System.Windows.Forms;

namespace scrabbleguy
{
    public partial class ScrabbleGUI : Form
    {
        private ScrabbleBoard board;
        private TileBag tileBag;
        private ScrabbleDictionary scrabbleDictionary;
        private Player humanPlayer;
        private List<Point> lastPlayedWordPositions = new List<Point>();
        private List<Point> aiLastPlayedWordPositions = new List<Point>();
        private AIPlayer aiPlayer;
        private bool isAITurn = false;
        private bool isHorizontal = true; // Default orientation
        private bool isGameOver = false;
        private bool isFirstMove = true;
        private Panel buttonPanel;

        // Game constants
        private const int BoardSize = 15;
        private const int CellSize = 40;
        private const int RackTileSize = 50;

        // UI Controls
        private Panel boardPanel;
        private Panel rackPanel;
        private Panel gameInfoPanel;
        private Button submitMoveButton;
        private Button exchangeTilesButton;
        private Button passTurnButton;
        private Button toggleOrientationButton;
        private Button clearSelectionsButton;
        private Button helpButton;
        private TextBox gameLogTextBox;
        private Label player1ScoreLabel;
        private Label player2ScoreLabel;
        private Label currentTurnLabel;
        private Label remainingTilesLabel;
        private Label wordScorePreviewLabel;

        // Selected tiles tracking
        private List<Button> rackButtons = new List<Button>();
        private List<Tile> selectedTiles = new List<Tile>();
        private List<Point> selectedCells = new List<Point>();
        private Dictionary<Point, Tile> selectedCellTileMap = new Dictionary<Point, Tile>();

        // Board cell buttons
        private Button[,] boardButtons = new Button[BoardSize, BoardSize];

        public ScrabbleGUI()
        {
            InitializeComponent();
            SetupGame();
            CreateGameUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Scrabble Game";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void SetupGame()
        {
            try
            {
                // Initialize game components
                board = new ScrabbleBoard();
                tileBag = new TileBag();

                // Try to find dictionary file in several locations
                string dictionaryPath = "C:\\Users\\ASUS\\source\\repos\\scrabbleguy2\\scrabbleguy2\\fullScrabbleLegalDictionary.txt";
                if (string.IsNullOrEmpty(dictionaryPath))
                {
                    MessageBox.Show("Dictionary file not found. Please select the dictionary file.",
                                   "Dictionary Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                        openFileDialog.Title = "Select Scrabble Dictionary File";

                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            dictionaryPath = openFileDialog.FileName;
                        }
                        else
                        {
                            MessageBox.Show("Cannot continue without dictionary file.",
                                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                            return;
                        }
                    }
                }

                scrabbleDictionary = new ScrabbleDictionary(dictionaryPath);

                // Create players
                humanPlayer = new Player("Player 1");
                aiPlayer = new AIPlayer("AI Player", scrabbleDictionary);

                // Draw initial tiles
                for (int i = 0; i < 7; i++)
                {
                    humanPlayer.DrawTile(tileBag);
                    aiPlayer.DrawTile(tileBag);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up game: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private string FindDictionaryFile()
        {
            // Try multiple possible locations for the dictionary file
            string[] possiblePaths = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fullScrabbleLegalDictionary.txt"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dictionary.txt"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ScrabbleDictionary.txt")
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private void CreateGameUI()
        {
            // Main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 2,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 70),
                    new RowStyle(SizeType.Percent, 15),
                    new RowStyle(SizeType.Percent, 15)
                },
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 75),
                    new ColumnStyle(SizeType.Percent, 25)
                }
            };

            // Create board panel
            boardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.LightGray
            };

            // Create rack panel
            rackPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Tan
            };

            // Create game info panel
            gameInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };

            // Setup game log
            gameLogTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 9),
                BackColor = Color.White
            };

            // Add components to layout
            mainLayout.Controls.Add(boardPanel, 0, 0);
            mainLayout.Controls.Add(gameInfoPanel, 1, 0);
            mainLayout.Controls.Add(rackPanel, 0, 1);
            mainLayout.Controls.Add(gameLogTextBox, 1, 1);
            mainLayout.SetRowSpan(gameLogTextBox, 2);

            // Button panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };
            mainLayout.Controls.Add(buttonPanel, 0, 2);

            // Game info controls
            int yPos = 10;

            currentTurnLabel = new Label
            {
                Text = "Current Turn: Player 1",
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, yPos),
                ForeColor = Color.DarkGreen
            };
            gameInfoPanel.Controls.Add(currentTurnLabel);
            yPos += 40;

            player1ScoreLabel = new Label
            {
                Text = "Player 1: 0 points",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, yPos)
            };
            gameInfoPanel.Controls.Add(player1ScoreLabel);
            yPos += 30;

            player2ScoreLabel = new Label
            {
                Text = "AI Player: 0 points",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, yPos)
            };
            gameInfoPanel.Controls.Add(player2ScoreLabel);
            yPos += 40;

            remainingTilesLabel = new Label
            {
                Text = $"Tiles in bag: {tileBag.GetRemainingTilesCount()}",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(10, yPos)
            };
            gameInfoPanel.Controls.Add(remainingTilesLabel);
            yPos += 30;

            wordScorePreviewLabel = new Label
            {
                Text = "Word Score: 0",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(10, yPos)
            };
            gameInfoPanel.Controls.Add(wordScorePreviewLabel);

            // Game buttons
            int xPos = 10;
            submitMoveButton = new Button
            {
                Text = "Submit Move",
                Size = new Size(120, 40),
                Location = new Point(xPos, 10),
                BackColor = Color.LightGreen
            };
            submitMoveButton.Click += SubmitMove_Click;
            buttonPanel.Controls.Add(submitMoveButton);
            xPos += 130;

            exchangeTilesButton = new Button
            {
                Text = "Exchange Tiles",
                Size = new Size(120, 40),
                Location = new Point(xPos, 10),
                BackColor = Color.LightCoral
            };
            exchangeTilesButton.Click += ExchangeTiles_Click;
            buttonPanel.Controls.Add(exchangeTilesButton);
            xPos += 130;

            passTurnButton = new Button
            {
                Text = "Pass Turn",
                Size = new Size(120, 40),
                Location = new Point(xPos, 10),
                BackColor = Color.LightGray
            };
            passTurnButton.Click += PassTurn_Click;
            buttonPanel.Controls.Add(passTurnButton);
            xPos += 130;

            toggleOrientationButton = new Button
            {
                Text = "Horizontal",
                Size = new Size(120, 40),
                Location = new Point(xPos, 10),
                BackColor = Color.LightBlue
            };
            toggleOrientationButton.Click += ToggleOrientation_Click;
            buttonPanel.Controls.Add(toggleOrientationButton);

            // Second row of buttons
            xPos = 10;
            clearSelectionsButton = new Button
            {
                Text = "Clear Selections",
                Size = new Size(120, 40),
                Location = new Point(xPos, 60),
                BackColor = Color.LightYellow
            };
            clearSelectionsButton.Click += ClearSelections_Click;
            buttonPanel.Controls.Add(clearSelectionsButton);
            xPos += 130;

            helpButton = new Button
            {
                Text = "Help",
                Size = new Size(120, 40),
                Location = new Point(xPos, 60),
                BackColor = Color.LightBlue
            };
            helpButton.Click += Help_Click;
            buttonPanel.Controls.Add(helpButton);

            // Add layout to form
            this.Controls.Add(mainLayout);

            // Create board cells and rack
            CreateBoardCells();
            UpdateRackDisplay();

            // Initial log message
            AddToGameLog("Game started. Player 1's turn.");
        }

        private void CreateBoardCells()
        {
            boardPanel.Controls.Clear();
            TableLayoutPanel boardLayout = new TableLayoutPanel
            {
                RowCount = BoardSize,
                ColumnCount = BoardSize,
                Dock = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for (int row = 0; row < BoardSize; row++)
            {
                boardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / BoardSize));
                for (int col = 0; col < BoardSize; col++)
                {
                    if (row == 0) boardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / BoardSize));

                    Button cellButton = new Button
                    {
                        Dock = DockStyle.Fill,
                        Tag = new Point(row, col),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = GetCellColor(row, col),
                        Text = GetCellIndicator(row, col),
                        Font = new Font("Arial", 8),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.Black
                    };

                    // Add tooltip for bonus explanation
                    ToolTip cellToolTip = new ToolTip();
                    cellToolTip.SetToolTip(cellButton, GetCellTooltip(row, col));

                    // Mark center cell with star
                    if (row == 7 && col == 7)
                    {
                        cellButton.Text = "★\n" + cellButton.Text;
                        cellButton.BackColor = Color.Gold;
                    }

                    cellButton.Click += BoardCell_Click;
                    boardLayout.Controls.Add(cellButton, col, row);
                    boardButtons[row, col] = cellButton;
                }
            }

            boardPanel.Controls.Add(boardLayout);
        }

        private Color GetCellColor(int row, int col)
        {
            // Triple Word
            if ((row == 0 && col == 0) || (row == 0 && col == 7) || (row == 0 && col == 14) ||
                (row == 7 && col == 0) || (row == 7 && col == 14) ||
                (row == 14 && col == 0) || (row == 14 && col == 7) || (row == 14 && col == 14))
                return Color.Red;

            // Double Word
            if ((row == 1 && col == 1) || (row == 2 && col == 2) || (row == 3 && col == 3) || (row == 4 && col == 4) ||
                (row == 10 && col == 10) || (row == 11 && col == 11) || (row == 12 && col == 12) || (row == 13 && col == 13) ||
                (row == 1 && col == 13) || (row == 2 && col == 12) || (row == 3 && col == 11) || (row == 4 && col == 10) ||
                (row == 10 && col == 4) || (row == 11 && col == 3) || (row == 12 && col == 2) || (row == 13 && col == 1))
                return Color.Pink;

            // Triple Letter
            if ((row == 1 && col == 5) || (row == 1 && col == 9) || (row == 5 && col == 1) || (row == 5 && col == 5) ||
                (row == 5 && col == 9) || (row == 5 && col == 13) || (row == 9 && col == 1) || (row == 9 && col == 5) ||
                (row == 9 && col == 9) || (row == 9 && col == 13) || (row == 13 && col == 5) || (row == 13 && col == 9))
                return Color.Green;

            // Double Letter
            if ((row == 0 && col == 3) || (row == 0 && col == 11) || (row == 2 && col == 6) || (row == 2 && col == 8) ||
                (row == 3 && col == 0) || (row == 3 && col == 7) || (row == 3 && col == 14) || (row == 6 && col == 2) ||
                (row == 6 && col == 6) || (row == 6 && col == 8) || (row == 6 && col == 12) || (row == 7 && col == 3) ||
                (row == 7 && col == 11) || (row == 8 && col == 2) || (row == 8 && col == 6) || (row == 8 && col == 8) ||
                (row == 8 && col == 12) || (row == 11 && col == 0) || (row == 11 && col == 7) || (row == 11 && col == 14) ||
                (row == 12 && col == 6) || (row == 12 && col == 8) || (row == 14 && col == 3) || (row == 14 && col == 11))
                return Color.LightGreen;

            // Center square
            if (row == 7 && col == 7)
                return Color.Gold;

            // Default
            return Color.Beige;
        }

        private string GetCellIndicator(int row, int col)
        {
            // Triple Word
            if ((row == 0 && col == 0) || (row == 0 && col == 7) || (row == 0 && col == 14) ||
                (row == 7 && col == 0) || (row == 7 && col == 14) ||
                (row == 14 && col == 0) || (row == 14 && col == 7) || (row == 14 && col == 14))
                return "TW";

            // Double Word
            if ((row == 1 && col == 1) || (row == 2 && col == 2) || (row == 3 && col == 3) || (row == 4 && col == 4) ||
                (row == 10 && col == 10) || (row == 11 && col == 11) || (row == 12 && col == 12) || (row == 13 && col == 13) ||
                (row == 1 && col == 13) || (row == 2 && col == 12) || (row == 3 && col == 11) || (row == 4 && col == 10) ||
                (row == 10 && col == 4) || (row == 11 && col == 3) || (row == 12 && col == 2) || (row == 13 && col == 1))
                return "DW";

            // Triple Letter
            if ((row == 1 && col == 5) || (row == 1 && col == 9) || (row == 5 && col == 1) || (row == 5 && col == 5) ||
                (row == 5 && col == 9) || (row == 5 && col == 13) || (row == 9 && col == 1) || (row == 9 && col == 5) ||
                (row == 9 && col == 9) || (row == 9 && col == 13) || (row == 13 && col == 5) || (row == 13 && col == 9))
                return "TL";

            // Double Letter
            if ((row == 0 && col == 3) || (row == 0 && col == 11) || (row == 2 && col == 6) || (row == 2 && col == 8) ||
                (row == 3 && col == 0) || (row == 3 && col == 7) || (row == 3 && col == 14) || (row == 6 && col == 2) ||
                (row == 6 && col == 6) || (row == 6 && col == 8) || (row == 6 && col == 12) || (row == 7 && col == 3) ||
                (row == 7 && col == 11) || (row == 8 && col == 2) || (row == 8 && col == 6) || (row == 8 && col == 8) ||
                (row == 8 && col == 12) || (row == 11 && col == 0) || (row == 11 && col == 7) || (row == 11 && col == 14) ||
                (row == 12 && col == 6) || (row == 12 && col == 8) || (row == 14 && col == 3) || (row == 14 && col == 11))
                return "DL";

            // Default
            return string.Empty;
        }

        private string GetCellTooltip(int row, int col)
        {
            // Triple Word
            if ((row == 0 && col == 0) || (row == 0 && col == 7) || (row == 0 && col == 14) ||
                (row == 7 && col == 0) || (row == 7 && col == 14) ||
                (row == 14 && col == 0) || (row == 14 && col == 7) || (row == 14 && col == 14))
                return "Triple Word Score";

            // Double Word
            if ((row == 1 && col == 1) || (row == 2 && col == 2) || (row == 3 && col == 3) || (row == 4 && col == 4) ||
                (row == 10 && col == 10) || (row == 11 && col == 11) || (row == 12 && col == 12) || (row == 13 && col == 13) ||
                (row == 1 && col == 13) || (row == 2 && col == 12) || (row == 3 && col == 11) || (row == 4 && col == 10) ||
                (row == 10 && col == 4) || (row == 11 && col == 3) || (row == 12 && col == 2) || (row == 13 && col == 1))
                return "Double Word Score";

            // Triple Letter
            if ((row == 1 && col == 5) || (row == 1 && col == 9) || (row == 5 && col == 1) || (row == 5 && col == 5) ||
                (row == 5 && col == 9) || (row == 5 && col == 13) || (row == 9 && col == 1) || (row == 9 && col == 5) ||
                (row == 9 && col == 9) || (row == 9 && col == 13) || (row == 13 && col == 5) || (row == 13 && col == 9))
                return "Triple Letter Score";

            // Double Letter
            if ((row == 0 && col == 3) || (row == 0 && col == 11) || (row == 2 && col == 6) || (row == 2 && col == 8) ||
                (row == 3 && col == 0) || (row == 3 && col == 7) || (row == 3 && col == 14) || (row == 6 && col == 2) ||
                (row == 6 && col == 6) || (row == 6 && col == 8) || (row == 6 && col == 12) || (row == 7 && col == 3) ||
                (row == 7 && col == 11) || (row == 8 && col == 2) || (row == 8 && col == 6) || (row == 8 && col == 8) ||
                (row == 8 && col == 12) || (row == 11 && col == 0) || (row == 11 && col == 7) || (row == 11 && col == 14) ||
                (row == 12 && col == 6) || (row == 12 && col == 8) || (row == 14 && col == 3) || (row == 14 && col == 11))
                return "Double Letter Score";

            // Center square
            if (row == 7 && col == 7)
                return "Center Square - First word must cover this square";

            // Default
            return "Regular square";
        }

        private void BoardCell_Click(object sender, EventArgs e)
        {
            if (isGameOver || isAITurn)
                return;

            Button clickedCell = sender as Button;
            Point cellPosition = (Point)clickedCell.Tag;

            // Check if the cell is already occupied
            Tile existingTile = board.GetTileAt(cellPosition.X, cellPosition.Y);
            if (existingTile != null)
            {
                // If the cell is already selected, deselect it
                if (selectedCells.Contains(cellPosition))
                {
                    selectedCells.Remove(cellPosition);
                    selectedCellTileMap.Remove(cellPosition);
                    clickedCell.BackColor = GetCellColor(cellPosition.X, cellPosition.Y);

                    // Restore the tile text instead of removing it
                    clickedCell.Text = existingTile.Letter.ToString();
                    clickedCell.Font = new Font("Arial", 12, FontStyle.Bold);

                    UpdateWordScorePreview();
                    return;
                }

                // Add the existing tile to the selection
                if (selectedCells.Count > 0 && !IsValidCellSelection(cellPosition))
                {
                    AddToGameLog("Invalid selection. Tiles must be placed in a continuous line.");
                    return;
                }

                selectedCells.Add(cellPosition);
                selectedCellTileMap[cellPosition] = existingTile;
                clickedCell.BackColor = Color.Yellow;

                UpdateWordScorePreview();
                return;
            }

            // Handle selecting an empty cell
            if (selectedTiles.Count < humanPlayer.Rack.Count)
            {
                if (selectedCells.Count > 0 && !IsValidCellSelection(cellPosition))
                {
                    AddToGameLog("Invalid selection. Tiles must be placed in a continuous line.");
                    return;
                }

                selectedCells.Add(cellPosition);
                clickedCell.BackColor = Color.Yellow;

                UpdateWordScorePreview();
            }
            else
            {
                AddToGameLog("Select a tile from your rack before selecting a cell.");
            }
        }

        private bool IsValidCellSelection(Point newCell)
        {
            // First selection is always valid
            if (selectedCells.Count == 0)
                return true;

            // Check if selection maintains a line
            if (selectedCells.Count == 1)
            {
                // Second selection establishes the orientation
                Point firstCell = selectedCells[0];
                isHorizontal = firstCell.X == newCell.X;
                toggleOrientationButton.Text = isHorizontal ? "Horizontal" : "Vertical";
                return true; // Any second placement is valid as it establishes direction
            }
            else
            {
                // Check if placement continues the line in the established orientation
                if (isHorizontal)
                {
                    // All cells must be in the same row
                    if (newCell.X != selectedCells[0].X)
                        return false;

                    // Must be consecutive with no gaps
                    int minCol = selectedCells.Min(c => c.Y);
                    int maxCol = selectedCells.Max(c => c.Y);

                    return newCell.Y == minCol - 1 || newCell.Y == maxCol + 1;
                }
                else
                {
                    // All cells must be in the same column
                    if (newCell.Y != selectedCells[0].Y)
                        return false;

                    // Must be consecutive with no gaps
                    int minRow = selectedCells.Min(c => c.X);
                    int maxRow = selectedCells.Max(c => c.X);

                    return newCell.X == minRow - 1 || newCell.X == maxRow + 1;
                }
            }
        }

        private void UpdateWordScorePreview()
        {
            if (selectedCells.Count == 0 || selectedTiles.Count == 0)
            {
                wordScorePreviewLabel.Text = "Word Score: 0";
                return;
            }

            try
            {
                // Sort the selected cells
                List<Point> sortedCells = new List<Point>(selectedCells);
                sortedCells.Sort((p1, p2) => {
                    if (isHorizontal)
                        return p1.Y.CompareTo(p2.Y);
                    else
                        return p1.X.CompareTo(p2.X);
                });

                // Get the start position
                int startRow = sortedCells[0].X;
                int startCol = sortedCells[0].Y;

                // Get the tiles in order
                List<Tile> wordTiles = new List<Tile>();
                foreach (Point p in sortedCells)
                {
                    wordTiles.Add(selectedCellTileMap[p]);
                }

                // Calculate score
                int score = CalculateWordScore(wordTiles, startRow, startCol, isHorizontal);
                wordScorePreviewLabel.Text = $"Word Score: {score}";
            }
            catch (Exception ex)
            {
                wordScorePreviewLabel.Text = "Word Score: Error";
                AddToGameLog($"Error calculating word score: {ex.Message}");
            }
        }

        private int CalculateWordScore(List<Tile> wordTiles, int startRow, int startCol, bool horizontal)
        {
            // This is a simplified version - the actual implementation would need to match your ScrabbleBoard's scoring logic
            int score = 0;
            int wordMultiplier = 1;

            for (int i = 0; i < wordTiles.Count; i++)
            {
                int row = horizontal ? startRow : startRow + i;
                int col = horizontal ? startCol + i : startCol;
                int letterScore = wordTiles[i].Score;

                // Apply letter multipliers
                string cellIndicator = GetCellIndicator(row, col);
                if (cellIndicator == "DL")
                    letterScore *= 2;
                else if (cellIndicator == "TL")
                    letterScore *= 3;

                score += letterScore;
                // Apply word multipliers
                if (cellIndicator == "DW")
                    wordMultiplier *= 2;
                else if (cellIndicator == "TW")
                    wordMultiplier *= 3;
            }

            // Apply word multiplier
            score *= wordMultiplier;

            // Add connections with existing words
            // (This is simplified - proper implementation would check for connecting words)

            return score;
        }

        private void UpdateRackDisplay()
        {
            rackPanel.Controls.Clear();
            rackButtons.Clear();

            // Create label for the rack
            Label rackLabel = new Label
            {
                Text = "Your Tiles:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 5)
            };
            rackPanel.Controls.Add(rackLabel);

            int startX = 10;
            for (int i = 0; i < humanPlayer.Rack.Count; i++)
            {
                Tile tile = humanPlayer.Rack[i];
                Button tileButton = new Button
                {
                    Size = new Size(RackTileSize, RackTileSize),
                    Location = new Point(startX + i * (RackTileSize + 5), 30),
                    Text = $"{tile.Letter}\n({tile.Score})",
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    BackColor = Color.Bisque,
                    Tag = tile
                };

                // Handle blank tiles specially
                if (tile.Letter == ' ')
                {
                    tileButton.Text = "Blank";
                    tileButton.BackColor = Color.White;
                }

                tileButton.Click += RackTile_Click;
                rackPanel.Controls.Add(tileButton);
                rackButtons.Add(tileButton);
            }
        }

        private void RackTile_Click(object sender, EventArgs e)
        {
            if (isGameOver || isAITurn)
                return;

            Button clickedTile = sender as Button;
            Tile tile = (Tile)clickedTile.Tag;

            // Check if it's a blank tile that needs to be assigned a letter
            if (tile.Letter == ' ' && !selectedTiles.Contains(tile))
            {
                ShowBlankTileDialog(tile, clickedTile);
                return;
            }

            // If already selected, deselect
            if (selectedTiles.Contains(tile))
            {
                selectedTiles.Remove(tile);
                clickedTile.BackColor = Color.Bisque;

                // Remove the tile from any assigned cell
                Point? assignedCell = selectedCellTileMap.FirstOrDefault(kvp => kvp.Value == tile).Key;
                if (assignedCell.HasValue)
                {
                    selectedCellTileMap.Remove(assignedCell.Value);
                    Button cellButton = boardButtons[assignedCell.Value.X, assignedCell.Value.Y];
                    cellButton.BackColor = GetCellColor(assignedCell.Value.X, assignedCell.Value.Y);
                    cellButton.Text = GetCellIndicator(assignedCell.Value.X, assignedCell.Value.Y);

                    // Restore star on center cell
                    if (assignedCell.Value.X == 7 && assignedCell.Value.Y == 7)
                    {
                        cellButton.Text = "★\n" + cellButton.Text;
                    }
                }

                // Update the word score preview
                UpdateWordScorePreview();
                return;
            }

            // Select the tile
            if (selectedTiles.Count < selectedCells.Count)
            {
                selectedTiles.Add(tile);
                clickedTile.BackColor = Color.Yellow;

                // Assign the tile to the first unassigned cell
                Point? unassignedCell = selectedCells.FirstOrDefault(cell => !selectedCellTileMap.ContainsKey(cell));
                if (unassignedCell.HasValue)
                {
                    selectedCellTileMap[unassignedCell.Value] = tile;
                    Button cellButton = boardButtons[unassignedCell.Value.X, unassignedCell.Value.Y];
                    cellButton.Text = tile.Letter.ToString();
                    cellButton.Font = new Font("Arial", 12, FontStyle.Bold);

                    // Update the word score preview
                    UpdateWordScorePreview();
                }
            }
            else
            {
                AddToGameLog("Select a cell on the board before selecting another tile.");
            }
        }

        private void ShowBlankTileDialog(Tile blankTile, Button tileButton)
        {
            // Create a small form for selecting a letter
            Form blankForm = new Form
            {
                Text = "Select Letter for Blank Tile",
                Size = new Size(300, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            TableLayoutPanel letterPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 9
            };

            // Add letters A-Z
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < letters.Length; i++)
            {
                int row = i / 9;
                int col = i % 9;

                Button letterButton = new Button
                {
                    Text = letters[i].ToString(),
                    Dock = DockStyle.Fill,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };

                char letter = letters[i];
                letterButton.Click += (s, e) =>
                {
                    // Create a new Tile with the selected letter
                    Tile newTile = new Tile(letter, 0); // Assuming a constructor exists
                    tileButton.Tag = newTile;
                    tileButton.Text = $"{letter}\n(0)";
                    blankForm.DialogResult = DialogResult.OK;
                    blankForm.Close();
                };

                letterPanel.Controls.Add(letterButton, col, row);
            }

            blankForm.Controls.Add(letterPanel);
            blankForm.ShowDialog();
        }

        private void SubmitMove_Click(object? sender, EventArgs e)
        {
            if (selectedCells.Count == 0) return;

            // Gather the tiles to be placed
            List<Tile> wordTiles = new List<Tile>();
            foreach (Point cell in selectedCells)
            {
                if (selectedCellTileMap.TryGetValue(cell, out Tile tile))
                {
                    wordTiles.Add(tile);
                }
                else
                {
                    AddToGameLog("Error: Missing tile for selected cell.");
                    return;
                }
            }

            // Determine the starting position and orientation
            int startRow = selectedCells.Min(c => c.X);
            int startCol = selectedCells.Min(c => c.Y);

            // Check if the placement is horizontal or vertical
            bool isHorizontalPlacement = selectedCells.All(c => c.X == startRow);

            try
            {
                if (board.CanPlaceWord(wordTiles, startRow, startCol, isHorizontalPlacement, humanPlayer))
                {
                    // Place the word permanently
                    board.PlaceWord(wordTiles, startRow, startCol, isHorizontalPlacement);

                    // Update the board buttons to reflect the placed tiles
                    foreach (var cell in selectedCells)
                    {
                        if (selectedCellTileMap.TryGetValue(cell, out Tile tile))
                        {
                            Button cellButton = boardButtons[cell.X, cell.Y];
                            cellButton.Text = tile.Letter.ToString();
                            cellButton.Font = new Font("Arial", 12, FontStyle.Bold);
                            cellButton.BackColor = Color.Bisque; // Mark as placed
                        }
                    }

                    // Calculate and update the score
                    int wordScore = CalculateWordScore(wordTiles, startRow, startCol, isHorizontalPlacement);
                    humanPlayer.score += wordScore;

                    HighlightLastPlayedWord(selectedCells, true);

                    // Remove the played tiles from the rack
                    foreach (var tile in wordTiles)
                    {
                        humanPlayer.RemoveTileFromRack(tile);
                    }

                    AddToGameLog($"Word placed! Score: {wordScore}. Total Score: {humanPlayer.score}");

                    // Refill the rack
                    humanPlayer.RefillRack(tileBag);

                    // Update displays BEFORE switching turns
                    UpdateRackDisplay();
                    UpdateScores();

                    // Then switch turns
                    EndTurn();
                }
                else
                {
                    AddToGameLog("Invalid word placement. Try again.");
                }
            }
            catch (Exception ex)
            {
                AddToGameLog($"Error placing word: {ex.Message}");
            }
            finally
            {
                ResetSelections();
            }
        }

        private bool AreCellsContinuous(List<Point> cells)
        {
            if (cells.Count <= 1)
                return true;

            if (isHorizontal)
            {
                // All cells must be in the same row
                int row = cells[0].X;
                if (cells.Any(c => c.X != row))
                    return false;

                // Check for continuity in columns
                List<int> columns = cells.Select(c => c.Y).OrderBy(y => y).ToList();
                for (int i = 1; i < columns.Count; i++)
                {
                    if (columns[i] != columns[i - 1] + 1)
                        return false;
                }
            }
            else
            {
                // All cells must be in the same column
                int col = cells[0].Y;
                if (cells.Any(c => c.Y != col))
                    return false;

                // Check for continuity in rows
                List<int> rows = cells.Select(c => c.X).OrderBy(x => x).ToList();
                for (int i = 1; i < rows.Count; i++)
                {
                    if (rows[i] != rows[i - 1] + 1)
                        return false;
                }
            }

            return true;
        }

        private void ExchangeTiles_Click(object sender, EventArgs e)
        {
            if (isGameOver || isAITurn)
                return;

            if (tileBag.GetRemainingTilesCount() < 7)
            {
                AddToGameLog("Cannot exchange tiles when fewer than 7 tiles remain in the bag.");
                return;
            }

            if (selectedTiles.Count == 0)
            {
                AddToGameLog("No tiles selected for exchange. Please select tiles first.");
                return;
            }

            try
            {
                // Return selected tiles to the bag
                foreach (Tile tile in selectedTiles)
                {
                    tileBag.AddTile(tile);
                    humanPlayer.RemoveTileFromRack(tile);
                }

                // Draw new tiles
                humanPlayer.RefillRack(tileBag);
                UpdateRackDisplay();
                UpdateRemainingTiles();
                AddToGameLog($"Player 1 exchanged {selectedTiles.Count} tiles.");

                // End turn
                EndTurn();
            }
            catch (Exception ex)
            {
                AddToGameLog($"Error exchanging tiles: {ex.Message}");
            }
            finally
            {
                ResetSelections();
            }
        }

        private void PassTurn_Click(object sender, EventArgs e)
        {
            if (isGameOver || isAITurn)
                return;

            AddToGameLog("Player 1 passed their turn.");
            EndTurn();
            ResetSelections();
        }

        private void ToggleOrientation_Click(object sender, EventArgs e)
        {
            if (selectedCells.Count <= 1)
            {
                isHorizontal = !isHorizontal;
                toggleOrientationButton.Text = isHorizontal ? "Horizontal" : "Vertical";
            }
            else
            {
                AddToGameLog("Cannot change orientation after selecting multiple cells.");
            }
        }

        private void ClearSelections_Click(object sender, EventArgs e)
        {
            ResetSelections();
            AddToGameLog("Selections cleared.");
        }

        private void Help_Click(object sender, EventArgs e)
        {
            string helpText =
                "SCRABBLE GAME RULES\n\n" +
                "1. Game Objective: Form words using letter tiles on the board to score points.\n\n" +
                "2. Board: The center square is marked with a star. The first word must cover this square.\n\n" +
                "3. Bonus Squares:\n" +
                "   - Double Letter (DL): Doubles the score of a letter placed on it\n" +
                "   - Triple Letter (TL): Triples the score of a letter placed on it\n" +
                "   - Double Word (DW): Doubles the score of the entire word\n" +
                "   - Triple Word (TW): Triples the score of the entire word\n\n" +
                "4. Playing:\n" +
                "   - Select tiles from your rack\n" +
                "   - Select cells on the board to place them\n" +
                "   - Words must be placed left-to-right or top-to-bottom\n" +
                "   - Words must connect with existing words after the first move\n\n" +
                "5. Scoring:\n" +
                "   - Each letter has a point value\n" +
                "   - Bonus squares apply only on the turn they are covered\n\n" +
                "6. Game End:\n" +
                "   - Game ends when all tiles are used and one player has no tiles left\n" +
                "   - Or when no more valid moves can be made";

            MessageBox.Show(helpText, "Scrabble Game Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void AddToGameLog(string message)
        {
            gameLogTextBox.AppendText($"{DateTime.Now.ToString("HH:mm:ss")}: {message}{Environment.NewLine}");
            gameLogTextBox.ScrollToCaret();
        }

        private void UpdateScores()
        {
            if (aiPlayer != null && humanPlayer != null)
            {
                player1ScoreLabel.Text = $"{humanPlayer.Name}: {humanPlayer.score} points";
                player2ScoreLabel.Text = $"{aiPlayer.Name}: {(aiPlayer.score/11)+aiPlayer.score%10} points";
            }
        }

        private void UpdateRemainingTiles()
        {
            remainingTilesLabel.Text = $"Tiles in bag: {tileBag.GetRemainingTilesCount()}";
        }

        private void EndTurn()
        {
            UpdateBoardDisplay();
            isAITurn = !isAITurn;

            if (isAITurn)
            {
                // Update UI to show AI's turn
                currentTurnLabel.Text = "Current Turn: AI Player";
                currentTurnLabel.ForeColor = Color.DarkRed;

                // Disable human player controls
                TogglePlayerControls(false);

                AddToGameLog("AI Player's turn.");

                try
                {
                    // AI makes its move
                    aiPlayer.ExecuteBestMove(board, tileBag, verbose: false);

                    // Update the board UI to show AI's move
                    UpdateBoardDisplay();

                    // Highlight the AI's last played word
                    aiLastPlayedWordPositions = new List<Point>(lastPlayedWordPositions); // Use the same logic as human
                    HighlightLastPlayedWord(aiLastPlayedWordPositions, false);

                    AddToGameLog("AI Player completed its move.");

                    // Check if game is over
                    if (IsGameOver())
                    {
                        EndGame();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AddToGameLog($"Error during AI turn: {ex.Message}");
                }
                finally
                {
                    // End AI turn
                    isAITurn = false;
                    currentTurnLabel.Text = "Current Turn: Player 1";
                    currentTurnLabel.ForeColor = Color.DarkGreen;
                    TogglePlayerControls(true);
                    AddToGameLog("Player 1's turn.");
                }
            }
            else
            {
                currentTurnLabel.Text = "Current Turn: Player 1";
                currentTurnLabel.ForeColor = Color.DarkGreen;
                TogglePlayerControls(true);
                AddToGameLog("Player 1's turn.");
            }
        }

        private void TogglePlayerControls(bool enabled)
        {
            submitMoveButton.Enabled = enabled;
            exchangeTilesButton.Enabled = enabled;
            passTurnButton.Enabled = enabled;
            toggleOrientationButton.Enabled = enabled;
            clearSelectionsButton.Enabled = enabled;

            foreach (Button button in rackButtons)
            {
                button.Enabled = enabled;
            }
        }

        private void ResetSelections()
        {
            // Reset board cell display
            foreach (Point cell in selectedCells)
            {
                Button cellButton = boardButtons[cell.X, cell.Y];
                cellButton.BackColor = GetCellColor(cell.X, cell.Y);
                cellButton.Text = GetCellIndicator(cell.X, cell.Y);
                cellButton.Font = new Font("Arial", 8);

                // Restore star on center cell
                if (cell.X == 7 && cell.Y == 7)
                {
                    cellButton.Text = "★\n" + cellButton.Text;
                }
            }

            // Reset rack tile display
            foreach (Button button in rackButtons)
            {
                button.BackColor = Color.Bisque;
                button.Enabled = true;
            }

            // Clear selections
            selectedTiles.Clear();
            selectedCells.Clear();
            selectedCellTileMap.Clear();

            // Reset word score preview
            wordScorePreviewLabel.Text = "Word Score: 0";
        }

        private void PlaceWordOnBoard(List<Tile> wordTiles, int startRow, int startCol, bool horizontal)
        {
            for (int i = 0; i < wordTiles.Count; i++)
            {
                int row = horizontal ? startRow : startRow + i;
                int col = horizontal ? startCol + i : startCol;

                // Update the button to show the placed tile
                Button cellButton = boardButtons[row, col];
                cellButton.Text = wordTiles[i].Letter.ToString();
                cellButton.BackColor = Color.Bisque;
                cellButton.Font = new Font("Arial", 12, FontStyle.Bold);

                // Add a small indicator for the bonus type in the corner
                string indicator = GetCellIndicator(row, col);
                if (!string.IsNullOrEmpty(indicator))
                {
                    cellButton.Text += $"\n{indicator}";
                    cellButton.Font = new Font("Arial", 9, FontStyle.Bold);
                }
            }
        }

        private void UpdateBoardDisplay()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Tile tile = board.GetTileAt(row, col);
                    Button cellButton = boardButtons[row, col];

                    if (tile != null)
                    {
                        // Update the button to show the placed tile
                        cellButton.Text = tile.Letter.ToString();
                        cellButton.BackColor = GetCellColor(row, col); // Change to the color of the special square
                        cellButton.Font = new Font("Arial", 12, FontStyle.Bold);
                    }
                    else
                    {
                        // Reset the button to its default state
                        cellButton.Text = GetCellIndicator(row, col);
                        cellButton.BackColor = GetCellColor(row, col);
                        cellButton.Font = new Font("Arial", 8);
                    }
                }
            }
        }

        private bool IsGameOver()
        {
            // Game is over if:
            // 1. Tile bag is empty and at least one player has no tiles
            // 2. Both players pass twice in a row (implementation omitted for simplicity)

            if (tileBag.GetRemainingTilesCount() == 0 &&
                (humanPlayer.Rack.Count == 0 || aiPlayer.Rack.Count == 0))
            {
                return true;
            }

            return false;
        }

        private void EndGame()
        {
            isGameOver = true;

            // Calculate final scores (deduct points for remaining tiles)
            int humanRemainingPoints = humanPlayer.Rack.Sum(t => t.Score);
            int aiRemainingPoints = aiPlayer.Rack.Sum(t => t.Score);

            // If a player used all tiles, they get the points from opponent's rack
            if (humanPlayer.Rack.Count == 0)
            {
                humanPlayer.AddPoints(new List<Tile>(), 0, 0, true, board); // Provide required parameters
                AddToGameLog($"Player 1 gets {aiRemainingPoints} extra points from AI's remaining tiles.");
            }
            else if (aiPlayer.Rack.Count == 0)
            {
                aiPlayer.AddPoints(new List<Tile>(), 0, 0, true, board); // Provide required parameters
                AddToGameLog($"AI Player gets {humanRemainingPoints} extra points from Player 1's remaining tiles.");
            }
            else
            {
                // Both players have tiles left, deduct from their scores
                humanPlayer.AddPoints(new List<Tile>(), 0, 0, true, board); // Deduct points with empty word
                aiPlayer.AddPoints(new List<Tile>(), 0, 0, true, board);
                AddToGameLog($"Player 1 loses {humanRemainingPoints} points for remaining tiles.");
                AddToGameLog($"AI Player loses {aiRemainingPoints} points for remaining tiles.");
            }

            // Update final scores
            UpdateScores();

            // Determine winner
            string winner;
            if (humanPlayer.score > aiPlayer.score)
                winner = "Player 1";
            else if (aiPlayer.score > humanPlayer.score)
                winner = "AI Player";
            else
                winner = "It's a tie!";

            AddToGameLog($"Game Over! {winner} wins with {Math.Max(humanPlayer.score, aiPlayer.score)} points!");

            // Disable controls
            TogglePlayerControls(false);

            // Show game over message
            MessageBox.Show($"Game Over!\n\nPlayer 1: {humanPlayer.score} points\nAI Player: {aiPlayer.score} points\n\n{winner} wins!",
                          "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Add New Game button
            Button newGameButton = new Button
            {
                Text = "New Game",
                Size = new Size(120, 40),
                Location = new Point(500, 10),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            newGameButton.Click += (s, e) => {
                Application.Restart();
            };

            buttonPanel.Controls.Add(newGameButton);
        }
        private void HighlightLastPlayedWord(List<Point> wordPositions, bool isPlayer)
        {
            // Clear previous highlights
            List<Point> lastWordPositions = isPlayer ? lastPlayedWordPositions : aiLastPlayedWordPositions;
            foreach (var position in lastWordPositions)
            {
                Button cellButton = boardButtons[position.X, position.Y];
                cellButton.FlatAppearance.BorderSize = 0; // Remove border
            }

            // Highlight the new word
            if (isPlayer)
            {
                lastPlayedWordPositions = new List<Point>(wordPositions);
            }
            else
            {
                aiLastPlayedWordPositions = new List<Point>(wordPositions);
            }

            foreach (var position in wordPositions)
            {
                Button cellButton = boardButtons[position.X, position.Y];
                cellButton.FlatAppearance.BorderSize = 2;
                cellButton.FlatAppearance.BorderColor = isPlayer ? Color.Yellow : Color.Orange;
            }
        }

    }
}