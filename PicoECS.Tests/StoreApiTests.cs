using Xunit;

namespace PicoECS.Tests;

public class StoreApiTests
{
    // Example entity classes to be used in API tests
    public class Player : Entity
    {
        public string Name { get; set; } = string.Empty;
        public int Health { get; set; } = 100;
    }

    public class Transform : Entity
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class InventoryItem : Entity
    {
        public string ItemId { get; set; } = string.Empty;
    }

    public class Weapon : InventoryItem
    {
        public int Damage { get; set; }
    }
    

    [Fact]
    public void HowTo_InitializeStore_And_AddEntities()
    {
        var store = new EcStore();
        var player = new Player { Name = "Hero" };
        store.Add(player);

        Assert.NotEqual(0u, player.Id);
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void HowTo_Create_And_Manage_Hierarchies()
    {
        var store = new EcStore();
        var player = new Player { Name = "Hero" };
        var transform = new Transform { X = 10, Y = 20 };
        var sword = new InventoryItem { ItemId = "sword_01" };

        store.Add(player, transform, sword);

        var children = store.GetChildren<Entity>(player);
        var swordParent = store.GetParent<Player>(sword);

        Assert.Equal(2, children.Count);
        Assert.Contains(transform, children);
        Assert.Same(player, swordParent);
    }

    [Fact]
    public void HowTo_Query_Entities()
    {
        var store = new EcStore();
        var player1 = new Player { Name = "Player 1" };
        var player2 = new Player { Name = "Player 2" };
        store.Add(player1);
        store.Add(player2);
        
        var transform = new Transform { X = 5, Y = 5 };
        store.Add(player1, transform);

        var allPlayers = store.GetByType<Player>();
        Assert.Equal(2, allPlayers.Count());

        var firstTransform = store.GetFirst<Transform>();
        Assert.NotNull(firstTransform);

        var specificPlayer = store.Get<Player>(player2.Id);
        Assert.Same(player2, specificPlayer);

        var descendants = store.GetDescendants(player1);
        Assert.Single(descendants); 
    }

    [Fact]
    public void HowTo_Remove_Entities()
    {
        var store = new EcStore();
        var player = new Player();
        var transform = new Transform();
        var item = new InventoryItem();

        store.Add(player, transform);
        store.Add(transform, item);

        Assert.Equal(3, store.Count);

        store.Remove(player);

        Assert.Equal(0, store.Count);
        Assert.Null(store.Get<Player>(player.Id));
        Assert.Null(store.Get<Transform>(transform.Id));
        Assert.Null(store.Get<InventoryItem>(item.Id));
    }

    [Fact]
    public void HowTo_Query_Parent_And_Children_Relationships()
    {
        var store = new EcStore();
        var player = new Player { Name = "Hero" };
        var sword = new Weapon { ItemId = "sword_01", Damage = 50 };
        var shield = new InventoryItem { ItemId = "shield_01" };

        store.Add(player, sword, shield);

        Assert.Equal(2, store.GetChildCount(player));

        var allItems = store.GetChildren<InventoryItem>(player);
        Assert.Equal(2, allItems.Count);

        var parentOfSword = store.GetParent<Player>(sword);
        Assert.Same(player, parentOfSword);
    }

    [Fact]
    public void HowTo_Get_All_Entities()
    {
        var store = new EcStore();
        store.Add(new Player());
        store.Add(new Transform());
        store.Add(new InventoryItem());

        Assert.Equal(3, store.Count);

        var allEntities = store.GetAll();
        Assert.Equal(3, allEntities.Count);
    }

    [Fact]
    public void HowTo_Safely_Use_Store_Concurrently()
    {
        var store = new EcStore();
        
        Parallel.For(0, 100, i => 
        {
            var player = new Player { Name = $"Player {i}" };
            store.Add(player);
            
            var players = store.GetByType<Player>();
            Assert.NotEmpty(players);
        });

        Assert.Equal(100, store.Count);
    }

    [Fact]
    public void HowTo_GetAll_FilteringByMultipleTypes()
    {
        var store = new EcStore();
        store.Add(new Player { Name = "Hero" });
        store.Add(new Transform { X = 0, Y = 0 });
        store.Add(new Weapon { Damage = 10 });
        store.Add(new InventoryItem { ItemId = "Potion" });

        var typesToFetch = new Entity[] { new Player(), new Transform(), new Weapon() };
        var filteredEntities = store.GetAll(typesToFetch);
        
        Assert.Equal(3, filteredEntities.Count);
        Assert.Contains(filteredEntities, e => e is Player);
        Assert.Contains(filteredEntities, e => e is Transform);
        Assert.Contains(filteredEntities, e => e is Weapon);
        Assert.DoesNotContain(filteredEntities, e => e.GetType() == typeof(InventoryItem));
    }

    [Fact]
    public void HowTo_Clear_Store()
    {
        var store = new EcStore();
        store.Add(new Player());
        store.Add(new Transform());

        Assert.Equal(2, store.Count);

        store.Clear();

        Assert.Equal(0, store.Count);
        var allEntities = store.GetAll();
        Assert.Empty(allEntities);
    }

    [Fact]
    public void HowTo_Get_Descendants_Recursively()
    {
        var store = new EcStore();
        var grandparent = new Player { Name = "Grandparent" };
        var parent = new Transform { X = 1, Y = 1 };
        var child = new InventoryItem { ItemId = "Heirloom" };

        store.Add(grandparent, parent);
        store.Add(parent, child);

        var directChildren = store.GetChildren<Entity>(grandparent);
        Assert.Single(directChildren);
        Assert.Contains(parent, directChildren);

        var allDescendants = store.GetDescendants(grandparent);
        Assert.Equal(2, allDescendants.Count);
        Assert.Contains(parent, allDescendants);
        Assert.Contains(child, allDescendants);
    }

    [Fact]
    public void HowTo_Understand_ExactType_Vs_Polymorphism()
    {
        var store = new EcStore();
        var weapon = new Weapon { Damage = 50 };
        var basicItem = new InventoryItem { ItemId = "Apple" };

        store.Add(weapon);
        store.Add(basicItem);

        // 1. GetByType uses EXACT type matching for maximum performance (O(1) dictionary lookup).
        // It will NOT return derived types.
        var exactItems = store.GetByType<InventoryItem>();
        Assert.Single(exactItems); // Only the "Apple", not the Weapon
        
        var weapons = store.GetByType<Weapon>();
        Assert.Single(weapons);

        // 2. In contrast, relationship queries like GetChildren are polymorphic.
        var player = new Player();
        store.Add(player, weapon, basicItem);

        // Querying children by the base class (InventoryItem) returns BOTH the basic item and the derived Weapon.
        var childItems = store.GetChildren<InventoryItem>(player);
        Assert.Equal(2, childItems.Count);
    }

    [Fact]
    public void HowTo_Handle_Invalid_Relationships()
    {
        var store = new EcStore();
        var parent1 = new Player { Name = "Parent 1" };
        var parent2 = new Player { Name = "Parent 2" };
        var child = new Transform { X = 0, Y = 0 };

        store.Add(parent1, child);

        // An entity can only have one parent.
        // Attempting to add an existing child to a NEW parent throws an InvalidOperationException.
        Assert.Throws<InvalidOperationException>(() => store.Add(parent2, child));
    }

    [Fact]
    public void HowTo_Remove_Multiple_Entities_At_Once()
    {
        var store = new EcStore();
        var player1 = new Player();
        var player2 = new Player();
        var player3 = new Player();
        
        store.Add(player1);
        store.Add(player2);
        store.Add(player3);
        
        Assert.Equal(3, store.Count);

        // Remove accepts a params array, allowing you to efficiently batch-remove entities
        store.Remove(player1, player2);

        // Only player3 remains
        Assert.Equal(1, store.Count);
        Assert.NotNull(store.Get<Player>(player3.Id));
    }
}