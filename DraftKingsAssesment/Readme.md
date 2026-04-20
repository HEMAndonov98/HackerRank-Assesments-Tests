# What I Learned

_This assesment by DraftKings helped me shift my mindset from the usual developer flow of using databases, balancing relationships and working with ORM's_

## The Task

This program manages the Organizational Structure of a company. The program should allow us to:

- Add employees.
- Generate the Org Chart in a specific format.
- Remove employees.
- Move an employee from one manager to another.
- Count the total number of employees that ultimately report up to a given manager.

#### Your Job:

Implement the Add(), Print(), Remove(), Move(), Count() methods.

### My Original Approach or What I Did Correctly and What I Missed

_Doing these challenges and assesments especially with a time limit and having no one to actually discuss the problem with, as well as not having access to an actual IDE(No debugging ;( ) can be dishartening, so I want to share what my though process was doing the challenge blind and what I missed(learned)._

#### The Right

##### The Object (Entity)

**Early on I correctly assesed that we need to use a POCO so we can store the actual data for an employee, such as `{ Id, Name, ManagerId }`**

##### The Store ("Database")

**As a subsitute for an actual db and to solve the lookup time I used the classic `Dictionary<int, Employee>`**

#### The Wrong

##### The Object

_Because of a confusion about what a report is or reporting level, and because I've gotten to used to databases and this seemed like a redundant opperation in Real-World Development I missed to add a single simple property to my Employee object_

- `List<Employee> Report` | This is the internal store of the object of which employees this employee manages, -1 if he is top-level

##### The Store

_I correctly assesed the use of a dictionary but because of my previous miss I created a very complex "Tree-like structure using two seperate dictionaries"_

- `Dictionary<int, Employee> AllEmployees`
- `Dictionary<int, Employee> ManagerEmployee` | this would resemble a database like structure, the problem is maintaining relationships properly
- Because dictionaries do not keep order I needed an ordered store to keep track of addition order for employees `List<Employee>` rootLevel or topLevelManagers

---

From having the wrong data structure the whole problem became a messy complex job of correctly maintaining relationships and using complex and downwright wrong logic for all the methods

### Key Takeaway

**Keywords that could've helped me:**

- reports will show up in the same order as they did under their former manager: clear indication of ordering `List`
- if the employee had any reports, those reports should now report to the removed employee's manager: shows that an internal store of reports or subordinates would've helped me tremendously: `public List<Employee> Reports` inside Employee object
- Return the total number of reports, including any indirect reports: Indicates clear recursion(most often DFS)`foreach report call Count recursively and add the current employee to the total count`
- Employees are always displayed in the order they were processed (insertion order): Clear display that I had to use a `List` to keep track of added users

**In conculusion using lists would eliminate the problem with keeping track of operation order also relationship management becomes easy because `Employee` is referenced in the local storage so change to one entity changes the whole "database"**
