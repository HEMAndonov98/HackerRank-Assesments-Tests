# Project SofiaConnect: Network Topology Manager

_You are tasked with building a management system for a software-defined network (SDN) used in a Sofia-based data center. The network consists of Routers and Connections._

---

## Requirements

### 1. Add Router

`add_router,<router_id>,<model_name>`

- Adds a router to the network.
- If a router ID already exists, ignore the command.

### 2. Connect Routers

`connect,<router_id_1>,<router_id_2>`

- Creates a bi-directional connection between two routers.
- If either ID doesn't exist, ignore the command.
- If they are already connected, do nothing.

### 3. Maintenance (Remove)

`maintenance,<router_id>`

- Removes a router from the network.
- **Crucial:** All active connections to this router must be severed. Other routers previously connected to it should no longer list it as a neighbor.

### 4. Trace Route (Connectivity)

`trace,<router_id_1>,<router_id_2>`

- Determine if there is any path (direct or indirect) between Router 1 and Router 2.
- **Output:** `Path Exists` or `No Path`

### 5. Network Stats

`stats,<router_id>`

- Output the total number of routers reachable from the given router (its "Network Island"), including itself.

## Example Input

```plaintext
add_router,1,Cisco-A
add_router,2,Juniper-B
add_router,3,Nokia-C
connect,1,2
trace,1,3        → Output: No Path
connect,2,3
trace,1,3        → Output: Path Exists
stats,1           → Output: 3
```
