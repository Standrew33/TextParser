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

            Console.WriteLine("Total unique words: " + allWords.Distinct(StringComparer.CurrentCultureIgnoreCase).Count());
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
            return words.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)     //---Grouping the same words case insensitive
                .Select(x => new UniqueWords { Word = x.Key, Frequency = x.Count() })   //---Selection of grouped words and their number
                .OrderByDescending(x => x.Frequency)                                    //---Reverse sorting by frequency
                .ThenBy(x => x.Word).Take(20).ToList();                                 //---Sorting further alphabetically (for same word frequency) and taking the first 20 words
        }

        private static List<AdjacenWords> GetAdjacenWords(List<string> words, List<UniqueWords> uniqueWords)
        {
            List<AdjacenWords> adjacenWords = new List<AdjacenWords>();
            var wordWNeighbor = words.Select((x, idx) => new { Word = x, Index = idx })                     //---Sample all words to get each index
                .Where(x => uniqueWords.Any(y => y.Word.ToLower() == x.Word.ToLower()))                     //---Filter only words from Top 20
                .Join(uniqueWords, u => u.Word, w => w.Word,                                                //---Link words from a unique list for further sorting by frequency
                    (u, w) => new { u.Word, u.Index, Frequency = w.Frequency })
                .Select(x => new { Word = x.Word, Frequency = x.Frequency, Neighbors = new[] {              //---Get neighbors by indices for each word
                    x.Index != 0 ? words[x.Index - 1] : null, 
                    x.Index < words.Count - 1 ? words[x.Index + 1] : null } 
                })
                .OrderByDescending(x => x.Frequency)                                                        //---Sort by frequency
                .ThenBy(x => x.Word)                                                                        //---Sorting further alphabetically (for same word frequency)
                .GroupBy(x => x.Word, StringComparer.InvariantCultureIgnoreCase)                            //---Grouping by the same words
                .Select(x => new { x.Key, Neighbor = x.SelectMany(x => x.Neighbors).ToList()}).ToList();    //---Combining all neighbors for a word

            foreach (var word in wordWNeighbor)
            {
                AdjacenWords adWords = new AdjacenWords() {
                    Word = word.Key,
                    Neighbors = word.Neighbor
                        .Where(x => x != null)                                                  //---Filter empty neighbors if words were extreme
                        .GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)             //---Grouping the same neighbors
                        .Select(x => new Neighbor { Word = x.Key, Frequency = x.Count() })      //---Selection of grouped neighbors and their number
                        .OrderByDescending(x => x.Frequency)                                    //---Reverse sorting by frequency
                        .ThenBy(x => x.Word).Take(5).ToList()                                   //---Sorting further alphabetically (for same word frequency) and taking the first 5 words
                };
                adjacenWords.Add(adWords);
            }
            
            return adjacenWords;
        }
    }
}
