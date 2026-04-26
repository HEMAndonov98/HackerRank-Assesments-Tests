namespace SofiaBuild
{
    public class TaskItem
    {
        public string Id { get; set; }

        // Hint: In a directed graph, it is often useful to track 
        // who you depend on AND who depends on you.
        public Dictionary<string, TaskItem> Prerequisites { get; set; } = new Dictionary<string, TaskItem>();
        public Dictionary<string, TaskItem> Dependents { get; set; } = new Dictionary<string, TaskItem>();

        public int NumberOfPrerequisites { get; set; }

        public TaskItem(string id)
        {
            Id = id;
        }
    }

    public class PipelineManager
    {
        private Dictionary<string, TaskItem> _tasks = new Dictionary<string, TaskItem>();

        public void RegisterTask(string id)
        {
            if (_tasks.ContainsKey(id))
                return;
            var newTask = new TaskItem(id);
            _tasks[id] = newTask;
        }

        public void AddDependency(string targetId, string prerequisiteId)
        {
            if (!_tasks.ContainsKey(targetId) || !_tasks.ContainsKey(prerequisiteId))
                return;

            var dependentTask = _tasks[targetId];
            var parentTask = _tasks[prerequisiteId];

            parentTask.Dependents[dependentTask.Id] = dependentTask;
            dependentTask.Prerequisites[parentTask.Id] = parentTask;
            dependentTask.NumberOfPrerequisites++;
        }

        public string GetRunOrder()
        {
            var inDegree = _tasks.Values.ToDictionary(n => n, n => n.NumberOfPrerequisites);
            var queue = new Queue<TaskItem>();

            foreach (var taskItem in _tasks.Values)
            {
                if (inDegree[taskItem] == 0)
                {
                    queue.Enqueue(taskItem);
                }
            }

            List<string> result = new List<string>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current.Id);

                foreach (var dependent in current.Dependents.Values)
                {
                    inDegree[dependent]--;

                    if (inDegree[dependent] == 0)
                    {
                        queue.Enqueue(dependent);
                    }
                }
            }

            if (result.Count != _tasks.Count)
            {
                return "CYCLE_DETECTED";
            }

            return string.Join(",", result);
        }

        public int GetBlastRadius(string id)
        {
            if (!_tasks.ContainsKey(id))
                return 0;

            int count = 0;

            var queue = new Queue<TaskItem>();
            var visited = new HashSet<TaskItem>();

            queue.Enqueue(_tasks[id]);
            visited.Add(_tasks[id]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var dep in current.Dependents.Values)
                {
                    if (visited.Add(dep))
                    {
                        count++;
                        queue.Enqueue(dep);
                    }
                }
            }

            return count;
        }

        public int GetBlastRadiusRec(string id)
        {
            if (!_tasks.ContainsKey(id))
                return 0;

            var visited = new HashSet<TaskItem>();
            return DFS(_tasks[id], visited);
        }

        private int DFS(TaskItem task, HashSet<TaskItem> visited)
        {
            int count = 0;
            visited.Add(task);

            foreach (var dep in task.Dependents.Values)
            {
                if (visited.Add(dep))
                {
                    count++;
                    count += DFS(dep, visited);
                }
            }


            return count;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== CI/CD Pipeline Resolver Tests ===\n");

            // Test 1: Basic functionality from description
            RunTest("Test 1: Basic functionality", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("Compile");
                manager.RegisterTask("Test");
                manager.RegisterTask("Deploy");
                manager.AddDependency("Test", "Compile");
                manager.AddDependency("Deploy", "Test");

                var runOrder = manager.GetRunOrder();
                var blastRadius = manager.GetBlastRadius("Compile");

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: Compile,Test,Deploy");
                Console.WriteLine($"  Actual:   {runOrder}");
                Console.WriteLine($"  Match:    {runOrder == "Compile,Test,Deploy"}");
                Console.WriteLine();
                Console.WriteLine($"  blast_radius,Compile: {blastRadius}");
                Console.WriteLine($"  Expected: 2");
                Console.WriteLine($"  Actual:   {blastRadius}");
                Console.WriteLine($"  Match:    {blastRadius == 2}");
            });

            // Test 2: Duplicate registration (should ignore)
            RunTest("Test 2: Duplicate registration", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.RegisterTask("A"); // duplicate
                manager.RegisterTask("B");
                manager.AddDependency("B", "A");

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: A,B");
                Console.WriteLine($"  Actual:   {runOrder}");
                Console.WriteLine($"  Match:    {runOrder == "A,B"}");
            });

            // Test 3: Non-existent task dependency (should ignore)
            RunTest("Test 3: Non-existent task dependency", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.AddDependency("A", "NonExistent"); // should be ignored
                manager.AddDependency("NonExistent", "A"); // should be ignored

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: A");
                Console.WriteLine($"  Actual:   {runOrder}");
                Console.WriteLine($"  Match:    {runOrder == "A"}");
            });

            // Test 4: Self-dependency (should be ignored or handled)
            RunTest("Test 4: Self-dependency", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.AddDependency("A", "A"); // self dependency

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: A (self-dep ignored)");
                Console.WriteLine($"  Actual:   {runOrder}");
                Console.WriteLine($"  Match:    {runOrder == "A"}");
            });

            // Test 5: Cycle detection
            RunTest("Test 5: Cycle detection", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.RegisterTask("B");
                manager.RegisterTask("C");
                manager.AddDependency("B", "A");
                manager.AddDependency("C", "B");
                manager.AddDependency("A", "C"); // creates cycle A->B->C->A

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: CYCLE_DETECTED");
                Console.WriteLine($"  Actual:   {runOrder}");
                Console.WriteLine($"  Match:    {runOrder == "CYCLE_DETECTED"}");
            });

            // Test 6: Multiple independent chains
            RunTest("Test 6: Multiple independent chains", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A1");
                manager.RegisterTask("A2");
                manager.RegisterTask("B1");
                manager.RegisterTask("B2");
                manager.AddDependency("A2", "A1");
                manager.AddDependency("B2", "B1");

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: A1,B1,A2,B2 (or similar valid order)");
                Console.WriteLine($"  Actual:   {runOrder}");
                // Just verify it contains all tasks
                var valid = runOrder.Split(',').OrderBy(x => x).SequenceEqual(new[] { "A1", "A2", "B1", "B2" }.OrderBy(x => x));
                Console.WriteLine($"  Match:    {valid}");
            });

            // Test 7: Diamond dependency
            RunTest("Test 7: Diamond dependency", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.RegisterTask("B");
                manager.RegisterTask("C");
                manager.RegisterTask("D");
                manager.AddDependency("B", "A");
                manager.AddDependency("C", "A");
                manager.AddDependency("D", "B");
                manager.AddDependency("D", "C");

                var runOrder = manager.GetRunOrder();
                var blastRadius = manager.GetBlastRadius("A");

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  blast_radius,A: {blastRadius}");
                Console.WriteLine($"  Expected: 3 (B, C, D)");
                Console.WriteLine($"  Actual:   {blastRadius}");
                Console.WriteLine($"  Match:    {blastRadius == 3}");
            });

            // Test 8: Blast radius on leaf node
            RunTest("Test 8: Blast radius on leaf node", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.RegisterTask("B");
                manager.AddDependency("B", "A");

                var blastRadius = manager.GetBlastRadius("B");

                Console.WriteLine($"  blast_radius,B: {blastRadius}");
                Console.WriteLine($"  Expected: 0 (leaf node)");
                Console.WriteLine($"  Actual:   {blastRadius}");
                Console.WriteLine($"  Match:    {blastRadius == 0}");
            });

            // Test 9: Non-existent task blast radius
            RunTest("Test 9: Non-existent task blast radius", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");

                var blastRadius = manager.GetBlastRadius("NonExistent");

                Console.WriteLine($"  blast_radius,NonExistent: {blastRadius}");
                Console.WriteLine($"  Expected: 0");
                Console.WriteLine($"  Actual:   {blastRadius}");
                Console.WriteLine($"  Match:    {blastRadius == 0}");
            });

            // Test 10: Empty pipeline
            RunTest("Test 10: Empty pipeline", () =>
            {
                var manager = new PipelineManager();

                var runOrder = manager.GetRunOrder();
                var blastRadius = manager.GetBlastRadius("Anything");

                Console.WriteLine($"  run_order: '{runOrder}'");
                Console.WriteLine($"  Expected: '' (empty)");
                Console.WriteLine($"  Actual:   '{runOrder}'");
                Console.WriteLine($"  Match:    {runOrder == ""}");
                Console.WriteLine();
                Console.WriteLine($"  blast_radius,Anything: {blastRadius}");
                Console.WriteLine($"  Expected: 0");
                Console.WriteLine($"  Actual:   {blastRadius}");
                Console.WriteLine($"  Match:    {blastRadius == 0}");
            });

            // Test 11: Complex multi-level dependencies
            RunTest("Test 11: Complex multi-level", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("Init");
                manager.RegisterTask("Compile");
                manager.RegisterTask("Test");
                manager.RegisterTask("Package");
                manager.RegisterTask("Deploy");
                manager.AddDependency("Compile", "Init");
                manager.AddDependency("Test", "Compile");
                manager.AddDependency("Package", "Test");
                manager.AddDependency("Deploy", "Package");

                var runOrder = manager.GetRunOrder();
                var blastInit = manager.GetBlastRadius("Init");
                var blastTest = manager.GetBlastRadius("Test");

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  blast_radius,Init: {blastInit}");
                Console.WriteLine($"  Expected: 4");
                Console.WriteLine($"  Actual:   {blastInit}");
                Console.WriteLine($"  Match:    {blastInit == 4}");
                Console.WriteLine();
                Console.WriteLine($"  blast_radius,Test: {blastTest}");
                Console.WriteLine($"  Expected: 2");
                Console.WriteLine($"  Actual:   {blastTest}");
                Console.WriteLine($"  Match:    {blastTest == 2}");
            });

            // Test 12: Register after dependencies (should still work)
            RunTest("Test 12: Register after dependencies", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.AddDependency("B", "A"); // B doesn't exist yet
                manager.RegisterTask("B"); // Register B after dependency

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: A,B");
                Console.WriteLine($"  Actual:   {runOrder}");
                Console.WriteLine($"  Match:    {runOrder == "A,B"}");
            });

            // Test 13: Self cycle (A depends on A)
            RunTest("Test 13: Self cycle", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.AddDependency("A", "A");

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: CYCLE_DETECTED or A");
                Console.WriteLine($"  Actual:   {runOrder}");
            });

            // Test 14: Two independent tasks
            RunTest("Test 14: Two independent tasks", () =>
            {
                var manager = new PipelineManager();
                manager.RegisterTask("A");
                manager.RegisterTask("B");

                var runOrder = manager.GetRunOrder();

                Console.WriteLine($"  run_order: {runOrder}");
                Console.WriteLine($"  Expected: A,B or B,A");
                Console.WriteLine($"  Actual:   {runOrder}");
                var valid = runOrder.Split(',').OrderBy(x => x).SequenceEqual(new[] { "A", "B" }.OrderBy(x => x));
                Console.WriteLine($"  Match:    {valid}");
            });

            // Test 15: Full example from description
            RunTest("Test 15: Full example from description", () =>
            {
                var manager = new PipelineManager();

                string[] commands = new string[]
                {
                    "register,Compile",
                    "register,Test",
                    "register,Deploy",
                    "depend,Test,Compile",
                    "depend,Deploy,Test",
                    "run_order",
                    "blast_radius,Compile",
                    "register,Lint",
                    "depend,Compile,Lint",
                    "run_order",
                    "depend,Lint,Deploy",
                    "run_order"
                };

                Console.WriteLine("  Running commands:");
                foreach (var line in commands)
                {
                    var parts = line.Split(',');
                    var command = parts[0];
                    string result = "";

                    switch (command)
                    {
                        case "register":
                            manager.RegisterTask(parts[1]);
                            result = $"(registered {parts[1]})";
                            break;
                        case "depend":
                            manager.AddDependency(parts[1], parts[2]);
                            result = $"(depend {parts[1]} on {parts[2]})";
                            break;
                        case "run_order":
                            result = manager.GetRunOrder();
                            break;
                        case "blast_radius":
                            result = manager.GetBlastRadius(parts[1]).ToString();
                            break;
                    }
                    Console.WriteLine($"    {line} -> {result}");
                }
            });

            Console.WriteLine("\n=== Tests Complete ===");
        }

        static void RunTest(string testName, Action testAction)
        {
            Console.WriteLine($"\n--- {testName} ---");
            try
            {
                testAction();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: {ex.Message}");
            }
        }
    }
}