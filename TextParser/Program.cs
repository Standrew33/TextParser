using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextParser
{
    class Parser
    {
        private class UniqueWords { public string Word; public int Frequency; }
        public struct Neighbor { public string Word; public int Frequency; }
        private class AdjacenWords { public string Word; public List<Neighbor> Neighbors; }

        static void Main(string[] args)
        {
            Document document = new Document();

            Regex reg_exp = new Regex("^?'*[^А-Яа-яa-zA-Z0-9]");
            string text = reg_exp.Replace(document.getText().Replace(Environment.NewLine, " "), " ");

            //All words in text without punctuation and whiteplaces
            List<string> allWords = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            //Top 20 unique words
            List<UniqueWords> uniqueWords = GetUniqueWords(allWords);

            //Top 5 neighbors
            List<AdjacenWords> adjacenWords = GetAdjacenWords(allWords, uniqueWords);

            Console.WriteLine("Total unique words: " + allWords.Distinct().Count());
            Console.WriteLine();

            Console.WriteLine("Top 20 unique words");
            foreach (var uniqueWord in uniqueWords)
                Console.WriteLine("Repeats: {1} Word: {0}", uniqueWord.Word.ToLower(), uniqueWord.Frequency);
            Console.WriteLine();

            Console.WriteLine("Top 5 neighbors");
            foreach (var adjacenWord in adjacenWords)
            {
                Console.WriteLine("Word: {0}", adjacenWord.Word.ToLower());
                foreach (var neighbors in adjacenWord.Neighbors)
                    Console.WriteLine("Repeats: {1} Neighbor: {0}", neighbors.Word.ToLower(), neighbors.Frequency);
                Console.WriteLine();
            }

        }

        private static List<UniqueWords> GetUniqueWords(List<string> words)
        {
            return words.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => new UniqueWords { Word = x.Key, Frequency = x.Count() })
                .OrderByDescending(x => x.Frequency)
                .ThenBy(x => x.Word).Take(20).ToList();
        }

        private static List<AdjacenWords> GetAdjacenWords(List<string> words, List<UniqueWords> uniqueWords)
        {
            List<AdjacenWords> adjacenWords = new List<AdjacenWords>();
            var wordWNeighbor = words.Select((x, idx) => new { Word = x, Index = idx })
                .Where(x => uniqueWords.Any(y => y.Word.ToLower() == x.Word.ToLower()))
                .Join(uniqueWords, u => u.Word, w => w.Word, (u, w) => new { u.Word, u.Index, Frequency = w.Frequency })
                .Select(x => new { Word = x.Word, Frequency = x.Frequency, Neighbors = new[] { 
                    x.Index != 0 ? words[x.Index - 1] : null, 
                    x.Index < words.Count - 1 ? words[x.Index + 1] : null } 
                })
                .OrderByDescending(x => x.Frequency)
                .ThenBy(x => x.Word)
                .GroupBy(x => x.Word, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => new { x.Key, Neighbor = x.Count() > 1 ? 
                    x.ElementAt(0).Neighbors.Concat(x.ElementAt(1).Neighbors).ToList() : 
                    x.ElementAt(0).Neighbors.ToList() 
                }).ToList();

            foreach (var testword in wordWNeighbor)
            {
                AdjacenWords adWords = new AdjacenWords() {
                    Word = testword.Key,
                    Neighbors = testword.Neighbor
                        .Where(x => x != null)
                        .GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)
                        .Select(x => new Neighbor { Word = x.Key, Frequency = x.Count() })
                        .OrderByDescending(x => x.Frequency)
                        .ThenBy(x => x.Word).Take(5).ToList()
                };
                adjacenWords.Add(adWords);
            }
            
            return adjacenWords;
        }
    }
}
