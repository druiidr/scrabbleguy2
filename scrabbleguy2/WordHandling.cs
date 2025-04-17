using System;
using System.Collections.Generic;
using System.Text;

namespace scrabbleguy
{
    internal class WordHandling
    {
        //transmute the included full scrabble dictionary included as a file into a ScrabbleDictionary object 
        static ScrabbleDictionary scrabbleDictionary = new ScrabbleDictionary(@"C:\Users\ASUS\source\repos\druiidr\scrabbleguy\scrabbleguy\fullScrabbleLegalDictionary.txt");//modify filepath when downloading
        static Trie preTrie = new Trie();
        static Trie postTrie = new Trie();
        static WordHandling()
        {
            foreach (string word in scrabbleDictionary.GetFullDictionary())
            {
                preTrie.Insert(word);
            }
        }
        public static string TilesToWord(List<Tile> word)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (Tile tile in word)
            {
                strBuilder.Append(tile.ToString());
            }

            return strBuilder.ToString();
        }
        public static bool ValidWord(List<Tile> word)
        {
            string str = TilesToWord(word);

            if (scrabbleDictionary.IsValidWord(str))
            {
                return true;
            }
            return false;
        }
    // Use Trie to suggest possible extensions of a prefix
    public static List<string> GetPossibleWords(string prefix)
        {
            return preTrie.FindWordsWith(prefix);
        }
    }
}
