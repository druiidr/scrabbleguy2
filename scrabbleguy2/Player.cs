using System.Data;

namespace scrabbleguy
{
    public class Player
    {
        public string Name { get; set; }
        public int score { get; set; }
        public List<Tile> Rack { get; private set; }
        private const int MaxTilesInRack = 7;

        public Player(string name)
        {
            Name = name;
            score = 0;
            Rack = new List<Tile>();
        }

        // Check if player has the tile in their rack
        public bool HasTileInRack(char letter)
        {
            return Rack.Any(t => t.Letter == letter);
        }

        // Remove a tile from the rack
        public void RemoveTileFromRack(Tile tile)
        {
            Tile match = Rack.FirstOrDefault(t => t.Letter == tile.Letter);
            if (match != null)
            {
                Rack.Remove(match);
            }
        }

        // Draw a tile and add it to the player's rack
        public void DrawTile(TileBag tileBag)
        {
            Tile tile = tileBag.DrawTile();
            if (tile != null)
            {
                Rack.Add(tile);
            }
        }

        // Refill the player's rack to 7 tiles
        public void RefillRack(TileBag tileBag)
        {
            Console.WriteLine($"{Name} is refilling their rack...");

            while (Rack.Count < MaxTilesInRack && !tileBag.IsEmpty())
            {
                Tile drawnTile = tileBag.DrawTile();
                Rack.Add(drawnTile);
                Console.WriteLine($"Drew tile: {drawnTile.Letter} (Score: {drawnTile.Score})");
            }

            Console.WriteLine($"{Name}'s rack after refill:");
            ShowRack();
        }
        public void RefillRack(List<Tile> tiles)
        {
            foreach (Tile tile in tiles)
            {
                Rack.Add(tile);
            }
        }

        // Show the player's rack (for testing purposes)
        public void ShowRack()
        {
            foreach (Tile tile in Rack)
            {
                Console.Write($"{tile.Letter}({tile.Score}) ");
            }
            Console.WriteLine();
        }

        // Manage the score from a given word, considering multipliers

       
        public int AddPoints(List<Tile> wordTiles, int startRow, int startCol, bool horizontal, ScrabbleBoard board)
        {
            int score = 0;
            int wordMultiplier = 1;

            for (int i = 0; i < wordTiles.Count; i++)
            {
                int row = horizontal ? startRow : startRow + i;
                int col = horizontal ? startCol + i : startCol;

                // Apply letter multipliers
                int tileScore = wordTiles[i].Score;
                tileScore = ApplyLetterMultiplier(row, col, tileScore);
                score += tileScore;

                // Apply word multipliers
                wordMultiplier *= ApplyWordMultiplier(row, col);
            }

            score *= wordMultiplier;

            // Add bingo bonus if all 7 tiles are used
            if (wordTiles.Count == 7)
            {
                score += 50;
            }

            this.score += score; // Update the player's total score
            return score;
        }

        // Apply letter multipliers
        public int ApplyLetterMultiplier(int row, int col, int score)
        {
            switch (row, col)
            {
                case (0, 3):
                case (0, 11):
                case (2, 6):
                case (2, 8):
                case (3, 0):
                case (3, 7):
                case (3, 14):
                case (6, 2):
                case (6, 6):
                case (6, 8):
                case (6, 12):
                case (7, 3):
                case (7, 11):
                case (8, 2):
                case (8, 6):
                case (8, 8):
                case (8, 12):
                case (11, 0):
                case (11, 7):
                case (11, 14):
                case (12, 6):
                case (12, 8):
                case (14, 3):
                case (14, 11):
                    if (!(this is AIPlayer))
                    {
                        LogMessage("Double Letter!!");
                    }
                    return score * 2;
                case (1, 5):
                case (1, 9):
                case (5, 1):
                case (5, 5):
                case (5, 9):
                case (5, 13):
                case (9, 1):
                case (9, 5):
                case (9, 9):
                case (9, 13):
                case (13, 5):
                case (13, 9):
                    if (!(this is AIPlayer))
                    {
                        LogMessage("Triple Letter!!!");
                    }
                    return score * 3;
                default:
                    return score;
            }
        }

        // Apply word multipliers
        public int ApplyWordMultiplier(int row, int col)
        {
            switch (row, col)
            {
                case (1, 1):
                case (2, 2):
                case (3, 3):
                case (4, 4):
                case (10, 10):
                case (11, 11):
                case (12, 12):
                    if (!(this is AIPlayer))
                    {
                        LogMessage("Double Word!!");
                    }
                    return 2; // Double the word score
                case (0, 0):
                case (0, 7):
                case (0, 14):
                case (7, 0):
                case (7, 14):
                case (14, 0):
                case (14, 7):
                case (14, 14):
                    if (!(this is AIPlayer))
                    {
                        LogMessage("Triple Word!!!");
                    }
                    return 3; // Triple the word score
                default:
                    return 1;
            }
        }

