# PicoECS Development Plan: Go to C# Port

This plan outlines the process of converting a Go-based Entity Component System (ECS) store into a thread-safe C# library.

## Project Overview
The goal is to provide a lightweight, high-performance ECS store in C# that supports hierarchical entity relationships (parent/child), type-based querying, and thread-safe operations. The C# implementation will leverage .NET 8.0 features such as generics, modern collections, and advanced concurrency primitives.

## Technical Stack
- **Language:** C# 12
- **Framework:** .NET 8.0
- **Concurrency:** `System.Threading.ReaderWriterLockSlim`
- **Testing:** xUnit
- **ID Generation:** `System.Security.Cryptography.RandomNumberGenerator`

## Phases of Development

### 1. Research & Discovery
- **Interface Mapping:** Map Go's `IEntity` and `iEntityInternal` to C# interfaces. C# can use `internal interface` to replicate Go's unexported interface behavior within the assembly.
- **Concurrency Model:** Map Go's `sync.RWMutex` to C#'s `ReaderWriterLockSlim`.
- **Type System:** Map Go's `reflect.TypeOf().String()` lookups to C#'s `typeof(T)` or `entity.GetType()`.
- **Iterators:** Map Go's `iter.Seq` to C#'s `IEnumerable<T>` or `yield return` patterns.

### 2. Architecture & Design
- **Core Types:**
    - `EntityId`: A `readonly record struct` or `uint` alias for type-safe IDs.
    - `IEntity`: Public interface with a `uint Id { get; }` property.
    - `IInternalEntity`: Internal interface for `SetId`, `ParentId`, and `ChildIds` management.
- **Store Structure:**
    - `EcStore`: Main class containing:
        - `Dictionary<Type, List<IEntity>> _lists`
        - `Dictionary<uint, IEntity> _idIndex`
        - `ReaderWriterLockSlim _lock`
- **Error Handling:** Define custom exceptions (e.g., `EntityNotFoundException`, `DuplicateParentException`) to replace Go's `errors.New`.

### 3. Implementation Steps

#### Task 1: Basic Infrastructure
- Define `IEntity` and the base `Entity` class.
- Implement the internal interface for state management.
- Set up the `EcStore` class skeleton with thread-safe locking.

#### Task 2: ID Generation & Basic Operations
- Implement `NewId` using `RandomNumberGenerator`.
- Implement `AddEntity` (the most complex logic in the Go source).
- Implement `GetById<T>` and `GetFirst<T>` using C# generics.

#### Task 3: Hierarchical Management
- Implement `GetParent<T>`, `GetChildren<T>`, and `ChildCount<T>`.
- Implement `GetDescendants` using an iterative approach (as seen in the Go code).
- Implement `SetEntityId` and ensure internal links are correctly updated.

#### Task 4: Querying & Iteration
- Implement `GetAll` and `IterAll`.
- Implement `Count` and `CountType`.
- Ensure all queries respect the `Reader` lock.

#### Task 5: Cleanup & Removal
- Implement the `Remove` logic, including recursive descendant removal.
- Implement `Clear`.

### 4. Testing & Quality Assurance
- **Unit Tests:**
    - Entity creation and ID assignment.
    - Parent/child relationship integrity.
    - Concurrent read/write stress tests.
    - Type-based filtering and generic retrieval.
    - Descendant removal logic.
- **Benchmarks:** Compare performance of type lookups vs. Go implementation.

## Potential Challenges
- **Internal Access:** Ensuring the `IInternalEntity` methods remain hidden from the public API while accessible to the `EcStore`.
- **Performance:** C# `Dictionary` lookups by `Type` are generally fast, but we should verify if `_lists` management becomes a bottleneck during frequent Add/Remove operations.
- **Reference Equality:** In Go, pointers are used; in C#, we'll use reference types. We must ensure `_idIndex` and `_lists` stay synchronized.

## Clarifications Needed
1. **ID Type:** Should we stick to `uint32` (C# `uint`) or move to `ulong` for a larger ID space?
2. **Base Class vs Interface:** Do you prefer a mandatory base `Entity` class, or should the library support any class implementing `IEntity`? (The Go code suggests a base-like structure but uses interfaces for flexibility).
