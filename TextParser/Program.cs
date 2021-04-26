using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextParser
{
    class Parser
    {
        private class UniqueWords { public string Word; public int Frequency; }
        private class AdjacenWords { public string Word; public int Frequency; }

        static void Main(string[] args)
        {
            Document document = new Document();

            Regex reg_exp = new Regex("^?'*[^'А-Яа-яa-zA-Z0-9]");
            string text = reg_exp.Replace(document.getText(), " ");

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
                Console.WriteLine("Repeats: {1} Word: {0}", adjacenWord.Word.ToLower(), adjacenWord.Frequency);
        }

        private static List<UniqueWords> GetUniqueWords(List<string> words)
        {
            return words.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)
                //.Where(x => x.Count() >= 1)
                .Select(x => new UniqueWords { Word = x.Key, Frequency = x.Count() })
                .OrderByDescending(x => x.Frequency)
                .ThenBy(x => x.Word).Take(20).ToList();
        }

        private static List<AdjacenWords> GetAdjacenWords(List<string> words, List<UniqueWords> uniqueWords)
        {
            return words.Select((x, idx) => new { Word = x, Index = idx })
                .Where(x => uniqueWords.Any(y => y.Word.ToLower() == x.Word.ToLower()))
                .SelectMany(x => new[] {
                    x.Index != 0 ? words[x.Index - 1] : null,
                    x.Index < words.Count - 1 ? words[x.Index + 1] : null })
                .Where(x => x != null)
                .GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => new AdjacenWords { Word = x.Key, Frequency = x.Count() })
                .OrderByDescending(x => x.Frequency)
                .ThenBy(x => x.Word).Take(5).ToList();
        }
    }
}
