using System;
using System.Collections.Generic;

namespace scrabbleguy
{
    public class ScrabbleBoard
    {
        private Tile[,] board;
        private const int BoardSize = 15;
        private HashSet<string> playedWords = new HashSet<string>();
        private bool isEmpty = true;

        public ScrabbleBoard()
        {
            board = new Tile[BoardSize, BoardSize];
        }

        public bool IsBoardEmpty()
        {
            return isEmpty;
        }

        public Tile[,] GetBoard()
        {
            return board;
        }

        // Display the board
        public void PrintBoard()
        {
            Console.Write("   "); // Leading space for the row number column
            for (int i = 0; i < BoardSize; i++)
            {
                Console.Write((i < 10 ? " " : "") + i + " "); // Print column headers with extra space for single-digit numbers
            }
            Console.WriteLine();
            Console.WriteLine();

            for (int row = 0; row < BoardSize; row++)
            {
                Console.Write((row < 10 ? " " : "") + row + " "); // Print row number with extra space for single digits

                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] != null)
                    {
                        Console.Write(" " + board[row, col].Letter + " ");
                    }
                    else
                    {
                        Console.Write(" - ");
                    }
                }
                Console.WriteLine();
            }
        }

        // CanPlaceWord: Validate that the word can be placed considering both rack and board tiles


        public bool CanPlaceWord(List<Tile> wordTiles, int wordRow, int wordCol, bool horizontal, Player player)
        {
            // Tracks whether the word is attached to existing tiles
            bool isAttached = false;

            // Handle first word on empty board
            if (isEmpty)
            {
                // Special case: First word must go through center
                if (!(wordRow <= 7 && wordRow + (horizontal ? 0 : wordTiles.Count - 1) >= 7 &&
                      wordCol <= 7 && wordCol + (horizontal ? wordTiles.Count - 1 : 0) >= 7))
                {
                    return false; // First word must go through the center
                }

                isEmpty = false;
                isAttached = true;
            }

            // Find the complete word being formed (including existing tiles)

            // 1. Find existing tiles before the start position
            int startRow = wordRow;
            int startCol = wordCol;
            List<Tile> fullWordTiles = new List<Tile>();

            if (horizontal)
            {
                int col = wordCol - 1;
                while (col >= 0 && board[wordRow, col] != null)
                {
                    isAttached = true;
                    fullWordTiles.Insert(0, board[wordRow, col]);
                    startCol = col; // Update the starting column
                    col--;
                }
            }
            else // vertical
            {
                int row = wordRow - 1;
                while (row >= 0 && board[row, wordCol] != null)
                {
                    isAttached = true;
                    fullWordTiles.Insert(0, board[row, wordCol]);
                    startRow = row; // Update the starting row
                    row--;
                }
            }

            // 2. Add the tiles being placed (accounting for any overlapping tiles)
            for (int i = 0; i < wordTiles.Count; i++)
            {
                int tileRow = horizontal ? wordRow : wordRow + i;
                int tileCol = horizontal ? wordCol + i : wordCol;

                // Check board boundaries
                if (tileRow >= BoardSize || tileCol >= BoardSize)
                    return false;

                // Handle overlap with existing tiles
                if (board[tileRow, tileCol] != null)
                {
                    isAttached = true;

                    // Ensure the overlapping tile matches
                    if (board[tileRow, tileCol].Letter != wordTiles[i].Letter)
                        return false;

                    // Use the existing tile in our word
                    fullWordTiles.Add(board[tileRow, tileCol]);
                }
                else
                {
                    // Add the new tile from the player's hand
                    fullWordTiles.Add(wordTiles[i]);
                }

                // Check for perpendicular words formed
                if (horizontal)
                {
                    if (HasAdjacentTilesVertically(tileRow, tileCol))
                    {
                        isAttached = true;

                        // Form and validate the perpendicular word
                        List<Tile> perpWord = FormVerticalWord(tileRow, tileCol);

                        if (!WordHandling.ValidWord(perpWord))
                            return false;

                        // Score the perpendicular word if it's new
                        string perpWordStr = WordHandling.TilesToWord(perpWord);
                        if (!playedWords.Contains(perpWordStr))
                        {
                            int vertStartRow = GetVerticalWordStart(tileRow, tileCol);
                            player.AddPoints(perpWord, vertStartRow, tileCol, false, this);
                            playedWords.Add(perpWordStr);
                        }
                    }
                }
                else // vertical
                {
                    if (HasAdjacentTilesHorizontally(tileRow, tileCol))
                    {
                        isAttached = true;

                        // Form and validate the perpendicular word
                        List<Tile> perpWord = FormHorizontalWord(tileRow, tileCol);

                        if (!WordHandling.ValidWord(perpWord))
                            return false;

                        // Score the perpendicular word if it's new
                        string perpWordStr = WordHandling.TilesToWord(perpWord);
                        if (!playedWords.Contains(perpWordStr))
                        {
                            int horizStartCol = GetHorizontalWordStart(tileRow, tileCol);
                            player.AddPoints(perpWord, tileRow, horizStartCol, true, this);
                            playedWords.Add(perpWordStr);
                        }
                    }
                }
            }

            // 3. Find existing tiles after the end of the word
            if (horizontal)
            {
                int col = wordCol + wordTiles.Count;
                while (col < BoardSize && board[wordRow, col] != null)
                {
                    isAttached = true;
                    fullWordTiles.Add(board[wordRow, col]);
                    col++;
                }
            }
            else // vertical
            {
                int row = wordRow + wordTiles.Count;
                while (row < BoardSize && board[row, wordCol] != null)
                {
                    isAttached = true;
                    fullWordTiles.Add(board[row, wordCol]);
                    row++;
                }
            }

            // If not connected to any existing tiles and not the first word, invalid
            if (!isAttached && !isEmpty)
                return false;

            // Validate the main word
            if (!WordHandling.ValidWord(fullWordTiles))
                return false;

            // Add points for the main word if it's valid
            string mainWordStr = WordHandling.TilesToWord(fullWordTiles);
            if (!playedWords.Contains(mainWordStr))
            {
                player.AddPoints(fullWordTiles, startRow, startCol, horizontal, this);
                playedWords.Add(mainWordStr);
            }

            return true;
        }


        private bool CoversCenterSquare(int wordRow, int wordCol, int wordLength, bool horizontal)
        {
            int centerRow = BoardSize / 2;
            int centerCol = BoardSize / 2;

            if (horizontal)
            {
                return wordRow == centerRow && wordCol <= centerCol && wordCol + wordLength > centerCol;
            }
            else
            {
                return wordCol == centerCol && wordRow <= centerRow && wordRow + wordLength > centerRow;
            }
        }

        private bool HasAdjacentTiles(int row, int col)
        {
            // Check for adjacent tiles in all four directions
            return (row > 0 && GetTileAt(row - 1, col) != null) || // Above
                   (row < BoardSize - 1 && GetTileAt(row + 1, col) != null) || // Below
                   (col > 0 && GetTileAt(row, col - 1) != null) || // Left
                   (col < BoardSize - 1 && GetTileAt(row, col + 1) != null); // Right
        }
        // Helper methods to get the start position of perpendicular words
        private int GetVerticalWordStart(int row, int col)
        {
            while (row > 0 && board[row - 1, col] != null)
            {
                row--;
            }
            return row;
        }

        private int GetHorizontalWordStart(int row, int col)
        {
            while (col > 0 && board[row, col - 1] != null)
            {
                col--;
            }
            return col;
        }
        // Place the word on the board (only if CanPlaceWord passed)
        public bool CanPlaceWordWithoutScoring(List<Tile> wordTiles, int wordRow, int wordCol, bool horizontal, Player player)
        {
            bool isAttached = false;
            if (isEmpty)
            {
                isEmpty = false;
                isAttached = true;
            }

            // Extend the word to include any existing tiles before the anchor point
            List<Tile> extendedWordTiles = new List<Tile>(wordTiles);
            int startRow = wordRow;
            int startCol = wordCol;

            if (horizontal)
            {
                while (startCol > 0 && board[wordRow, startCol - 1] != null)
                {
                    startCol--;
                    extendedWordTiles.Insert(0, board[wordRow, startCol]);
                }
            }
            else
            {
                while (startRow > 0 && board[startRow - 1, wordCol] != null)
                {
                    startRow--;
                    extendedWordTiles.Insert(0, board[startRow, wordCol]);
                }
            }

            // Validate if the main word is a valid word
            if (!WordHandling.ValidWord(extendedWordTiles))
                return false;

            for (int i = 0; i < wordTiles.Count; i++)
            {
                int tileRow = horizontal ? wordRow : wordRow + i;
                int tileCol = horizontal ? wordCol + i : wordCol;

                // Check if there’s already a tile on the board at this position
                if (board[tileRow, tileCol] != null)
                {
                    isAttached = true;
                    if (board[tileRow, tileCol].Letter != wordTiles[i].Letter)
                    {
                        if (!(player is AIPlayer))
                        {
                            Console.WriteLine("Can't place a tile on an existing tile!!");
                        }
                        return false;
                    }
                }

                // Check for perpendicular words
                if (horizontal)
                {
                    if (HasAdjacentTilesVertically(tileRow, tileCol))
                    {
                        List<Tile> newVerticalWord = FormVerticalWord(tileRow, tileCol);
                        if (!WordHandling.ValidWord(newVerticalWord))
                            return false; // Invalid perpendicular word
                    }
                }
                else
                {
                    if (HasAdjacentTilesHorizontally(tileRow, tileCol))
                    {
                        List<Tile> newHorizontalWord = FormHorizontalWord(tileRow, tileCol);
                        if (!WordHandling.ValidWord(newHorizontalWord))
                            return false; // Invalid perpendicular word
                    }
                }
            }

            return isAttached; // Return true only if it’s attached to existing tiles
        }

        public bool CanPlaceWord(List<Tile> wordTiles, int wordRow, int wordCol, bool horizontal, Player player, bool addPoints = true)
        {
            bool isAttached = false;

            // If the board is empty, the first word must cover the center square
            if (isEmpty)
            {
                if (horizontal)
                {
                    if (wordRow != 7 || (wordCol > 7 || wordCol + wordTiles.Count <= 7))
                        return false;
                }
                else
                {
                    if (wordCol != 7 || (wordRow > 7 || wordRow + wordTiles.Count <= 7))
                        return false;
                }
                isAttached = true; // First word is always considered attached
            }

            // Validate if the main word is a valid word
            if (!WordHandling.ValidWord(wordTiles))
                return false;

            // Add the word tiles being placed (or use board tiles if there's an overlap)
            for (int i = 0; i < wordTiles.Count; i++)
            {
                int tileRow = horizontal ? wordRow : wordRow + i;
                int tileCol = horizontal ? wordCol + i : wordCol;

                // Check if there's already a tile on the board at this position
                if (board[tileRow, tileCol] != null)
                {
                    isAttached = true;
                    if (board[tileRow, tileCol].Letter != wordTiles[i].Letter)
                        return false; // Conflict with an existing tile
                }

                // Check for perpendicular words
                if (horizontal)
                {
                    if (HasAdjacentTilesVertically(tileRow, tileCol))
                    {
                        isAttached = true;
                        List<Tile> newVerticalWord = FormVerticalWord(tileRow, tileCol);
                        if (!WordHandling.ValidWord(newVerticalWord))
                            return false; // Invalid perpendicular word

                        if (addPoints)
                        {
                            string verticalWordStr = WordHandling.TilesToWord(newVerticalWord);
                            if (!playedWords.Contains(verticalWordStr))
                            {
                                player.AddPoints(newVerticalWord, GetVerticalWordStart(tileRow, tileCol), tileCol, false, this);
                                playedWords.Add(verticalWordStr);
                            }
                        }
                    }
                }
                else
                {
                    if (HasAdjacentTilesHorizontally(tileRow, tileCol))
                    {
                        isAttached = true;
                        List<Tile> newHorizontalWord = FormHorizontalWord(tileRow, tileCol);
                        if (!WordHandling.ValidWord(newHorizontalWord))
                            return false; // Invalid perpendicular word

                        if (addPoints)
                        {
                            string horizontalWordStr = WordHandling.TilesToWord(newHorizontalWord);
                            if (!playedWords.Contains(horizontalWordStr))
                            {
                                player.AddPoints(newHorizontalWord, tileRow, GetHorizontalWordStart(tileRow, tileCol), true, this);
                                playedWords.Add(horizontalWordStr);
                            }
                        }
                    }
                }
            }

            // If the word is not attached to any existing tiles, it's invalid unless it's the first word
            if (!isAttached)
                return false;

            // Add points for the main word if it's not already played
            if (addPoints)
            {
                string mainWordStr = WordHandling.TilesToWord(wordTiles);
                if (!playedWords.Contains(mainWordStr))
                {
                    player.AddPoints(wordTiles, wordRow, wordCol, horizontal, this);
                    playedWords.Add(mainWordStr);
                }
            }

            return true;
        }
        public void PlaceWord(List<Tile> wordTiles, int startRow, int startCol, bool horizontal)
        {
            int row = startRow;
            int col = startCol;

            // Clear the isEmpty flag when placing a word
            isEmpty = false;

            for (int i = 0; i < wordTiles.Count; i++)
            {
                if (board[row, col] == null) // Only place tile if the spot is empty
                {
                    board[row, col] = wordTiles[i];
                }

                if (horizontal)
                    col++;
                else
                    row++;
            }

            // Add the word to played words
            playedWords.Add(WordHandling.TilesToWord(wordTiles));
        }

        public bool ValidateNewWordWithExistingTiles(int row, int col, bool horizontal, List<Tile> placedTiles)
            {
                // Validate main word
                if (!WordHandling.ValidWord(placedTiles))
                    return false;

                List<Tile> combinedWordTiles = new List<Tile>(placedTiles);

                foreach (Tile tile in placedTiles)
                {
                    int tileRow = horizontal ? row : row++;
                    int tileCol = horizontal ? col++ : col;

                    if (horizontal && HasAdjacentTilesVertically(tileRow, tileCol))
                    {
                        List<Tile> newVerticalWord = FormVerticalWord(tileRow, tileCol);
                        if (!WordHandling.ValidWord(newVerticalWord))
                        {
                            Console.WriteLine("Invalid adjacent word: " + WordHandling.TilesToWord(newVerticalWord));
                            return false;
                        }
                    }
                    else if (!horizontal && HasAdjacentTilesHorizontally(tileRow, tileCol))
                    {
                        List<Tile> newHorizontalWord = FormHorizontalWord(tileRow, tileCol);
                        if (!WordHandling.ValidWord(newHorizontalWord))
                        {
                            Console.WriteLine("Invalid adjacent word: " + WordHandling.TilesToWord(newHorizontalWord));
                            return false;
                        }
                    }

                    if (board[tileRow, tileCol] != null)
                    {
                        combinedWordTiles.Add(board[tileRow, tileCol]);
                    }

                    if (horizontal)
                        col++;
                    else
                        row++;
                }

                // Validate the combined word
                if (!WordHandling.ValidWord(combinedWordTiles))
                    return false;

                return true; // All words are valid
            }


        private bool HasAdjacentTilesVertically(int row, int col)
        {
            return (row > 0 && board[row - 1, col] != null) || (row < 14 && board[row + 1, col] != null);
        }

        private bool HasAdjacentTilesHorizontally(int row, int col)
        {
            return (col > 0 && board[row, col - 1] != null) || (col < 14 && board[row, col + 1] != null);
        }

        private List<Tile> FormVerticalWord(int row, int col)
        {
            List<Tile> word = new List<Tile>();

            // Move up to the start of the word
            int startRow = row;
            while (startRow > 0 && board[startRow - 1, col] != null)
            {
                startRow--;
            }

            // Collect tiles from the start of the word downward
            for (int r = startRow; r < BoardSize; r++)
            {
                if (board[r, col] != null)
                {
                    word.Add(board[r, col]);
                }
                else
                {
                    break; // End of the word
                }
            }

            return word;
        }

        private List<Tile> FormHorizontalWord(int row, int col)
        {
            List<Tile> word = new List<Tile>();

            // Move left to the start of the word
            int startCol = col;
            while (startCol > 0 && board[row, startCol - 1] != null)
            {
                startCol--;
            }

            // Collect tiles from the start of the word to the right
            for (int c = startCol; c < BoardSize; c++)
            {
                if (board[row, c] != null)
                {
                    word.Add(board[row, c]);
                }
                else
                {
                    break; // End of the word
                }
            }

            return word;
        }
        public void PrintPlayedWords()
        {
            foreach (string word in playedWords)
            {
                Console.Write(word + ", ");
            }
            Console.WriteLine();
        }
        public Tile GetTileAt(int row, int col)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
                return null; // Out of bounds
            return board[row, col];
        }
    }
}