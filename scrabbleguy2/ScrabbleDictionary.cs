using System;
using System.Collections.Generic;
using System.IO;

namespace scrabbleguy
{
    public class ScrabbleDictionary
    {
        private HashSet<string> validWords;
        public Trie WordTrie { get; private set; }
        public Trie FlippedWordTrie { get; private set; }

        public ScrabbleDictionary(string filePath)
        {
            validWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // For case-insensitive matching
            WordTrie = new Trie();
            FlippedWordTrie = new Trie();
            LoadWordsFromFile(filePath);
        }

        private void LoadWordsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Dictionary file not found!");
                return;
            }

            foreach (var line in File.ReadLines(filePath))
            {
                string word = line.Trim();
                validWords.Add(word);
                WordTrie.Insert(word);
                FlippedWordTrie.ReverseInsert(word);
            }
        }

        public bool IsValidWord(string word)
        {
            return validWords.Contains(word.ToUpper());
        }

        public HashSet<string> GetFullDictionary()
        {
            return validWords;
        }
    }
}