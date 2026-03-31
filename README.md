# PicoECS

PicoECS is a fast, thread-safe, and simple way to store and find entities.

## PicoStore Interface

```csharp
public sealed class PicoStore
{
    public int Count { get; }
    
    // Lifecycle & Management
    public void Add(PicoEntity parent, params PicoEntity[] children);
    public void Remove(params PicoEntity[] entities);
    public void Clear();

    // Retrieval
    public T? Get<T>(uint id) where T : PicoEntity;
    public T? First<T>() where T : PicoEntity;
    public List<PicoEntity> All();
    public List<T> All<T>() where T : PicoEntity;

    // Iteration
    public void ForEach(Action<PicoEntity> action);
    public void ForEach<T>(Action<T> action) where T : PicoEntity;

    // Hierarchy & Navigation
    public PicoEntity? Parent(PicoEntity entity);
    public List<PicoEntity> Children(PicoEntity parent);
    public List<PicoEntity> Descendants(PicoEntity parent);
}
```

## Why PicoECS?

- **Entity-Relationships:** Easily link entities as parents and children. Removing a parent automatically cleans up all its descendants.
- **Thread-Safe:** Built-in support for multiple threads reading and writing at the same time using `ReaderWriterLockSlim`.
- **Simple API:** No complex setup. Just inherit from `PicoEntity` and start using the `PicoStore`.

## Examples

### 1. Define Your Entities

Entities are just classes that inherit from `PicoEntity`.

```csharp
public class Player : PicoEntity { public string Name { get; set; } = "Hero"; }
public class Position : PicoEntity { public float X, Y; }
public class Inventory : PicoEntity { }
public class Sword : PicoEntity { }
public class Shield : PicoEntity { }
```

### 2. Basic Usage

```csharp
using PicoECS;

// Create the store
var store = new PicoStore();

// Add entities and establish a hierarchy
var player = new Player();
var pos = new Position { X = 10, Y = 20 };

// 'pos' becomes a child of 'player' and both are added to the store
store.Add(player, pos);
```

### Creating a Hierarchy

PicoECS manages nested entity relationships. For example, a Player can own an Inventory that contains various Items:

```csharp
// create the store
var store = new PicoStore();

// initialize your entities
var player = new Player();
var inventory = new Inventory();
var position = new Position();
var sword = new Sword();
var shield = new Shield();

// add player to the store with inventory and position as children
store.Add(player, inventory, position);

// add sword and shield to the store with inventory as the parent
store.Add(inventory, sword, shield);
```
#### Created Hierarchy
```mermaid
graph TD
    Player[Player]
    Position[Position]
    Inventory[Inventory]
    Sword[Sword]
    Shield[Shield]

    Player --> Position
    Player --> Inventory
    Inventory --> Sword
    Inventory --> Shield
```
#### Querying the Hierarchy
```csharp
var player = store.First<Player>();
var inventory = store.Children(player).OfType<Inventory>().First();
var sword = store.Children(inventory).OfType<Sword>().First();

// Removing an entity recursively removes all of the entity's descendants
store.Remove(player); 
```

### 3. Using ForEach

You can quickly run code on every entity of a certain type without creating a new list. 

> **Note:** `ForEach<T>` uses **exact type matching** for maximum performance (O(1)). It will not include entities of derived types.

```csharp
// Run an action on every Position entity
store.ForEach<Position>(p => {
    p.X += 1.0f;
    Console.WriteLine($"Moved to {p.X}, {p.Y}");
});

// Or run it on every single entity in the store
store.ForEach(e => Console.WriteLine($"PicoEntity ID: {e.Id}"));
```

## Querying

### Get By ID
Retrieve a specific entity in O(1) time:
```csharp
var player = store.Get<Player>(player.Id);
```

### Generic Type Lookup
Get every entity of a specific type. Like `ForEach<T>`, this uses **exact type matching**.
```csharp
var allPlayers = store.All<Player>();
```

### Children (Polymorphic)
Retrieve the direct children of an entity. Unlike `All<T>`, relationship queries are **polymorphic** and will include derived types.
```csharp
// Will find both Sword and Shield items if they inherit from Item
var items = store.Children(inventory);
```

### Descendants
Get every entity nested under a parent, recursively:
```csharp
var allDescendants = store.Descendants(player);
```

## More Examples

For a full look at the API, check out the test suite:
👉 **[PicoECS.Tests/StoreApiTests.cs](./PicoECS.Tests/StoreApiTests.cs)**

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
