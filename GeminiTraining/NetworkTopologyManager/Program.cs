using System;
using System.Collections.Generic;

namespace SofiaConnect
{
    public class Router
    {
        public string Id { get; set; }
        public string Model { get; set; }

        public Router(string id, string model)
        {
            Id = id;
            Model = model;
        }

        public Dictionary<string, Router> Connections { get; set; } = new Dictionary<string, Router>();
    }

    public class NetworkManager
    {
        public Dictionary<string, Router> routers { get; set; } = new Dictionary<string, Router>();

        public void AddRouter(string id, string model)
        {
            if (!routers.ContainsKey(id))
            {
                var router = new Router(id, model);
                routers[id] = router;
            }
        }

        public void Connect(string id1, string id2)
        {
            if (routers.ContainsKey(id1) && routers.ContainsKey(id2))
            {
                var router1 = routers[id1];
                var router2 = routers[id2];

                if (!router1.Connections.ContainsKey(id2) && !router2.Connections.ContainsKey(id1))
                {
                    router1.Connections[id2] = router2;
                    router2.Connections[id1] = router1;
                }
            }
        }

        public void Maintenance(string id)
        {
            if (routers.ContainsKey(id))
                return;

            var routerToRemove = routers[id];

            routers.Remove(id);
            foreach (var connection in routerToRemove.Connections)
            {
                connection.Value.Connections.Remove(id);
            }
        }

        public bool TraceRoute(string startId, string endId)
        {
            if (!routers.ContainsKey(startId) || !routers.ContainsKey(endId))
                return false;

            var startRouter = routers[startId];
            var visited = new HashSet<string>();
            var queue = new Queue<Router>();
            queue.Enqueue(startRouter);

            while (queue.Count > 0)
            {
                var currentRouter = queue.Dequeue();
                if (currentRouter.Id == endId)
                    return true;

                visited.Add(currentRouter.Id);
                foreach (var connection in currentRouter.Connections)
                {
                    if (!visited.Contains(connection.Key))
                    {
                        queue.Enqueue(connection.Value);
                    }
                }
            }

            return false;
        }

        public int GetNetworkSize(string startId)
        {
            if (!routers.ContainsKey(startId))
                return 0;

            var startRouter = routers[startId];
            var visited = new HashSet<string>();
            var queue = new Queue<Router>();

            queue.Enqueue(startRouter);
            visited.Add(startRouter.Id);

            while (queue.Count > 0)
            {
                var currentRouter = queue.Dequeue();
                foreach (var connection in currentRouter.Connections)
                {
                    if (!visited.Contains(connection.Key))
                    {
                        visited.Add(connection.Key);
                        queue.Enqueue(connection.Value);
                    }
                }
            }

            return visited.Count;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            NetworkManager manager = new NetworkManager();

            string[] commands = new string[]
            {
                "add_router,1,Cisco-A",
                "add_router,2,Juniper-B",
                "add_router,3,Nokia-C",
                "connect,1,2",
                "trace,1,3",
                "connect,2,3",
                "trace,1,3",
                "stats,1"
            };

            foreach (var line in commands)
            {
                var parts = line.Split(',');
                var command = parts[0];

                switch (command)
                {
                    case "add_router":
                        manager.AddRouter(parts[1], parts[2]);
                        break;

                    case "connect":
                        manager.Connect(parts[1], parts[2]);
                        break;

                    case "maintenance":
                        manager.Maintenance(parts[1]);
                        break;

                    case "trace":
                        bool pathExists = manager.TraceRoute(parts[1], parts[2]);
                        Console.WriteLine(pathExists ? "Path Exists" : "No Path");
                        break;

                    case "stats":
                        int networkSize = manager.GetNetworkSize(parts[1]);
                        Console.WriteLine(networkSize);
                        break;
                }
            }
        }
    }
}