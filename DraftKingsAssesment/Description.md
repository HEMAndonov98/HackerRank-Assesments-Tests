# Organizational Structure

This program manages the Organizational Structure of a company. The program should allow us to:

- Add employees.
- Generate the Org Chart in a specific format.
- Remove employees.
- Move an employee from one manager to another.
- Count the total number of employees that ultimately report up to a given manager.

## Input Format

The input is a series of commands. The first line contains an integer representing the number of lines of input to follow.

## Commands

### Adding an Employee

```
add,<employee id>,<name>,<manager id>
```

- If an employee with an ID has already been added, subsequent additions with the same ID are ignored.
- If an employee's manager has not already been added, the employee is considered to have no manager (use -1 to represent no manager).

### Moving an Employee

```
move,<employee id>,<new manager id>
```

- If the employee ID or the new manager ID does not exist, do nothing.
- The newly moved employee will be appended to the manager's list of reports.
- Subsequent adds will continue to be added to the end of the list of reports.

### Removing an Employee

```
remove,<employee id>
```

- If the ID does not exist, do nothing.
- After removal, if the employee had any reports, those reports should now report to the removed employee's manager.
- These reports will show up in the same order as they did under their former manager, but after any current reports of the new manager.
- If the removed employee had no manager, then the reports will have no manager either.

### Counting Employees

```
count,<employee id>
```

- Return the total number of reports, including any indirect reports (all descendant reports), that the specified employee has.

### Printing the Org Chart

```
print
```

- Each employee is printed as: `Name [employee_id]`
- At each reporting level, add 2 spaces preceding the name.
- Employees are always displayed in the order they were processed (insertion order).

## Example Output

```
Sharilyn Gruber [10]
  Denice Mattice [7]
    Lissette Gorney [34]
  Lawana Futrell [3]
    Lan Puls [5]
```
