# LRU Cache Implementation

## Difficulty: Medium | Doubly Linked Lists

---

## Problem Description

You need to implement a **Least Recently Used (LRU) Cache** data structure that supports the following operations in **O(1)** time complexity:

1. **Initialize** - Set the maximum capacity of the cache
2. **Put** - Add or update a key-value pair
3. **Get** - Retrieve a value by key
4. **Status** - Display all keys in MRU to LRU order

An LRU cache evicts the least recently used item when full and a new item is added.

---

## Data Structure Requirement

**You must implement this using a Doubly Linked List** to achieve O(1) time complexity for all operations.

A Doubly Linked List allows you to:

- Move nodes to the front (most recently used) in O(1)
- Remove nodes from the back (least recently used) in O(1)
- Update existing nodes in O(1)

---

## Command Format

| Command    | Format              | Description                                          |
| ---------- | ------------------- | ---------------------------------------------------- |
| Initialize | `init,<capacity>`   | Creates cache with given capacity (positive integer) |
| Put        | `put,<key>,<value>` | Adds or updates a key-value pair                     |
| Get        | `get,<key>`         | Returns value for key, or `NOT_FOUND`                |
| Status     | `status`            | Returns all keys from MRU to LRU, or `EMPTY`         |

---

## Rules

1. **Put Operation**:
    - If key exists → update value and move to MRU position
    - If key doesn't exist → add new node
    - If cache is full → evict LRU item before adding

2. **Get Operation**:
    - If key exists → return value and move to MRU position
    - If key doesn't exist → return `NOT_FOUND`

3. **Accessing a key (via get or put) makes it the most recently used**

---

## Input Format

- First line: integer `n` - number of commands
- Next `n` lines: each contains one command

```
5
init,3
put,A,100
get,A
put,B,200
status
```

---

## Output Format

- Each command that requires output produces one line of output
- `get` → outputs the value or `NOT_FOUND`
- `status` → outputs comma-separated keys (MRU→LRU) or `EMPTY`

```
100
B,A
```

---

## Constraints

- `1 ≤ capacity ≤ 1000`
- `key` and `value` are non-empty strings (no commas)
- Number of commands: `1 ≤ n ≤ 10^5`
- All operations must be O(1)

---

## Sample Test Case

**Input:**

```
9
init,3
put,A,100
put,B,200
put,C,300
status
get,B
status
put,D,400
status
get,A
```

**Expected Output:**

```
C,B,A
200
B,C,A
D,B,C
NOT_FOUND
```

**Explanation:**

- After `put,A,100`, `put,B,200`, `put,C,300`: order is C→B→A
- `get,B` returns 200 and moves B to front: B→C→A
- `put,D,400` evicts A (LRU) since cache is full: D→B→C
- `get,A` returns NOT_FOUND (A was evicted)

---

## Implementation Notes

- Use a HashMap/Dictionary for O(1) key lookups
- Combine with Doubly Linked List for O(1) node movement
- Maintain head as Most Recently Used (MRU)
- Maintain tail as Least Recently Used (LRU)
