# Project SofiaBuild: CI/CD Pipeline Resolver

You are building the core dependency resolution engine for a new Continuous Integration (CI) system. Developers define tasks and declare dependencies between them. Your engine must figure out the correct order to execute these tasks and detect if a developer accidentally created an infinite loop (a cycle).

---

## Requirements

### 1. Register Task

```
register,<task_id>
```

Registers a new task in the pipeline.

- If the task already exists, ignore the command.

---

### 2. Add Dependency

```
depend,<target_task_id>,<prerequisite_task_id>
```

Declares that `<target_task_id>` cannot run until `<prerequisite_task_id>` has successfully completed.

- If either task does not exist, ignore the command.

---

### 3. Calculate Build Order

```
run_order
```

Outputs a comma-separated list of tasks in a valid execution order.

- A task can only be executed if all of its prerequisites have already been executed.
- If multiple valid orders exist, any valid topological order is acceptable.

> **CRITICAL:** If there is a circular dependency (e.g., A → B → C → A), the system cannot run. Output exactly: `CYCLE_DETECTED`

---

### 4. Calculate Blast Radius

```
blast_radius,<task_id>
```

If `<task_id>` fails, it will block any tasks that depend on it, which blocks tasks that depend on them, and so on.

- Output the total number of **other** tasks that will be blocked if this task fails.
- **Do not include the failed task itself** in the count.
- If the task does not exist, output `0`.

---

## Example Input & Output

```
register,Compile
register,Test
register,Deploy
depend,Test,Compile        // Test depends on Compile
depend,Deploy,Test         // Deploy depends on Test
run_order                  // Output: Compile,Test,Deploy
blast_radius,Compile       // Output: 2 (Test and Deploy are blocked)
```

```
register,Lint
depend,Compile,Lint
run_order                  // Output: Lint,Compile,Test,Deploy
```

```
depend,Lint,Deploy         // Creates a cycle! Lint needs Deploy, Deploy needs Lint.
run_order                  // Output: CYCLE_DETECTED
```
