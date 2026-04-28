using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace SofiaCache
{
    // HINT: To achieve O(1) time complexity for moving items to the "front" of the line,
    // a standard List<T> or Array will not work (removing from the middle of a List is O(N)).
    // You will need a Doubly Linked List.
    public class CacheNode
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public CacheNode Prev { get; set; }
        public CacheNode Next { get; set; }

        public CacheNode(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public class LRUCache
    {
        private int _capacity;

        // Dictionary provides O(1) lookups.
        private Dictionary<string, CacheNode> _cache;

        // Dummy head and tail nodes make adding/removing from the linked list MUCH easier
        // because you don't have to constantly check for null reference edge cases.
        private CacheNode _head;
        private CacheNode _tail;

        public LRUCache(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<string, CacheNode>();

            // Setup dummy nodes
            _head = new CacheNode("HEAD", "HEAD");
            _tail = new CacheNode("TAIL", "TAIL");
            _head.Next = _tail;
            _tail.Prev = _head;
        }

        public void Put(string key, string value)
        {
            // TODO: Implementation
            // 1. If key exists, update value and move to front.
            if (_cache.ContainsKey(key))
            {
                var existingNode = _cache[key];
                existingNode.Value = value;

                MoveToFront(existingNode);
            }


            // 2. If new key, check capacity. 
            if (!_cache.ContainsKey(key) && _cache.Count + 1 <= _capacity)
            {
                var newNode = new CacheNode(key, value);
                _cache[key] = newNode;

                AddToFront(newNode);
            }
            // 3. If full, remove from tail AND dictionary.

            if (!_cache.ContainsKey(key) && _cache.Count + 1 > _capacity)
            {
                var nodeToRemove = _tail.Prev;

                RemoveNode(nodeToRemove);
                _cache.Remove(nodeToRemove.Key);

                // 4. Insert new node at head AND dictionary.

                var newNode = new CacheNode(key, value);
                _cache[key] = newNode;
                AddToFront(newNode);

            }
        }

        public string Get(string key)
        {
            // TODO: Implementation
            // 1. If not found, return "NOT_FOUND"
            if (!_cache.ContainsKey(key))
            {
                return "NOT_FOUND";
            }
            // 2. If found, move node to front of the linked list.
            var foundNode = _cache[key];
            MoveToFront(foundNode);

            // 3. Return value.
            return foundNode.Value;
        }

        public string GetStatus()
        {
            // TODO: Implementation
            if (_cache.Count == 0)
            {
                return "EMPTY";
            }
            // Traverse from head.Next to tail.Prev and collect keys.
            var NodeList = new List<string>();
            var currentNode = _head.Next;

            while (currentNode.Value != "TAIL")
            {
                NodeList.Add(currentNode.Key);
                currentNode = currentNode.Next;
            }

            return string.Join(",", NodeList);
        }

        // --- Helper Methods ---
        // Writing these two helpers will make your Put and Get logic 10x cleaner.
        private void RemoveNode(CacheNode node)
        {
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
        }

        private void AddToFront(CacheNode node)
        {
            node.Next = _head.Next;
            node.Prev = _head;

            //work with LLM to understand if this line is redundant
            _head.Next.Prev = node;
            _head.Next = node;
        }

        private void AddToEnd(CacheNode node)
        {
            node.Prev = _tail.Prev;
            node.Next = _tail;

            _tail.Prev.Next = node;
            _tail.Prev = node;
        }

        private void MoveToFront(CacheNode node)
        {
            RemoveNode(node);
            AddToFront(node);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LRUCache cache = null;

            string[] commands = new string[]
            {
                "init,3",
                "put,A,100",
                "put,B,200",
                "put,C,300",
                "status",
                "get,B",
                "status",
                "put,D,400",
                "status",
                "get,A",
                "put,C,350",
                "status"
            };

            foreach (var line in commands)
            {
                var parts = line.Split(',');
                var command = parts[0];

                switch (command)
                {
                    case "init":
                        cache = new LRUCache(int.Parse(parts[1]));
                        break;
                    case "put":
                        cache.Put(parts[1], parts[2]);
                        break;
                    case "get":
                        Console.WriteLine(cache.Get(parts[1]));
                        break;
                    case "status":
                        Console.WriteLine(cache.GetStatus());
                        break;
                }
            }

            // Run comprehensive tests
            Console.WriteLine("\n=== COMPREHENSIVE TESTS ===\n");
            RunTests();
        }

        static void RunTests()
        {
            int passed = 0;
            int failed = 0;

            // Test 1: Basic Put and Get
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("X", "10");
                string result = cache.Get("X");
                string expected = "10";
                bool pass = result == expected;
                Console.WriteLine($"Test 1 - Basic Put/Get: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 2: Get non-existent key
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("X", "10");
                string result = cache.Get("Y");
                string expected = "NOT_FOUND";
                bool pass = result == expected;
                Console.WriteLine($"Test 2 - Get non-existent: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 3: Update existing key
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("X", "10");
                cache.Put("X", "20");
                string result = cache.Get("X");
                string expected = "20";
                bool pass = result == expected;
                Console.WriteLine($"Test 3 - Update existing key: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 4: Cache eviction - LRU removed
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("A", "100");
                cache.Put("B", "200");
                cache.Put("C", "300");
                cache.Put("D", "400"); // Should evict A
                string result = cache.Get("A");
                string expected = "NOT_FOUND";
                bool pass = result == expected;
                Console.WriteLine($"Test 4 - Eviction LRU: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 5: Status shows keys in MRU->LRU order
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("A", "100");
                cache.Put("B", "200");
                cache.Put("C", "300");
                string result = cache.GetStatus();
                string expected = "C,B,A";
                bool pass = result == expected;
                Console.WriteLine($"Test 5 - Status order: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 6: Get moves to front (MRU)
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("A", "100");
                cache.Put("B", "200");
                cache.Put("C", "300");
                cache.Get("A"); // Access A, should move to front
                string result = cache.GetStatus();
                string expected = "A,C,B";
                bool pass = result == expected;
                Console.WriteLine($"Test 6 - Get moves to front: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 7: Put on existing key moves to front
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("A", "100");
                cache.Put("B", "200");
                cache.Put("C", "300");
                cache.Put("A", "150"); // Update A, should move to front
                string result = cache.GetStatus();
                string expected = "A,C,B";
                bool pass = result == expected;
                Console.WriteLine($"Test 7 - Put update moves to front: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 8: Empty cache status
            {
                LRUCache cache = new LRUCache(3);
                string result = cache.GetStatus();
                string expected = "EMPTY";
                bool pass = result == expected;
                Console.WriteLine($"Test 8 - Empty cache status: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 9: Capacity of 1
            {
                LRUCache cache = new LRUCache(1);
                cache.Put("A", "100");
                cache.Put("B", "200"); // Should evict A
                string result1 = cache.Get("A");
                string result2 = cache.Get("B");
                bool pass = result1 == "NOT_FOUND" && result2 == "200";
                Console.WriteLine($"Test 9 - Capacity 1: A='NOT_FOUND', B='200' => Actual A='{result1}', B='{result2}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 10: Multiple evictions
            {
                LRUCache cache = new LRUCache(2);
                cache.Put("A", "100");
                cache.Put("B", "200");
                cache.Put("C", "300"); // Evicts A
                cache.Put("D", "400"); // Evicts B
                cache.Put("E", "500"); // Evicts C
                string result = cache.GetStatus();
                string expected = "E,D";
                bool pass = result == expected;
                Console.WriteLine($"Test 10 - Multiple evictions: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 11: Get after eviction
            {
                LRUCache cache = new LRUCache(2);
                cache.Put("A", "100");
                cache.Put("B", "200");
                cache.Put("C", "300"); // Evicts A
                string result = cache.Get("B");
                string expected = "200";
                bool pass = result == expected;
                Console.WriteLine($"Test 11 - Get after eviction: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            // Test 12: Put moves accessed node to front
            {
                LRUCache cache = new LRUCache(3);
                cache.Put("A", "100");
                cache.Put("B", "200");
                cache.Put("C", "300");
                cache.Get("B"); // B becomes MRU
                cache.Put("D", "400"); // Should evict A (LRU), not C
                string result = cache.Get("C");
                string expected = "300"; // C should still exist - A was evicted, not C
                bool pass = result == expected;
                Console.WriteLine($"Test 12 - Eviction after get: Expected='{expected}', Actual='{result}' => {(pass ? "PASS" : "FAIL")}");
                if (pass) passed++; else failed++;
            }

            Console.WriteLine($"\n=== SUMMARY: {passed} PASSED, {failed} FAILED ===");
        }
    }
}