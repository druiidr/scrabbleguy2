using System;
using System.Collections.Generic;
using System.Linq;

namespace scrabbleguy
{
    public class AIPlayer : Player
    {
        private ScrabbleDictionary scrabbleDictionary;
        private bool scoreAdded = false; // Flag to track if score has been added
        private List<(int row, int col)> possibleStartingPoints = new List<(int row, int col)>(); // List of possible starting points

        public AIPlayer(string name, ScrabbleDictionary scrabbleDictionary) : base(name)
        {
            this.scrabbleDictionary = scrabbleDictionary;
            InitializeStartingPoints();
        }

        private void InitializeStartingPoints()
        {
            // Initialize with the center of the board if it's the first move
            possibleStartingPoints.Add((7, 7));
        }

        public void ExecuteBestMove(ScrabbleBoard board, TileBag tileBag, bool verbose = false)
        {
            Console.WriteLine($"{Name}'s turn (AI Player)");
            ShowRackForDebugging();

            scoreAdded = false; // Reset the flag at the start of each turn

            // If board is empty, place a word from the center
            if (board.IsBoardEmpty())
            {
                PlaceFirstWordFromCenter(board, tileBag);
                return;
            }

            // Find all possible word placements
            var possibleWords = new List<(string word, int score, int row, int col, bool horizontal)>();

            // Find words around existing tiles
            FindWordsAroundStartingPoints(board, possibleWords);

            // Sort possible words by score in descending order
            possibleWords = possibleWords.OrderByDescending(w => w.score).ToList();

            // Print all possible words and their scores
            foreach (var wordPlacement in possibleWords)
            {
                Console.WriteLine($"Possible word: {wordPlacement.word}");
            }

            // Try to place the highest-scoring word
            bool wordPlaced = false;
            foreach (var wordPlacement in possibleWords)
            {
                if (TryPlaceWord(board, tileBag, wordPlacement, verbose))
                {
                    wordPlaced = true;
                    break;
                }
            }

            board.PrintBoard();

            // If no word could be placed, exchange tiles
            if (!wordPlaced)
            {
                Console.WriteLine("No valid word found. Exchanging tiles.");
                ExchangeTiles(tileBag);
            }
        }

