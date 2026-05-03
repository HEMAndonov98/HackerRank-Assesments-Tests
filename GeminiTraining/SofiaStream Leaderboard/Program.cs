using System;
using System.Collections.Generic;
using System.IO.Compression;
using SofiaStream;

// Example Test Case
Leaderboard lb = new Leaderboard(3);
lb.AddScore("PlayerA", 100);
lb.AddScore("PlayerB", 500);
lb.AddScore("PlayerC", 200);

// Should show: PlayerB:500, PlayerC:200, PlayerA:100
Console.WriteLine(string.Join(", ", lb.GetTop()));

lb.AddScore("PlayerD", 300);
// PlayerA (100) should be gone now.
// Should show: PlayerB:500, PlayerD:300, PlayerC:200
Console.WriteLine(string.Join(", ", lb.GetTop()));


Console.WriteLine("Delete Player Test \n ------------------");
lb = new Leaderboard(3);
lb.AddScore("PlayerA", 100);
lb.AddScore("PlayerB", 500);
lb.AddScore("PlayerC", 200);
Console.WriteLine("After adding result");
Console.WriteLine(string.Join(", ", lb.GetTop()));
lb.DeletePlayer("PlayerC");
Console.WriteLine("Result after deletion");
Console.WriteLine(string.Join(", ", lb.GetTop()));

namespace SofiaStream
{
    public class Player
    {
        public string Name { get; set; }
        public int Score { get; set; }

        public Player(string name, int score)
        {
            Name = name;
            Score = score;
        }

        override public string ToString()
        {
            return $"{Name}:{Score}";
        }
    }
    public class Leaderboard
    {
        private Player[] players;
        private int size;

        private Dictionary<string, int> positionMap = new Dictionary<string, int>();

        private int GetParentIndex(int childIndex) => (childIndex - 1) / 2;
        private int GetLeftChildIndex(int parentIndex) => (2 * parentIndex) + 1;
        private int GetRightChildIndex(int parentIndex) => (2 * parentIndex) + 2;

        private int GetLastPlayerIndex() => size - 1;

        private Player GetLastPlayer() => players[size - 1];
        private Player GetParent(int childIndex) => players[GetParentIndex(childIndex)];
        private Player GetLeftChild(int parentIndex) => players[GetLeftChildIndex(parentIndex)];
        private Player GetRightChild(int parentIndex) => players[GetRightChildIndex(parentIndex)];

        private void SetPlayerInMap(string name, int arrPosition)
        {
            positionMap[name] = arrPosition;
        }

        private void HeapifyUp(int index)
        {
            //While the current node is not the root and its score is less than its parent's score, swap it with its parent
            while (index > 0 && GetParent(index).Score > players[index].Score)
            {
                Swap(index, GetParentIndex(index));
                index = GetParentIndex(index);
            }
        }

        private void HeapifyDown(int index = 0)
        {
            //While the current node has at least one child
            while (GetLeftChildIndex(index) < size)
            {
                //Assume the smaller child is the left child
                int smallerChildIndex = GetLeftChildIndex(index);

                //If the right child exists and is smaller than the left child, update the smaller child index
                if (GetRightChildIndex(index) < size && GetRightChild(index).Score < GetLeftChild(index).Score)
                {
                    smallerChildIndex = GetRightChildIndex(index);
                }

                //If the current node's score is less than or equal to the smaller child's score, the heap property is satisfied, so break
                if (players[index].Score <= players[smallerChildIndex].Score)
                {
                    break;
                }

                //Otherwise, swap the current node with the smaller child and update the index to continue heapifying down
                Swap(index, smallerChildIndex);
                index = smallerChildIndex;
            }
        }


        private void Swap(int indexOne, int indexTwo)
        {
            Player temp = players[indexOne];
            players[indexOne] = players[indexTwo];
            players[indexTwo] = temp;

            SetPlayerInMap(players[indexOne].Name, indexOne);
            SetPlayerInMap(players[indexTwo].Name, indexTwo);
        }

        public Leaderboard(int k)
        {
            players = new Player[k];
            size = 0;
        }

        public void AddScore(string playerName, int score)
        {
            if (positionMap.TryGetValue(playerName, out var existingIndex))
            {
                var existingPlayer = players[existingIndex];
                existingPlayer.Score = score;

                HeapifyUp(existingIndex);
                HeapifyDown(existingIndex);

                return;
            }


            Player newPlayer = new Player(playerName, score);


            if (size < players.Length)
            {
                int Lastindex = size;
                players[Lastindex] = newPlayer;
                SetPlayerInMap(playerName, Lastindex);

                HeapifyUp(size);
                size++;

            }
            else
            {
                var minPlayer = PeekMin();
                if (minPlayer != null && minPlayer.Score < score)
                {
                    positionMap.Remove(players[0].Name);

                    players[0] = newPlayer;
                    SetPlayerInMap(playerName, 0);

                    HeapifyDown();
                }
            }
        }

        public void DeletePlayer(string playerName)
        {
            if (!positionMap.TryGetValue(playerName, out int index))
                return;

            positionMap.Remove(playerName);

            if (index == size - 1)
            {
                players[index] = null;
            }
            else
            {
                var lastPlayer = GetLastPlayer();
                players[index] = lastPlayer;

                SetPlayerInMap(lastPlayer.Name, index);
                players[GetLastPlayerIndex()] = null;

                HeapifyUp(index);
                HeapifyDown(index);
            }

            size--;

        }

        public List<string> GetTop()
        {
            var copy = new Player[size];
            Array.Copy(players, 0, copy, 0, size);
            Array.Sort(copy, (a, b) => b.Score.CompareTo(a.Score)); // Sort in descending order
            List<string> result = new List<string>();
            foreach (var player in copy)
            {
                result.Add(player.ToString());
            }

            return result;

        }
        private Player? PeekMin() => size > 0 ? players[0] : null;
    }
}