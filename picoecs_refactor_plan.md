# PicoECS Store API Refactor Plan

## Project Overview
Refactor the `PicoStore` class in the PicoECS project to provide a simpler, more intuitive, and less verbose public API. The goal is to reduce cognitive load by shortening method names and grouping related functionality logically in IntelliSense.

## Technical Stack
- **Language:** C# (.NET 8.0/10.0)
- **Testing:** xUnit
- **Benchmarks:** BenchmarkDotNet

## Phases of Development

### 1. Research & Discovery
- **API Mapping:** Create a definitive mapping of old method signatures to new ones.
- **Dependency Check:** Use `grep` to identify all call sites in `PicoECS.Tests` and `PicoECS.Benchmarks`.
- **Internal Audit:** Verify that `internal` methods (like `ensurePicoEntityIndexed` and `ForEachInternal`) do not share names with the new public API to avoid collision or confusion.

### 2. Architecture & Design
The refactor follows a "Verb-First" and "Property-Like" naming convention:
- **Retrieval:** Shortened to core action (`Get`, `All`, `First`).
- **Hierarchy:** Named after the relationship (`Children`, `Parent`, `Descendants`, `ChildCount`).
- **Removal of Redundancy:** Eliminate overloads that can be achieved via LINQ or basic generic methods (e.g., `params Type[]` and `ICollection` filling).
- **Consistency:** Ensure all generic methods follow the `T? Method<T>(...) where T : PicoEntity` pattern consistently.

### 3. Implementation Steps

#### Task 1: Refactor Retrieval Methods
- **`GetById<T>(uint id)`** → **`Get<T>(uint id)`**
- **`GetFirst<T>()`** → **`First<T>()`**
- **`GetAll<T>()`** → **`All<T>()`**
- **`GetAll()`** → **`All()`**
- **Removal:** Delete `GetAll(params Type[] types)` and `GetAll(ICollection<PicoEntity> result)`.

#### Task 2: Refactor Hierarchy & Navigation Methods
- **`GetParent<T>(PicoEntity entity)`** → **`Parent<T>(PicoEntity entity)`**
- **`GetChildren<T>(PicoEntity parent)`** → **`Children<T>(PicoEntity parent)`**
- **`GetChildren(PicoEntity parent)`** → **`Children(PicoEntity parent)`**
- **`GetDescendants(PicoEntity parent)`** → **`Descendants(PicoEntity parent)`**
- **`GetChildCount(PicoEntity parent)`** → **`ChildCount(PicoEntity parent)`**

#### Task 3: Standardize Iteration & Lifecycle
- Ensure `ForEach<T>(Action<T> action)` and `ForEach(Action<PicoEntity> action)` remain the primary high-performance iteration paths.
- Ensure `Add(PicoEntity entity, params PicoEntity[] children)` remains the primary entry point for entities.

#### Task 4: Cascade Changes to Tests & Benchmarks
- Update `PicoStoreTests.cs`.
- Update `StoreApiTests.cs`.
- Update `StoreBenchmarks.cs`.
- Refactor any helper methods in tests that rely on the old naming conventions.

### 4. Testing & Quality Assurance
- **Build Validation:** Run `dotnet build` on the solution to catch any missed call sites.
- **Test Suite Execution:** Run `dotnet test` to confirm all 100+ tests pass with the new API.
- **Benchmark Verification:** Run `dotnet run -c Release --project PicoECS.Benchmarks` (subset if necessary) to ensure that shortening names didn't inadvertently change the IL/performance characteristics.

## Potential Challenges
- **Breaking Changes:** This is a major breaking change; any external consumers would need to update their code.
- **Naming Collisions:** "Parent" and "Children" are common names; ensure they don't conflict with local variables in common usage patterns.
- **Internal Maintenance:** Ensure the `ReaderWriteLockSlim` logic remains correctly wrapped around the new method bodies.