        private void PlaceFirstWordFromCenter(ScrabbleBoard board, TileBag tileBag)
        {
            int centerRow = 7;
            int centerCol = 7;

            // Generate valid words from rack that start at center
            var validFirstWords = GenerateAllWordCombinations(Rack)
                .Where(word => scrabbleDictionary.IsValidWord(word))
                .OrderByDescending(word => CalculateBaseScore(word))
                .ToList();

            foreach (string word in validFirstWords)
            {
                List<Tile> wordTiles = word.Select(c => Rack.First(t => t.Letter == c)).ToList();

                try
                {
                    // Try horizontal placement
                    if (board.CanPlaceWord(wordTiles, centerRow, centerCol, true, this))
                    {
                        board.PlaceWord(wordTiles, centerRow, centerCol, true);

                        // Remove used tiles from rack
                        foreach (var tile in wordTiles)
                        {
                            RemoveTileFromRack(tile);
                        }

                        // Only add points if not already added
                        if (!scoreAdded)
                        {
                            AddPoints(wordTiles, centerRow, centerCol, true, board);
                            scoreAdded = true;
                        }

                        RefillRack(tileBag);
                        UpdateStartingPoints(centerRow, centerCol, wordTiles.Count, true);
                        return;
                    }

                    // Try vertical placement
                    if (board.CanPlaceWord(wordTiles, centerRow, centerCol, false, this))
                    {
                        board.PlaceWord(wordTiles, centerRow, centerCol, false);

                        // Remove used tiles from rack
                        foreach (var tile in wordTiles)
                        {
                            RemoveTileFromRack(tile);
                        }

                        // Only add points if not already added
                        if (!scoreAdded)
                        {
                            AddPoints(wordTiles, centerRow, centerCol, false, board);
                            scoreAdded = true;
                        }

                        RefillRack(tileBag);
                        UpdateStartingPoints(centerRow, centerCol, wordTiles.Count, false);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error placing first word: {ex.Message}");
                }
            }

            // If no word can be placed, exchange tiles
            ExchangeTiles(tileBag);
        }

        private void UpdateStartingPoints(int startRow, int startCol, int wordLength, bool horizontal)
        {
            // Remove the starting point where the word was placed
            possibleStartingPoints.Remove((startRow, startCol));

            // Add new starting points adjacent to the newly placed word
            for (int i = 0; i < wordLength; i++)
            {
                int row = horizontal ? startRow : startRow + i;
                int col = horizontal ? startCol + i : startCol;

                // Add adjacent points
                if (horizontal)
                {
                    if (row > 0) possibleStartingPoints.Add((row - 1, col));
                    if (row < 14) possibleStartingPoints.Add((row + 1, col));
                }
                else
                {
                    if (col > 0) possibleStartingPoints.Add((row, col - 1));
                    if (col < 14) possibleStartingPoints.Add((row, col + 1));
                }
            }
        }

        private IEnumerable<string> GenerateAllWordCombinations(List<Tile> rack)
        {
            // Implementation for generating all word combinations from the rack
            var letters = rack.Select(t => t.Letter).ToArray();
            return GetPermutations(letters, letters.Length).Select(p => new string(p.ToArray()));
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(T[] list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                            (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        private bool TryPlaceWord(ScrabbleBoard board, TileBag tileBag,
            (string word, int score, int row, int col, bool horizontal) wordPlacement, bool verbose = false)
        {
            // Extract word details
            string wordToPlace = wordPlacement.word;
            int row = wordPlacement.row;
            int col = wordPlacement.col;
            bool horizontal = wordPlacement.horizontal;

            List<Tile> wordTiles = new List<Tile>();

            // Create tiles from rack
            foreach (char c in wordToPlace)
            {
                Tile tile = Rack.FirstOrDefault(t => t.Letter == c);
                if (tile == null)
                {
                    return false; // Cannot form word with current rack
                }
                wordTiles.Add(tile);
            }

            try
            {
                // Try to place the word
                if (board.CanPlaceWord(wordTiles, row, col, horizontal, this))
                {
                    // Place the word on the board
                    board.PlaceWord(wordTiles, row, col, horizontal);

                    // Remove used tiles from rack
                    foreach (var tile in wordTiles)
                    {
                        RemoveTileFromRack(tile);
                    }

                    // Only add points if not already added
                    if (!scoreAdded)
                    {
                        AddPoints(wordTiles, row, col, horizontal, board);
                        scoreAdded = true;
                    }

                    // Refill rack
                    RefillRack(tileBag);

                    Console.WriteLine($"AI placed word: {wordToPlace} at ({row},{col}) " +
                                      $"{(horizontal ? "horizontally" : "vertically")}");
                    UpdateStartingPoints(row, col, wordTiles.Count, horizontal);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine($"Error placing word: {ex.Message}");
                }
            }

            return false;
        }

        private void FindWordsAroundStartingPoints(ScrabbleBoard board,
            List<(string word, int score, int row, int col, bool horizontal)> possibleWords)
        {
            // Generate possible words around each starting point
            foreach (var (row, col) in possibleStartingPoints)
            {
                // Check adjacent empty spaces in all 4 directions
                FindPossibleWordsFromPosition(board, row, col, true, possibleWords);   // Horizontal
                FindPossibleWordsFromPosition(board, row, col, false, possibleWords);  // Vertical
            }
        }

        private void FindPossibleWordsFromPosition(ScrabbleBoard board, int anchorRow, int anchorCol,
            bool horizontal, List<(string word, int score, int row, int col, bool horizontal)> possibleWords)
        {
            // Get rack tiles as a string
            string rackTiles = new string(Rack.Select(t => t.Letter).ToArray());

            // Try different word lengths
            for (int length = 2; length <= 7; length++)
            {
                // Generate combinations of rack tiles
                var combinations = GetCombinations(rackTiles, length);

                foreach (var wordChars in combinations)
                {
                    string word = new string(wordChars.ToArray());

                    // Check if the word is valid in the dictionary
                    if (scrabbleDictionary.IsValidWord(word))
                    {
                        // Try to place the word across existing tiles
                        int bestScore = TryPlaceWordAroundTile(board, word, anchorRow, anchorCol, horizontal);

                        if (bestScore > 0)
                        {
                            possibleWords.Add((word, bestScore, anchorRow, anchorCol, horizontal));
                        }
                    }
                }
            }
        }

        private int TryPlaceWordAroundTile(ScrabbleBoard board, string word, int anchorRow, int anchorCol, bool horizontal)
        {
            // Find the range of possible placements
            int wordScore = 0;
            List<Tile> potentialWordTiles = new List<Tile>();

            // Convert word to tiles from rack
            foreach (char c in word)
            {
                Tile tile = Rack.FirstOrDefault(t => t.Letter == c);
                if (tile == null) return 0;
                potentialWordTiles.Add(tile);
            }

            // Try placements that intersect with the anchor tile
            for (int offset = 0; offset < word.Length; offset++)
            {
                int startRow = horizontal ? anchorRow : anchorRow - offset;
                int startCol = horizontal ? anchorCol - offset : anchorCol;

                // Ensure we're using the correct tile at the anchor point
                if (horizontal && startCol + offset >= anchorCol &&
                    startCol + offset < anchorCol + 1 &&
                    board.GetBoard()[anchorRow, anchorCol] != null &&
                    word[offset] == board.GetBoard()[anchorRow, anchorCol].Letter)
                {
                    // Try to place the word without scoring
                    try
                    {
                        if (board.CanPlaceWordWithoutScoring(potentialWordTiles, startRow, startCol, horizontal, this))
                        {
                            // Calculate potential score
                            wordScore = CalculateWordScore(potentialWordTiles, startRow, startCol, horizontal, board);
                            return wordScore;
                        }
                    }
                    catch { }
                }
                else if (!horizontal && startRow + offset >= anchorRow &&
                         startRow + offset < anchorRow + 1 &&
                         board.GetBoard()[anchorRow, anchorCol] != null &&
                         word[offset] == board.GetBoard()[anchorRow, anchorCol].Letter)
                {
                    // Try to place the word without scoring
                    try
                    {
                        if (board.CanPlaceWordWithoutScoring(potentialWordTiles, startRow, startCol, horizontal, this))
                        {
                            // Calculate potential score
                            wordScore = CalculateWordScore(potentialWordTiles, startRow, startCol, horizontal, board);
                            return wordScore;
                        }
                    }
                    catch { }
                }
            }

            return 0;
        }

        private int CalculateWordScore(List<Tile> wordTiles, int startRow, int startCol, bool horizontal, ScrabbleBoard board)
        {
            int score = 0;
            int wordMultiplier = 1;

            for (int i = 0; i < wordTiles.Count; i++)
            {
                int row = horizontal ? startRow : startRow + i;
                int col = horizontal ? startCol + i : startCol;

                // Score calculation
                int tileScore = wordTiles[i].Score;
                tileScore = ApplyLetterMultiplier(row, col, tileScore);
                score += tileScore;
                wordMultiplier *= ApplyWordMultiplier(row, col);
            }

            score *= wordMultiplier;
            if (wordTiles.Count == 7) score += 50; // Bingo bonus

            return score;
        }

        private int CalculateBaseScore(string word)
        {
            return word.Sum(c => Rack.First(t => t.Letter == c).Score);
        }

        private void ExchangeTiles(TileBag tileBag)
        {
            Console.WriteLine($"{Name} is exchanging tiles.");

            // Return all tiles to the bag and draw new ones
            foreach (var tile in Rack.ToList())
            {
                tileBag.AddTiles(tile.Letter, 1, tile.Score);
                RemoveTileFromRack(tile);
            }
            RefillRack(tileBag);
        }

        // Debugging method for visibility
        public void ShowRackForDebugging()
        {
            Console.WriteLine($"{Name}'s rack:");
            ShowRack();
        }

        private IEnumerable<IEnumerable<char>> GetCombinations(string letters, int length)
        {
            if (length == 1) return letters.Select(t => new char[] { t });
            return GetCombinations(letters, length - 1)
                .SelectMany(t => letters.Where(e => !t.Contains(e)),
                            (t1, t2) => t1.Concat(new char[] { t2 }));
        }
    }
}