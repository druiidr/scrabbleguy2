using System;
using System.Collections.Generic;

namespace scrabbleguy
{
    public class Trie
    {
        private TrieNode root;

        public Trie()
        {
            root = new TrieNode();
        }
        public List<string> FindWordsWith(char[] letters)
        {
            List<string> results = new List<string>();
            FindWordsWithHelper(root, "", letters, results);
            return results;
        }

        private void FindWordsWithHelper(TrieNode node, string currentWord, char[] letters, List<string> results)
        {
            if (node.IsEndOfWord)
            {
                results.Add(currentWord);
            }

            foreach (char letter in letters)
            {
                if (node.Children.ContainsKey(letter))
                {
                    char[] remainingLetters = letters.Where(l => l != letter).ToArray();
                    FindWordsWithHelper(node.Children[letter], currentWord + letter, remainingLetters, results);
                }
            }
        }
        

        // Insert a word into the Trie
        public void Insert(string word)
        {
            TrieNode node = root;
            foreach (char c in word)
            {
                if (!node.Children.ContainsKey(c))
                {
                    node.Children[c] = new TrieNode();
                }
                node = node.Children[c];
            }
            node.IsEndOfWord = true;
        }
        public void ReverseInsert(string word)
        {
            TrieNode node = root;
            for(int i=word.Length-1;i>=0;i--)
            {
                if (!node.Children.ContainsKey(word[i]))
                {
                    node.Children[word[i]] = new TrieNode();
                }
                node = node.Children[word[i]];
            }
            node.IsEndOfWord = true;
        }

        // Check if a word exists in the Trie
        public bool Search(string word)
        {
            TrieNode node = root;
            foreach (char c in word)
            {
                if (!node.Children.ContainsKey(c))
                    return false;
                node = node.Children[c];
            }
            return node.IsEndOfWord;
        }

        // Check if any word starts with the given prefix
        public bool Includes(string fix)
        {
            TrieNode node = root;
            foreach (char c in fix)
            {
                if (!node.Children.ContainsKey(c))
                    return false;
                node = node.Children[c];
            }
            return true;
        }

        // Find all possible words that start with the given prefix
        public List<string> FindWordsWith(string fix)
        {
            TrieNode node = root;
            foreach (char c in fix)
            {
                if (!node.Children.ContainsKey(c))
                    return new List<string>();
                node = node.Children[c];
            }

            List<string> results = new List<string>();
            FindAllWords(node, fix, results);
            return results;
        }

        // Helper method to find all words from a given Trie node
        private void FindAllWords(TrieNode node, string currentWord, List<string> results)
        {
            if (node.IsEndOfWord)
            {
                results.Add(currentWord);
            }

            foreach (var kvp in node.Children)
            {
                FindAllWords(kvp.Value, currentWord + kvp.Key, results);
            }
        }
    }
}
