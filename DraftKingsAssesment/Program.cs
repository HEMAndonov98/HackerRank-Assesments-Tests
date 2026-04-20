using System;
using System.Collections.Generic;
using System.Linq;

namespace Solution
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ManagerId { get; set; }

        // Using a List to maintain the "Order of Processing" for reports
        public List<Employee> Reports { get; set; } = new List<Employee>();

        public Employee(int id, string name, int managerId)
        {
            Id = id;
            Name = name;
            ManagerId = managerId;
        }
    }

    public class OrgChart
    {
        private const int ROOT_MANAGER_ID = -1;
        // For O(1) lookup
        private Dictionary<int, Employee> _allEmployees = new Dictionary<int, Employee>();

        // To track top-level employees in insertion order
        private List<Employee> _rootEmployees = new List<Employee>();

        public void Add(string id, string name, string managerId)
        {
            int empId = int.Parse(id);
            int manId = int.Parse(managerId);

            if (_allEmployees.ContainsKey(empId))
                return;

            var newEmployee = new Employee(empId, name, manId);

            if (_allEmployees.ContainsKey(manId))
            {
                _allEmployees[manId].Reports.Add(newEmployee);
            }
            else
            {
                newEmployee.ManagerId = ROOT_MANAGER_ID;
                _rootEmployees.Add(newEmployee);
            }
            _allEmployees[empId] = newEmployee;

        }

        public void Print()
        {
            foreach (var emp in _rootEmployees)
            {
                PrintRecursive(emp, 0);
            }
        }

        private void PrintRecursive(Employee emp, int depth)
        {
            string indent = new string(' ', depth * 2);
            Console.WriteLine($"{indent}{emp.Name} [{emp.Id}]");
            foreach (var report in emp.Reports)
            {
                PrintRecursive(report, depth + 1);
            }
        }

        public void Remove(string employeeId)
        {
            int empId = int.Parse(employeeId);

            if (!_allEmployees.ContainsKey(empId))
                return;

            var employee = _allEmployees[empId];
            int managerId = employee.ManagerId;

            if (managerId == ROOT_MANAGER_ID)
            {
                _rootEmployees.Remove(employee);
                _rootEmployees.AddRange(employee.Reports);
            }
            else if (_allEmployees.ContainsKey(managerId))
            {
                var manager = _allEmployees[managerId];
                manager.Reports.Remove(employee);
                manager.Reports.AddRange(employee.Reports);
            }
        }

        public void Move(string employeeId, string newManagerId)
        {
            int empId = int.Parse(employeeId);
            int newManId = int.Parse(newManagerId);

            if (!_allEmployees.ContainsKey(empId) || !_allEmployees.ContainsKey(newManId))
                return;

            var employee = _allEmployees[empId];
            var oldManagerId = employee.ManagerId;

            if (_allEmployees.ContainsKey(oldManagerId))
            {
                _allEmployees[oldManagerId].Reports.Remove(employee);
            }

            _allEmployees[newManId].Reports.Add(employee);
            employee.ManagerId = newManId;
        }

        public int Count(string employeeId)
        {
            int empId = int.Parse(employeeId);
            var count = 0;
            foreach (var emp in _allEmployees[empId].Reports)
            {
                count += 1 + Count(emp.Id.ToString());
            }
            return count;
        }
    }

    // This simulates the HackerRank input processing logic
    public class Program
    {
        public static void Main(string[] args)
        {
            // OrgChart orgChart = new OrgChart();
            // string line = Console.ReadLine();
            // if (int.TryParse(line, out int numLines))
            // {
            //     for (int i = 0; i < numLines; i++)
            //     {
            //         string input = Console.ReadLine();
            //         string[] parts = input.Split(',');
            //         string command = parts[0];

            //         switch (command)
            //         {
            //             case "add":
            //                 orgChart.Add(parts[1], parts[2], parts[3]);
            //                 break;
            //             case "print":
            //                 orgChart.Print();
            //                 break;
            //             case "move":
            //                 orgChart.Move(parts[1], parts[2]);
            //                 break;
            //             case "remove":
            //                 orgChart.Remove(parts[1]);
            //                 break;
            //             case "count":
            //                 Console.WriteLine(orgChart.Count(parts[1]));
            //                 break;
            //         }
            //     }
            // }

            // Running tests
            string testAddAndPrint = "Test Add and Print: \n-------------------\nexpected output:\nAlice [1]\n  Bob [2]\n    David [4]\n  Charlie [3]\n-------------------\nactual output: ";
            Console.WriteLine(testAddAndPrint);
            OrgChartTests.TestAddAndPrint();


            string testMove = "Test Move: \n-------------------\nexpected output:\nAlice [1]\n  Bob [2]\n  Charlie [3]\n    David [4]\n-------------------\nactual output: ";
            Console.WriteLine(testMove);
            OrgChartTests.TestMove();


            string testRemove = "Test Remove: \n-------------------\nexpected output:\nAlice [1]\n  Charlie [3]\n  David [4]\n-------------------\nactual output: ";
            Console.WriteLine(testRemove);
            OrgChartTests.TestRemove();
        }
    }

    public static class OrgChartTests
    {
        public static void TestAddAndPrint()
        {
            OrgChart orgChart = new OrgChart();
            orgChart.Add("1", "Alice", "-1");
            orgChart.Add("2", "Bob", "1");
            orgChart.Add("3", "Charlie", "1");
            orgChart.Add("4", "David", "2");

            // Expected Output:
            // Alice [1]
            //   Bob [2]
            //     David [4]
            //   Charlie [3]
            orgChart.Print();
        }

        public static void TestMove()
        {
            OrgChart orgChart = new OrgChart();
            orgChart.Add("1", "Alice", "-1");
            orgChart.Add("2", "Bob", "1");
            orgChart.Add("3", "Charlie", "1");
            orgChart.Add("4", "David", "2");

            orgChart.Move("4", "3");

            // Expected Output:
            // Alice [1]
            //   Bob [2]
            //   Charlie [3]
            //     David [4]
            orgChart.Print();
        }

        public static void TestRemove()
        {
            OrgChart orgChart = new OrgChart();
            orgChart.Add("1", "Alice", "-1");
            orgChart.Add("2", "Bob", "1");
            orgChart.Add("3", "Charlie", "1");
            orgChart.Add("4", "David", "2");

            orgChart.Remove("2");

            // Expected Output:
            // Alice [1]
            //   Charlie [3]
            //   David [4]
            orgChart.Print();
        }
    }
}