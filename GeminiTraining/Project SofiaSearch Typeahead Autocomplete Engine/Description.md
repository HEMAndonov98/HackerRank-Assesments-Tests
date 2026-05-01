# Project SofiaSearch: Typeahead Autocomplete Engine

## Problem Description

You are building the backend for a real-time search bar autocomplete feature. As users type characters, your engine must instantly return the most popular search terms that start with those characters.

> **⚠️ Critical:** Because this runs on every single keystroke, speed is critical. Scanning a flat list of strings is too slow.

---

## Requirements

### 1. Add Term

```
add_term,<term>,<score>
```

Inserts a search term into your engine with an integer popularity score.

- If the term already exists, update its score to the new value.
- Terms will only contain lowercase English letters (`a-z`).

---

### 2. Get Top Matches

```
get_top,<prefix>,<limit>
```

Finds all terms that start with the exact `<prefix>`.

- Returns a comma-separated list of up to `<limit>` terms.
- **Sorting Rule 1:** Highest score first.
- **Sorting Rule 2:** If scores are tied, sort alphabetically.
- If no terms match the prefix, return `NO_MATCH`.

> **⏱️ Time Complexity Constraint:** Finding the prefix must be **O(P)** where **P** is the length of the prefix. You cannot iterate through all words in the system.

---

### 3. Remove Term

```
remove_term,<term>
```

Removes a specific term from the engine.

> **🔴 CRITICAL:** To save memory, your data structure must not leave "dead" branches behind. If removing a term means certain pathways in your structure are no longer used by any other word, those pathways must be pruned/deleted.

---

## Example Input & Output

```plaintext
add_term,sofia,100
add_term,software,200
add_term,soft,50
add_term,sonar,150
get_top,sof,3
```

**Output:**

```
software,sofia,soft
```

_(Sorted by score desc)_

---

```plaintext
get_top,so,2
```

**Output:**

```
software,sonar
```

---

```plaintext
add_term,soap,150
get_top,so,5
```

**Output:**

```
software,soap,sonar,sofia,soft
```

_(soap and sonar tied at 150, soap is alphabetically first)_

---

```plaintext
remove_term,software
get_top,sof,3
```

**Output:**

```
sofia,soft
```

---

```plaintext
get_top,z,1
```

**Output:**

```
NO_MATCH
```

---

## Constraints

| Constraint    | Description                            |
| ------------- | -------------------------------------- |
| Term Length   | 1 ≤ length ≤ 100                       |
| Score Range   | 0 ≤ score ≤ 10⁹                        |
| Operations    | Up to 10⁵ operations                   |
| Character Set | Lowercase English letters only (`a-z`) |

---

## Evaluation Criteria

1. **Correctness** - All operations must produce the correct output
2. **Time Complexity** - Prefix lookup must be O(P)
3. **Memory Efficiency** - No dead branches after removal
4. **Edge Cases** - Handle empty results, duplicate terms, and removal of non-existent terms
