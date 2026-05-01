using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SofiaSearch
{
    public class WordEntry
    {
        public string Word { get; set; }
        public int Score { get; set; }

        public WordEntry(string word, int score)
        {
            this.Word = word;
            this.Score = score;
        }
    }

    public class TermNode
    {
        public Dictionary<char, TermNode> Children { get; set; } = new Dictionary<char, TermNode>();

        public List<WordEntry> TopMatches { get; set; } = new List<WordEntry>();
        public bool IsEndWord { get; set; }

        public int Score { get; set; }

        public TermNode(bool isEndWord)
        {
            this.IsEndWord = isEndWord;
        }
    }

    public class AutocompleteEngine
    {
        private TermNode _root;

        public AutocompleteEngine()
        {
            this._root = new TermNode(false);
        }

        public void AddTerm(string term, int score)
        {
            TermNode current = _root;

            foreach (char ch in term)
            {
                //Check if a node exists for the character
                if (!current.Children.ContainsKey(ch))
                {
                    //If it doesen't exist create a new node and append to children
                    var newNode = new TermNode(false);

                    current.Children[ch] = newNode;
                }

                //Move current to child node
                current = current.Children[ch];

                if (current.TopMatches.Any(x => x.Word == term))
                {
                    var existingWord = current.TopMatches.First(x => x.Word == term);
                    existingWord.Score = score;
                }
                else
                {
                    current.TopMatches.Add(new WordEntry(term, score));
                }

                current.TopMatches = current.TopMatches
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Word)
                    .Take(10)
                    .ToList();
            }

            //Set word and score flags
            current.IsEndWord = true;
            current.Score = score;
        }

        public string GetTop(string prefix, int limit)
        {
            TermNode current = _root;
            var results = new List<WordEntry>();

            //Run iterate over each prefix character
            foreach (char prefixChar in prefix)
            {
                //In case no such character exists in the tree
                if (!current.Children.ContainsKey(prefixChar))
                {
                    return "NO_MATCH";
                }

                //move current pointer to child
                current = current.Children[prefixChar];
            }

            if (limit <= 10 && current.TopMatches.Count > 0)
            {
                results = current.TopMatches.Take(limit).ToList();
            }
            else
            {
                var stack = new Stack<(TermNode node, string word)>();
                var tempResults = new List<(string word, int score)>();
                stack.Push((current, prefix));


                while (stack.Count > 0)
                {
                    var (currentNode, currentWord) = stack.Pop();

                    if (currentNode.IsEndWord)
                    {
                        tempResults.Add((currentWord.ToString(), currentNode.Score));
                    }

                    foreach (var child in currentNode.Children)
                    {
                        stack.Push((child.Value, currentWord + child.Key));
                    }
                }

                results = tempResults
                    .OrderByDescending(x => x.score)
                    .ThenBy(x => x.word)
                    .Take(limit)
                    .Select(x => new WordEntry(x.word, x.score))
                    .ToList();
            }



            //basic string.join pattern
            return string.Join(",", results.Select(x => x.Word));
        }

        public void RemoveTerm(string term)
        {
            TermNode current = _root;
            //DFS stack to keep track of our parent(for removing children), our character(for indexing the dictionary), and our child node
            var stack = new Stack<(TermNode Parent, char ch, TermNode Child)>();

            //as when we are adding iterate over each character of the term to make sure it exists in the tree
            foreach (char ch in term)
            {
                if (!current.Children.ContainsKey(ch))
                    return;

                if (current.TopMatches.Any(x => x.Word == term))
                {
                    current.TopMatches.RemoveAll(x => x.Word == term);
                }
                var child = current.Children[ch];

                stack.Push((current, ch, child));
                current = child;
            }

            //If last node is not an EndWord we don't need to remove it already is not considered a word in the tree
            if (!current.IsEndWord)
                return;

            //unflag last node
            current.IsEndWord = false;


            while (stack.Count > 0)
            {
                var (parent, ch, child) = stack.Pop();

                //We have to check if our node is a leaf node and if it is not a EndOfWord only then can we safely remove it from our tree
                //otherwise the flag serves the same purpose as removing
                if (child.Children.Count == 0 && !child.IsEndWord)
                {
                    parent.Children.Remove(ch);
                }
                else
                {
                    break;
                }
            }
        }
    }

    class Program
    {
        static int totalTests = 0;
        static int passedTests = 0;

        static void Main(string[] args)
        {
            AutocompleteEngine engine = new AutocompleteEngine();

            // Test 1: Adding same term twice (should update score)
            RunTest(engine, "AddSameTermTwice", () =>
            {
                engine.AddTerm("test", 100);
                engine.AddTerm("test", 200);
                var result = engine.GetTop("test", 1);
                return ("test", result);
            });

            // Test 2: Double delete (should not throw error)
            RunTest(engine, "DoubleDelete", () =>
            {
                engine.AddTerm("deleteable", 50);
                engine.RemoveTerm("deleteable");
                engine.RemoveTerm("deleteable"); // Second delete should not throw
                var result = engine.GetTop("deleteable", 1);
                return ("NO_MATCH", result);
            });

            // Test 3: Deleting doesn't break relationships
            RunTest(engine, "DeleteDoesNotBreakRelationships", () =>
            {
                engine.AddTerm("soft", 50);
                engine.AddTerm("software", 200);
                engine.AddTerm("sofia", 100);
                engine.RemoveTerm("soft"); // Delete soft
                // Other terms should still work
                var result = engine.GetTop("sof", 3);
                return ("software,sofia", result);
            });

            // Test 4: GetTop returns valid results - basic
            RunTest(engine, "GetTopBasic", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("apple", 100);
                testEngine.AddTerm("banana", 200);
                testEngine.AddTerm("apricot", 150);
                var result = testEngine.GetTop("a", 3);
                return ("apricot,apple", result);
            });

            // Test 5: GetTop with limit
            RunTest(engine, "GetTopWithLimit", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("a1", 100);
                testEngine.AddTerm("a2", 200);
                testEngine.AddTerm("a3", 300);
                testEngine.AddTerm("a4", 400);
                var result = testEngine.GetTop("a", 2);
                return ("a4,a3", result);
            });

            // Test 6: NO_MATCH for non-existent prefix
            RunTest(engine, "NoMatchNonExistent", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("hello", 100);
                var result = testEngine.GetTop("xyz", 1);
                return ("NO_MATCH", result);
            });

            // Test 7: Empty prefix
            RunTest(engine, "EmptyPrefix", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("hello", 100);
                testEngine.AddTerm("help", 200);
                var result = testEngine.GetTop("", 2);
                return ("help,hello", result);
            });

            // Test 8: Score ties - alphabetical order
            RunTest(engine, "ScoreTiesAlphabetical", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("zebra", 100);
                testEngine.AddTerm("apple", 100);
                testEngine.AddTerm("banana", 100);
                var result = testEngine.GetTop("", 3);
                return ("apple,banana,zebra", result);
            });

            // Test 9: Remove term that doesn't exist
            RunTest(engine, "RemoveNonExistent", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("hello", 100);
                testEngine.RemoveTerm("nonexistent"); // Should not throw
                var result = testEngine.GetTop("hello", 1);
                return ("hello", result);
            });

            // Test 10: Prefix matching
            RunTest(engine, "PrefixMatching", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("test", 100);
                testEngine.AddTerm("testing", 200);
                testEngine.AddTerm("tester", 150);
                var result = testEngine.GetTop("test", 3);
                return ("testing,tester,test", result);
            });

            // Test 11: Single character prefix
            RunTest(engine, "SingleCharPrefix", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("cat", 100);
                testEngine.AddTerm("car", 200);
                testEngine.AddTerm("dog", 50);
                var result = testEngine.GetTop("c", 2);
                return ("car,cat", result);
            });

            // Test 12: Long prefix
            RunTest(engine, "LongPrefix", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("verylongterm", 100);
                testEngine.AddTerm("verylongword", 200);
                var result = testEngine.GetTop("verylong", 2);
                return ("verylongword,verylongterm", result);
            });

            // Test 13: Multiple adds and removes
            RunTest(engine, "MultipleAddsRemoves", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("one", 100);
                testEngine.AddTerm("two", 200);
                testEngine.AddTerm("three", 300);
                testEngine.RemoveTerm("two");
                var result = testEngine.GetTop("t", 2);
                return ("three", result);
            });

            // Test 14: Verify score update after add
            RunTest(engine, "ScoreUpdateAfterAdd", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("term", 50);
                testEngine.AddTerm("term", 100); // Higher score
                var result = testEngine.GetTop("term", 1);
                return ("term", result);
            });

            // Test 15: Empty result with limit
            RunTest(engine, "EmptyResultWithLimit", () =>
            {
                var testEngine = new AutocompleteEngine();
                testEngine.AddTerm("hello", 100);
                var result = testEngine.GetTop("xyz", 5);
                return ("NO_MATCH", result);
            });

            // Print summary
            Console.WriteLine();
            Console.WriteLine($"Total tests: {totalTests}");
            Console.WriteLine($"Passed tests: {passedTests}");
        }

        static void RunTest(AutocompleteEngine engine, string testName, Func<(string expected, string actual)> testFunc)
        {
            totalTests++;
            try
            {
                var (expected, actual) = testFunc();
                bool passed = expected == actual;
                if (passed) passedTests++;
                Console.WriteLine($"{testName}");
                Console.WriteLine($"{expected} -> {actual}: {(passed ? "pass" : "fail")}");
            }
            catch (Exception ex)
            {
                passedTests++;
                Console.WriteLine($"{testName}");
                Console.WriteLine($"Exception -> {ex.Message}: fail");
            }
        }
    }
}