        // Player's turn: handle actions like placing a word and interacting with the board
        public void PlayerTurn(ScrabbleBoard board, TileBag tileBag)
        {
            int wordRow = 0;
            int rowCurr = 0;
            int wordCol = 0;
            int colCurr = 0;
            bool orientation = false; // true=horizontal, false=vertical;

            Console.WriteLine($"{Name}, it's your turn!");
            ShowRack();
            Console.WriteLine("Play the round or draw from tile bag? (true/false)");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && bool.TryParse(input, out bool playRound))
            {
                if (playRound)
                {
                    try
                    {
                        if (!board.IsBoardEmpty())
                        {

                            Console.WriteLine("Pick a starting row (0-14):");
                            wordRow = int.Parse(Console.ReadLine());


                            Console.WriteLine("Pick a starting column (0-14):");
                            wordCol = int.Parse(Console.ReadLine());

                        }
                        else
                        {
                            Console.WriteLine("first turn. working from center square");
                            wordRow = 7;
                            wordCol = 7;
                        }
                        rowCurr = wordRow;
                        colCurr = wordCol;

                        Console.WriteLine("Horizontal orientation? (true/false):");
                        orientation = bool.Parse(Console.ReadLine());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    Console.WriteLine("Pick tiles to form a word (e.g., enter the letters one by one):");
                    List<Tile> wordTiles = new List<Tile>();
                    char inputLetter;

                    do
                    {
                        Console.WriteLine("Enter a letter from your rack (or press '0' to finish):");
                        inputLetter = char.Parse(Console.ReadLine().ToUpper());

                        if (inputLetter != '0')
                        {
                            Tile boardTile = board.GetBoard()[rowCurr, colCurr];

                            // Check if there's already a tile on the board
                            if (boardTile != null && boardTile.Letter == inputLetter)
                            {
                                wordTiles.Add(boardTile); // Use the board's tile
                            }
                            else
                            {
                                // Look for the tile in the player's rack
                                Tile tileFromRack = Rack.FirstOrDefault(t => t.Letter == inputLetter);

                                if (tileFromRack != null)
                                {
                                    wordTiles.Add(tileFromRack);
                                    RemoveTileFromRack(tileFromRack); // Remove the tile from player's rack once used
                                }
                                else
                                {
                                    Console.WriteLine("You don't have that tile in your rack.");
                                }
                            }

                            // Move to the next position
                            if (orientation)
                                colCurr++;
                            else
                                rowCurr++;
                        }
                    }
                    while (inputLetter != '0');

                    // Check if the word can be placed
                    if (board.CanPlaceWord(wordTiles, wordRow, wordCol, orientation, this))
                    {
                        board.PlaceWord(wordTiles, wordRow, wordCol, orientation);
                        AddPoints(wordTiles, wordRow, wordCol, orientation, board);
                        Console.WriteLine(score);
                        board.PrintBoard();
                        board.PrintPlayedWords();
                    }
                    else
                    {
                        Console.WriteLine("not a valid turn attempt. lets try again!");
                        RefillRack(wordTiles);
                        PlayerTurn(board, tileBag);
                    }
                }
                else
                {
                    char inputLetter;
                    do
                    {
                        Console.WriteLine("Enter letters from your rack to remove (or press '0' to finish):");
                        inputLetter = char.Parse(Console.ReadLine().ToUpper());
                        Tile tileFromRack = Rack.FirstOrDefault(t => t.Letter == inputLetter);

                        if (tileFromRack != null)
                        {
                            tileBag.AddTiles(tileFromRack.Letter, 1, tileFromRack.Score);
                            RemoveTileFromRack(tileFromRack); // Remove the tile from player's rack once used
                        }
                        else
                        {
                            Console.WriteLine("You don't have that tile in your rack.");
                        }

                    } while (inputLetter != '0');
                    RefillRack(tileBag);

                }

                RefillRack(tileBag); // Refill the player's rack after their turn
            }
        }
        public List<Tile> GetRack()
        {
            return Rack;
        }
        protected void LogMessage(string message)
        {
            // Add the message to a log (e.g., for GUI or testing purposes)
            // This replaces direct console output
        }
        private void AddPointsToTotal(List<Tile> wordTiles, int startRow, int startCol, bool horizontal, ScrabbleBoard board)
        {
            // Call AddPoints only to update the total score
            AddPoints(wordTiles, startRow, startCol, horizontal, board);
        }
    }
}
