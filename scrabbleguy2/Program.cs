using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace scrabbleguy
{
    class Program
    {
        private static void RunConsoleGame()
        {
            ScrabbleBoard board = new ScrabbleBoard();
            TileBag tileBag = new TileBag();
            ScrabbleDictionary scrabbleDictionary = new ScrabbleDictionary(@"C:\Users\ASUS\source\repos\druiidr\scrabbleguy\scrabbleguy\fullScrabbleLegalDictionary.txt");
            Player player1 = new Player("player1");
            Console.WriteLine("Play against AI? (true/false)");
            bool isPlayer2Ai = bool.Parse(Console.ReadLine() ?? "false");
            Player player2 = isPlayer2Ai ? new AIPlayer("player2", scrabbleDictionary) : new Player("player2");

            for (int i = 0; i < 7; i++)
            {
                player1.DrawTile(tileBag);
                player2.DrawTile(tileBag);
            }

            board.PrintBoard();

            while (!tileBag.IsEmpty())
            {
                player1.PlayerTurn(board, tileBag);

                if (isPlayer2Ai)
                {
                    ((AIPlayer)player2).ExecuteBestMove(board, tileBag);
                }
                else
                {
                    player2.PlayerTurn(board, tileBag);
                }
            }

            Player winner = player1.score > player2.score ? player1 : player2;
            Console.WriteLine("Sack cleared. {0} won", winner.Name);
        }
    }

}
