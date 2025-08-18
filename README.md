# MySimpleDictionary

This is an implementation of a simple hash map (dictionary) in C# using open addressing with linear probing for collision resolution. It demonstrates core dictionary functionalities and includes performance benchmarks comparing it to the built-in .NET `Dictionary<TKey, TValue>`.

---

## Project Structure

This solution consists of three main projects:

- **MySimpleDictionary (Class Library)**  
  Contains the implementation of the `MySimpleDictionary<TKey, TValue>` class.  
  This project can be referenced by other applications to use the dictionary.

- **BenchmarkTests (Console Application)**  
  A console app that references `MySimpleDictionary` and runs functional tests and performance benchmarks.  
  It compares the performance of `MySimpleDictionary` against the built-in .NET `Dictionary<TKey, TValue>`.  
  The results are printed to the console.

- **BlazorServer (Blazor Server Application)**  
  A Blazor Server app that provides a web interface for adding, displaying, and managing dictionary entries.
  Uses MudBlazor components for modern UI design.

  Implements two main pages:

  / — Page for adding new key-value pairs to the shared dictionary.

  /display — Page for viewing all entries, removing specific entries, and clearing the dictionary.
---

## How to Run

1. Build the solution so that the `BenchmarkTests` project references and uses the `MySimpleDictionary` class library.  
2. Run the `BenchmarkTests` console application.  
3. Observe the console output for test results and benchmark comparisons.
4. Build the solution so that the `BlazorServer` project also references the `MySimpleDictionary` class library.
5. Run the Blazor Server application

## Implementation Details

- **Internal Structure:**  
  Uses an array of `Entry` structs, each storing a key, value, and state (`Empty`, `Occupied`, `Deleted`).

- **Collision Handling:**  
  Employs linear probing to find the next available slot upon collision.

- **Rehashing:**  
  Triggered when the number of deleted entries exceeds a threshold (currently set to 1/20 of the capacity).  
  Rehashing doubles the table size and reinserts all existing entries into the new array.

- **Key Methods:**  
  - `Add` — Adds a new key-value pair  
  - `Remove` — Marks a key as deleted  
  - `ContainsKey` and `ContainsValue` — Check for existence of keys/values  
  - `Clear` — Removes all entries  
  - Indexer for accessing values by key  

---

## Functional Tests

The tests cover the following scenarios:

- Adding elements  
- Displaying all keys and values  
- Checking existence of keys and values (`ContainsKey`, `ContainsValue`)  
- Accessing values through the indexer  
- Iterating through all key-value pairs  
- Removing existing and non-existing keys  
- Adding a new element on a previously deleted slot  
- Clearing the dictionary  

---

## Benchmark Results

Performance comparison of `MySimpleDictionary` vs the built-in `Dictionary<TKey, TValue>` with 100,000 elements:

| Operation               | MySimpleDictionary            | Dictionary<TKey, TValue>       |
|-------------------------|------------------------------|-------------------------------|
| Add                     | 31.735 ms (317 ns/op)         | 19.260 ms (193 ns/op)          |
| ContainsKey Search      | 6.962 ms (70 ns/op)           | 1.038 ms (10 ns/op)            |
| Indexer Retrieval       | 7.398 ms (74 ns/op)           | 1.010 ms (10 ns/op)            |
| ContainsValue Search    | 31.736 ms (31.736 µs/op)      | 7.070 ms (7.070 µs/op)         |
| Remove 50,000 Entries   | 4687.752 ms (93.755 µs/op)    | 0.442 ms (9 ns/op)             |
| Clear                   | 0.098 ms (98 µs/op)           | 0.001 ms (0.8 µs/op)           |

- Multiple rehashes were triggered during removal due to the low rehash threshold (1/20).

---

## Blazor Server Application
The application contains two main pages:

1. **Add New Entry** – Page for entering new *key-value* pairs  
   Allows the user to enter a key and a value, and add them to the shared service (`DictionaryService`)
   
   ![Add New Entry](https://github.com/DajanaRadovic/BlazorServer/blob/main/PicturesBlazorService/AddNewEntry.png)

3. **Display Entries** – Page for viewing and managing the entered data  
   Displays a table with all added pairs, allowing deletion of individual entries or clearing the entire dictionary
   
   ![Display Entries](https://github.com/DajanaRadovic/BlazorServer/blob/main/PicturesBlazorService/DisplayEntries.png)

## Conclusion

- Performance is reasonable for small to medium datasets but slower than the built-in `Dictionary` especially in removal and search operations.  
- The low rehash threshold improves operation consistency at the cost of more frequent rehashing and increased CPU usage.  
- The standard .NET dictionary is highly optimized for performance and memory management.

---
