# 🧩 Project SofiaStream: The Top-K Leaderboard

## 📖 Problem Description

You are building a **live leaderboard** for a gaming platform. Instead of a search bar, this system needs to track the **top K highest-scoring players in real-time** as millions of scores pour in.

---

## ⚠️ The Challenge

You cannot use built-in sorting methods like `List.Sort()` because sorting a million players after every update is **O(N log N)**, which will cause performance issues.

Instead, you must implement a **Min-Heap from scratch** to keep track of only the top K players in **O(log K)** time.

---

## 📌 Requirements

### 1. `init,<K>`

Initializes the leaderboard with size `K`.

---

### 2. `add,<player_name>,<score>`

Adds a player's score:

- If the leaderboard has fewer than `K` players → add the player
- If the leaderboard is full:
    - Only add the player if their score is higher than the current K-th place (minimum score in the heap)
    - If added, remove the current lowest-ranked player

---

### 3. `get_top`

Returns the current leaderboard:

- Output format: players sorted from **highest to lowest score**

---

## 🧪 Example

### Input

```
init,3
add,PlayerA,100
add,PlayerB,500
add,PlayerC,200
get_top
add,PlayerD,300
get_top
add,PlayerE,50
```

### Output

```
PlayerB:500, PlayerC:200, PlayerA:100
PlayerB:500, PlayerD:300, PlayerC:200
```
