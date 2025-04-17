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
            bool isAttached = false;
            if (isEmpty)
            {
                isEmpty = false;
                isAttached = true;
            }

            // Validate if the main word is a valid word
            if (!WordHandling.ValidWord(wordTiles))
                return false;

            // Create a list to hold the combined word tiles (including existing tiles on the board)
            List<Tile> combinedWordTiles = new List<Tile>(wordTiles);

            // Find the complete word that's being formed (including adjacent tiles)
            List<Tile> fullMainWord = new List<Tile>();
            int startRow = wordRow;
            int startCol = wordCol;

            // First, check for any tiles before the start position
            if (horizontal)
            {
                int col = wordCol - 1;
                while (col >= 0 && board[wordRow, col] != null)
                {
                    isAttached = true;
                    fullMainWord.Insert(0, board[wordRow, col]);
                    col--;
                }
                // Starting position for main word
                startCol = col + 1;
            }
            else // vertical
            {
                int row = wordRow - 1;
                while (row >= 0 && board[row, wordCol] != null)
                {
                    isAttached = true;
                    fullMainWord.Insert(0, board[row, wordCol]);
                    row--;
                }
                // Starting position for main word
                startRow = row + 1;
            }

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
                    {
                        if (!(player is AIPlayer))
                        {
                            Console.WriteLine("Can't place a tile on an existing tile!!");
                        }
                        return false;
                    }
                    fullMainWord.Add(board[tileRow, tileCol]);
                }
                else
                {
                    fullMainWord.Add(wordTiles[i]);
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

                        string verticalWordStr = WordHandling.TilesToWord(newVerticalWord);
                        if (!playedWords.Contains(verticalWordStr))
                        {
                            // Add points for the new vertical word and add it to playedWords
                            player.AddPoints(newVerticalWord, GetVerticalWordStart(tileRow, tileCol), tileCol, false, this);
                            playedWords.Add(verticalWordStr);
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

                        string horizontalWordStr = WordHandling.TilesToWord(newHorizontalWord);
                        if (!playedWords.Contains(horizontalWordStr))
                        {
                            // Add points for the new horizontal word and add it to playedWords
                            player.AddPoints(newHorizontalWord, tileRow, GetHorizontalWordStart(tileRow, tileCol), true, this);
                            playedWords.Add(horizontalWordStr);
                        }
                    }
                }
            }

            // Now check for any tiles after the end position
            if (horizontal)
            {
                int endCol = wordCol + wordTiles.Count - 1;
                int col = endCol + 1;
                while (col < BoardSize && board[wordRow, col] != null)
                {
                    isAttached = true;
                    fullMainWord.Add(board[wordRow, col]);
                    col++;
                }
            }
            else // vertical
            {
                int endRow = wordRow + wordTiles.Count - 1;
                int row = endRow + 1;
                while (row < BoardSize && board[row, wordCol] != null)
                {
                    isAttached = true;
                    fullMainWord.Add(board[row, wordCol]);
                    row++;
                }
            }

            // If we've attached to existing tiles, we need to validate the full word
            if (isAttached && fullMainWord.Count > wordTiles.Count)
            {
                if (!WordHandling.ValidWord(fullMainWord))
                {
                    if (!(player is AIPlayer))
                    {
                        Console.WriteLine("Invalid combined word: " + WordHandling.TilesToWord(fullMainWord));
                    }
                    return false;
                }

                // Add points for the main word if it's not already played
                string mainWordStr = WordHandling.TilesToWord(fullMainWord);
                if (!playedWords.Contains(mainWordStr))
                {
                    player.AddPoints(fullMainWord, startRow, startCol, horizontal, this);
                    playedWords.Add(mainWordStr);
                }
            }
            else if (!isAttached)
            {
                // If the word is not attached to any existing tiles, it's invalid
                // unless it's the first word on the board
                return false;
            }
            else
            {
                // It's attached but the full word is just our word
                // Make sure to add points and add to played words
                string mainWordStr = WordHandling.TilesToWord(fullMainWord);
                if (!playedWords.Contains(mainWordStr))
                {
                    player.AddPoints(fullMainWord, startRow, startCol, horizontal, this);
                    playedWords.Add(mainWordStr);
                }
            }

            return true;
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
            for (int i = 0; i < wordTiles.Count; i++)
            {
                int row = horizontal ? startRow : startRow + i;
                int col = horizontal ? startCol + i : startCol;

                board[row, col] = wordTiles[i];
            }

            // Mark the board as no longer empty
            isEmpty = false;
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
            int tempRow = row;
            while (tempRow > 0 && board[tempRow - 1, col] != null)
            {
                tempRow--;
            }

            // Collect tiles from the start of the word downward
            while (tempRow <= 14 && board[tempRow, col] != null)
            {
                word.Add(board[tempRow, col]);
                tempRow++;
            }

            return word;
        }

        private List<Tile> FormHorizontalWord(int row, int col)
        {
            List<Tile> word = new List<Tile>();

            // Move left to the start of the word
            int tempCol = col;
            while (tempCol > 0 && board[row, tempCol - 1] != null)
            {
                tempCol--;
            }

            // Collect tiles from the start of the word to the right
            while (tempCol <= 14 && board[row, tempCol] != null)
            {
                word.Add(board[row, tempCol]);
                tempCol++;
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
    }
}